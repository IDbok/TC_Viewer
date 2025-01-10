using System.IO;
using static TC_WinForms.DataProcessing.AuthorizationService;
using static TC_WinForms.DataProcessing.AuthorizationService.User;

namespace TC_WinForms.DataProcessing;

public static class ApplicationInfoService
{
    public static string GetApplicationInfo()
    {
        var message = "Программа предназначена для создания и редактирования технологических карт.\n" +
                      $"Версия программы: {GetApplicationVersion()}\n";

        if (AuthorizationService.CurrentUser.UserRole() == Role.Lead
            || AuthorizationService.CurrentUser.UserRole() == Role.Implementer
            )
        {

            string serverName = ApplicationInfoService.GetServerName();
            if (!string.IsNullOrEmpty(serverName))
            {
                message += "Сервер: " + serverName + "\n";
            }

            string databaseName = ApplicationInfoService.GetDatabaseName();
            if (!string.IsNullOrEmpty(databaseName))
            {
                message += "База данных: " + databaseName + "\n";
            }
        }

        string userRole = ApplicationInfoService.GetCurrentUserRole();
        if (AuthorizationService.CurrentUser.UserRole() != Role.User)
            if (!string.IsNullOrEmpty(userRole))
            {
                message += "Права доступа: " + userRole + "\n";
            }

        string tempPath = Path.GetTempPath();
        if (!string.IsNullOrEmpty(tempPath))
        {
            message += "Путь к временным файлам: " + tempPath + "\n";
        }

        message += "\n";
        message += "Разработчик: ООО \"Таврида Электрик\"";

        return message;
    }
    public static string GetApplicationVersion()
    {
        return "1.8.4"; 
    }

    public static string GetApplicationName()
    {
        return "TC_WinForms";
    }

    public static string GetCurrentUserRole()
    {
        User.Role userRole = AuthorizationService.CurrentUser.UserRole();

        return AuthorizationService.UserRoleConverter(userRole);
    }

    public static string GetServerName()
    {
        string connectionString = TcDbConnector.StaticClass.ConnectString;

        ExtractServerAndDatabase(connectionString, out string server, out _);

        return server;
    }

    public static string GetDatabaseName()
    {
        string connectionString = TcDbConnector.StaticClass.ConnectString;

        ExtractServerAndDatabase(connectionString, out _, out string database);

        return database;
    }

    static void ExtractServerAndDatabase(string connectionString, 
        out string server, out string database)
    {
        server = null;
        database = null;

        string[] parameters = connectionString.Split(';');

        foreach (var param in parameters)
        {
            string[] keyValue = param.Split('=');
            if (keyValue.Length == 2)
            {
                if (keyValue[0].Trim().ToLower() == "server")
                {
                    server = keyValue[1].Trim();
                }
                else if (keyValue[0].Trim().ToLower() == "database")
                {
                    database = keyValue[1].Trim();
                }
            }
        }
    }
}
