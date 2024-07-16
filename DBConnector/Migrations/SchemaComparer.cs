using MySqlConnector;
using System.Data;

namespace TcDbConnector.Migrations;

public class SchemaComparer
{
    static string _oldConnectionString = "OldDbConnectionString";
    static string _newConnectionString = "NewDbConnectionString";
    public static void Compare(string oldConnectionString, string newConnectionString)
    {
        _oldConnectionString = oldConnectionString;
        _newConnectionString = newConnectionString;

        var oldSchema = GetSchema(oldConnectionString);
        var newSchema = GetSchema(newConnectionString);

        CompareSchemas(oldSchema, newSchema);
    }

    static DataTable GetSchema(string connectionString)
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            return connection.GetSchema("Tables");
        }
    }
    static DataTable GetColumns(string connectionString, string tableName)
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            connection.Open();
            return connection.GetSchema("Columns", new[] { null, null, tableName, null });
        }
    }
    static void CompareSchemas(DataTable oldSchema, DataTable newSchema)
    {
        foreach (DataRow row in oldSchema.Rows)
        {
            var tableName = row["TABLE_NAME"].ToString();
            var newRow = newSchema.Select($"TABLE_NAME = '{tableName}'");

            if (newRow.Length == 0)
            {
                Console.WriteLine($"Table {tableName} is missing in the new schema.");
            }
            else
            {
                // Compare columns
                var oldColumns = GetColumns(_oldConnectionString, tableName);
                var newColumns = GetColumns(_newConnectionString, tableName);

                CompareColumns(tableName, oldColumns, newColumns);
            }
        }

        // Check for tables present in the new schema but missing in the old schema
        foreach (DataRow row in newSchema.Rows)
        {
            var tableName = row["TABLE_NAME"].ToString();
            var oldRow = oldSchema.Select($"TABLE_NAME = '{tableName}'");

            if (oldRow.Length == 0)
            {
                Console.WriteLine($"Table {tableName} is new in the new schema.");
            }
        }
    }
    static void CompareColumns(string tableName, DataTable oldColumns, DataTable newColumns)
    {
        foreach (DataRow oldColumn in oldColumns.Rows)
        {
            var columnName = oldColumn["COLUMN_NAME"].ToString();
            var newColumn = newColumns.Select($"COLUMN_NAME = '{columnName}'");

            if (newColumn.Length == 0)
            {
                Console.WriteLine($"Column {columnName} in table {tableName} is missing in the new schema.");
            }
            else
            {
                // Compare other properties if necessary
            }
        }

        // Check for columns present in the new schema but missing in the old schema
        foreach (DataRow newColumn in newColumns.Rows)
        {
            var columnName = newColumn["COLUMN_NAME"].ToString();
            var oldColumn = oldColumns.Select($"COLUMN_NAME = '{columnName}'");

            if (oldColumn.Length == 0)
            {
                Console.WriteLine($"Column {columnName} in table {tableName} is new in the new schema.");
            }
        }
    }
}
public class ConnectionTester
{
    public static void TestConnection(string connectionString)
    {
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Connection successful!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Connection failed: {ex.Message}");
        }
    }
}
