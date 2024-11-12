//using Serilog;

//namespace TC_WinForms.Services.Logging;

//public class Logger : ILogger
//{
//    private readonly Serilog.ILogger _logger;

//    public Logger()
//    {
//        _logger = new LoggerConfiguration()
//            .WriteTo.Console()
//            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
//            .CreateLogger();
//    }

//    public void LogInformation(string message)
//    {
//        _logger.Information(message);
//    }

//    public void LogWarning(string message)
//    {
//        _logger.Warning(message);
//    }

//    public void LogError(string message, Exception ex)
//    {
//        _logger.Error(ex, message);
//    }
//}
