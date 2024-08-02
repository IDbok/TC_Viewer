namespace TestConsoleAppTC;

//using MySql.Data.MySqlClient; // todo: fix it
using MySqlConnector;
using System;
using System.Data;

public class DatabaseCopier
{
    private string sourceConnectionString;
    private string destinationConnectionString;

    public DatabaseCopier(string sourceConnectionString, string destinationConnectionString)
    {
        this.sourceConnectionString = sourceConnectionString;
        this.destinationConnectionString = destinationConnectionString;
    }

    public void CopyDatabase(string sourceDatabaseName, string destinationDatabaseName)
    {
        using (var sourceConnection = new MySqlConnection(sourceConnectionString))
        using (var destinationConnection = new MySqlConnection(destinationConnectionString))
        {
            sourceConnection.Open();
            destinationConnection.Open();

            // Create or recreate the destination database
            using (var createDbCmd = new MySqlCommand($"DROP DATABASE IF EXISTS `{destinationDatabaseName}`; CREATE DATABASE `{destinationDatabaseName}`;", destinationConnection))
            {
                createDbCmd.ExecuteNonQuery();
            }

            // Get the list of tables from the source database
            sourceConnection.ChangeDatabase(sourceDatabaseName);
            var tables = GetTables(sourceConnection);

            foreach (var table in tables)
            {
                // Create table in destination database
                var createTableScript = GetCreateTableScript(sourceConnection, table);
                using (var createTableCmd = new MySqlCommand(createTableScript, destinationConnection))
                {
                    destinationConnection.ChangeDatabase(destinationDatabaseName);
                    createTableCmd.ExecuteNonQuery();
                }

                // Copy data from source to destination
                var data = GetTableData(sourceConnection, table);
                BulkInsertData(destinationConnection, destinationDatabaseName, table, data);
            }
        }
    }

    private List<string> GetTables(MySqlConnection connection)
    {
        var tables = new List<string>();
        using (var cmd = new MySqlCommand("SHOW TABLES;", connection))
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                tables.Add(reader.GetString(0));
            }
        }
        return tables;
    }

    private string GetCreateTableScript(MySqlConnection connection, string tableName)
    {
        using (var cmd = new MySqlCommand($"SHOW CREATE TABLE `{tableName}`;", connection))
        using (var reader = cmd.ExecuteReader())
        {
            if (reader.Read())
            {
                return reader.GetString(1);
            }
        }
        throw new Exception($"Failed to get CREATE TABLE script for table `{tableName}`.");
    }

    private DataTable GetTableData(MySqlConnection connection, string tableName)
    {
        var dataTable = new DataTable();
        using (var cmd = new MySqlCommand($"SELECT * FROM `{tableName}`;", connection))
        using (var adapter = new MySqlDataAdapter(cmd))
        {
            adapter.Fill(dataTable);
        }
        return dataTable;
    }

    private void BulkInsertData(MySqlConnection connection, string databaseName, string tableName, DataTable data)
    {
        // todo: fix it
        //connection.ChangeDatabase(databaseName);
        //using (
        //    var bulkCopy = new MySqlBulkLoader(connection)
        //{
        //    TableName = tableName,
        //    FieldTerminator = "\t",
        //    LineTerminator = "\n",
        //    FileName = "",
        //    NumberOfLinesToSkip = 0,
        //    Local = true
        //}
        //    )
        //{
        //    // Create a temporary CSV file
        //    var tempFile = Path.GetTempFileName();
        //    try
        //    {
        //        WriteDataTableToCsv(data, tempFile);
        //        bulkCopy.FileName = tempFile;
        //        bulkCopy.Load();
        //    }
        //    finally
        //    {
        //        File.Delete(tempFile);
        //    }
        //}
    }

    private void WriteDataTableToCsv(DataTable table, string filePath)
    {
        using (var writer = new StreamWriter(filePath))
        {
            foreach (DataRow row in table.Rows)
            {
                var fields = row.ItemArray.Select(field => field.ToString().Replace("\t", " ")).ToArray();
                writer.WriteLine(string.Join("\t", fields));
            }
        }
    }
}

