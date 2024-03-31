using System;
using System.IO;

public static class Logger
{
    private static readonly string logFilePath;

    static Logger()
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string logFileName = $"Log_{timestamp}.txt";

        string logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        logFilePath = Path.Combine(logDirectory, logFileName);
    }

    public static void Log(string message)
    {
        try
        {
            string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\n";
            File.AppendAllText(logFilePath, logMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to write to log: {ex.Message}");
        }
    }
}
