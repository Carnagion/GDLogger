using System;
using System.Collections.Generic;

namespace Godot
{
    /// <summary>
    /// Handles the logging of messages and <see cref="Exception"/>s to a log file.
    /// </summary>
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

        /// <summary>
        /// Gets or sets the file path to which log entries are written.
        /// </summary>
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
        
        /// <summary>
        /// Writes the text representation of <paramref name="entry"/> to the log file. Also writes to the console in debug mode.
        /// </summary>
        /// <param name="entry">The <see cref="Entry"/> to write.</param>
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

        /// <summary>
        /// Writes <paramref name="message"/> to the log file, encoding it as a notification.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public static void Notification(string message)
        {
            Log.Write(new(message, Entry.MessageSeverity.Notification));
        }

        /// <summary>
        /// Writes <paramref name="message"/> to the log file, encoding it as a warning.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public static void Warning(string message)
        {
            Log.Write(new(message, Entry.MessageSeverity.Warning));
        }

        /// <summary>
        /// Writes the text representation of <paramref name="exception"/> to the log file, encoding it as a warning.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> to write.</param>
        public static void Warning(Exception exception)
        {
            Log.Write(new(exception.ToString(), Entry.MessageSeverity.Warning));
        }

        /// <summary>
        /// Writes <paramref name="message"/> to the log file, encoding it as an error.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public static void Error(string message)
        {
            Log.Write(new(message, Entry.MessageSeverity.Error));
        }

        /// <summary>
        /// Writes the text representation of <paramref name="exception"/> to the log file, encoding it as an error.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> to write.</param>
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

        /// <summary>
        /// Represents a log entry.
        /// </summary>
        public sealed record Entry
        {
            /// <summary>
            /// Initialises a new <see cref="Entry"/> with the specified parameters.
            /// </summary>
            /// <param name="message">The message to include in the <see cref="Entry"/>.</param>
            /// <param name="severity">The <see cref="Entry"/>'s severity level.</param>
            public Entry(string message, MessageSeverity severity)
            {
                this.Message = message.Trim();
                this.Severity = severity;
                this.Timestamp = DateTime.Now;
            }
            
            /// <summary>
            /// The message of the <see cref="Entry"/>.
            /// </summary>
            public string Message
            {
                get;
            }

            /// <summary>
            /// The time when the <see cref="Entry"/> was created.
            /// </summary>
            public DateTime Timestamp
            {
                get;
            }

            /// <summary>
            /// The severity level of the <see cref="Entry"/>.
            /// </summary>
            public MessageSeverity Severity
            {
                get;
            }

            /// <summary>
            /// Returns a <see cref="String"/> that represents the <see cref="Entry"/>.
            /// </summary>
            /// <returns>A <see cref="String"/> in the format "[Severity] at [Timestamp] - [Message]".</returns>
            public override string ToString()
            {
                return $"[{this.Severity}] at {this.Timestamp.Hour}:{this.Timestamp.Minute}:{this.Timestamp.Second}:{this.Timestamp.Millisecond} - {this.Message}";
            }

            /// <summary>
            /// Represents a log entry severity.
            /// </summary>
            public enum MessageSeverity
            {
                /// <summary>
                /// Miscellaneous information.
                /// </summary>
                Notification,
                /// <summary>
                /// Minor errors that can usually be recovered from.
                /// </summary>
                Warning,
                /// <summary>
                /// Major errors that usually stop the program.
                /// </summary>
                Error,
            }
        }
    }
}