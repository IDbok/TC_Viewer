using Serilog;

namespace TC_WinForms.Extensions;

public static class LoggingExtensions
{
    public static void LogUserAction(this ILogger logger, string actionDescription, int tcId)
    {
        logger.Information("Действие пользователя: {Action}, TcId={TcId}",
            actionDescription, tcId);
    }

    public static void LogUserAction(this ILogger logger, string actionDescription)
    {
        logger.Information("Действие пользователя: {Action}",
            actionDescription);
    }
}

