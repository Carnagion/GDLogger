using System;
using System.Collections.Generic;

namespace Godot
{
    public static class Log
    {
        static Log()
        {
            Log.FilePath = Log.defaultFilePath;
            
            AppDomain.CurrentDomain.UnhandledException += Log.OnUnhandledException;
            AppDomain.CurrentDomain.ProcessExit += Log.OnProcessExit;
        }

        private const string defaultFilePath = "user://Log.txt";
        
        private const int maxEntryCount = 100;

        private const int maxFlushIntervalSeconds = 60;

        private const int maxFlushIntervalMessages = 10;
        
        private static readonly Queue<Entry> entries = new(Log.maxEntryCount + 1);

        private static readonly File file = new();
        
        private static DateTime lastSynced;

        public static string FilePath
        {
            get
            {
                return Log.file.GetPathAbsolute();
            }
            set
            {
                // If a previous log file already exists, save its contents and close it
                if (Log.file.IsOpen())
                {
                    Log.Flush(true);
                    Log.file.Close();
                }
                Log.file.Open(value, File.ModeFlags.Write);
            }
        }
        
        public static void Write(Entry entry)
        {
#if DEBUG
            GD.Print(entry);
#endif
            Log.file.StoreLine(entry.ToString());
            if (Log.entries.Count is Log.maxEntryCount)
            {
                Log.entries.Dequeue();
            }
            Log.entries.Enqueue(entry);
            Log.Flush();
        }

        public static void Notification(string message)
        {
            Log.Write(new(message, Entry.MessageSeverity.Notification));
        }

        public static void Warning(string message)
        {
            Log.Write(new(message, Entry.MessageSeverity.Warning));
        }

        public static void Warning(Exception exception)
        {
            Log.Write(new(exception.ToString(), Entry.MessageSeverity.Warning));
        }

        public static void Error(string message)
        {
            Log.Write(new(message, Entry.MessageSeverity.Error));
        }

        public static void Error(Exception exception)
        {
            Log.Write(new(exception.ToString(), Entry.MessageSeverity.Error));
        }

        private static void Flush(bool force = false)
        {
            DateTime now = DateTime.Now;
            if (!force && ((now - Log.lastSynced).TotalSeconds < Log.maxFlushIntervalSeconds) && (Log.entries.Count < Log.maxFlushIntervalMessages))
            {
                return;
            }
            
            // If it has been 60 seconds since the last flush, or if there are 10 or more entries in the queue, flush immediately
            Log.entries.Clear();
            Log.file.Flush();
            Log.lastSynced = now;
        }

        private static void OnUnhandledException(object source, UnhandledExceptionEventArgs arguments)
        {
            Log.Write(new(arguments.ExceptionObject.ToString(), Entry.MessageSeverity.Error));
            if (!arguments.IsTerminating)
            {
                return;
            }
            
            Log.file.Close();
            Log.file.Dispose();
            AppDomain.CurrentDomain.ProcessExit -= Log.OnProcessExit;
        }

        private static void OnProcessExit(object source, EventArgs arguments)
        {
            if (Log.file.IsOpen())
            {
                Log.file.Close();
            }
            Log.file.Dispose();
        }

        public sealed record Entry
        {
            public Entry(string message, MessageSeverity severity)
            {
                this.Message = message.Trim();
                this.Severity = severity;
                this.Timestamp = DateTime.Now;
            }
            
            public string Message
            {
                get;
            }

            public DateTime Timestamp
            {
                get;
            }

            public MessageSeverity Severity
            {
                get;
            }

            public override string ToString()
            {
                return $"[{this.Severity}] at {this.Timestamp.Hour}:{this.Timestamp.Minute}:{this.Timestamp.Second}:{this.Timestamp.Millisecond} - {this.Message}";
            }

            public enum MessageSeverity
            {
                Notification,
                Warning,
                Error,
            }
        }
    }
}