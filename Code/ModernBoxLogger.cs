using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public static class ModernBoxLogger
{
    private static readonly string version = "5.01";
    private static readonly string logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MBOX_LOGS");
    private static readonly string currentLogPath = Path.Combine(logFolder, "modernbox.log");
    private static readonly string previousLogPath = Path.Combine(logFolder, "modernbox_prev.log");

    private static int totalLogs = 0;
    private static int normalLogs = 0;
    private static int warningLogs = 0;
    private static int errorLogs = 0;

    private static List<string> logEntries = new List<string>();

    static ModernBoxLogger()
    {
        InitializeLogger();
    }

    private static void InitializeLogger()
    {
        try
        {
            Directory.CreateDirectory(logFolder);

            if (File.Exists(currentLogPath))
            {
                if (File.Exists(previousLogPath))
                    File.Delete(previousLogPath);

                File.Move(currentLogPath, previousLogPath);
            }

            WriteLogFile();
        }
        catch (Exception e)
        {
            Debug.LogError("[ModernBoxLogger] Failed to initialize logger: " + e);
        }
    }

    public static void Log(string message)
    {
        string finalMessage = $"[ModernBox] {message}";
        normalLogs++;
        totalLogs++;
        AppendEntry(finalMessage);
    }

    public static void Warning(string message)
    {
        string finalMessage = $"[ModernBox] WARNING: {message}";
        Debug.LogWarning(finalMessage);
        warningLogs++;
        totalLogs++;
        AppendEntry(finalMessage);
    }

    public static void Error(string message)
    {
        string finalMessage = $"[ModernBox] ERROR: {message}";
        Debug.LogError(finalMessage);
        errorLogs++;
        totalLogs++;
        AppendEntry(finalMessage);
    }

    private static void AppendEntry(string message)
    {
        string entry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
        logEntries.Add(entry);
        WriteLogFile();
    }

    private static void WriteLogFile()
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(currentLogPath, false))
            {
                writer.WriteLine("===============================================");
                writer.WriteLine($"==============MODERNBOX {version}==================");
                writer.WriteLine("==============BY TUXXEGO========================");
                writer.WriteLine("===============================================");
                writer.WriteLine("========INFO:===================================");
                writer.WriteLine($"Total logs: {totalLogs}");
                writer.WriteLine($"Normal Logs: {normalLogs}");
                writer.WriteLine($"Warnings: {warningLogs}");
                writer.WriteLine($"Errors: {errorLogs}");
                writer.WriteLine("===============================================");
                writer.WriteLine("BEGIN LOGGING:");
                writer.WriteLine("===============================================\n");

                foreach (string entry in logEntries)
                {
                    writer.WriteLine(entry);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[ModernBoxLogger] Failed to write to log file: " + e);
        }
    }
}
