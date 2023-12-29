using System;
using System.Text;
using System.Collections.Generic;

namespace Godot {
    /// <summary>
    /// Handles the logging of messages and <see cref="Exception"/>s to a log file.
    /// </summary>
    public static class Log {
        static Log() {
            Log.FilePath = Log.defaultFilePath;
            Log.AdditionalInfoFormat = string.Empty;
            
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
        /// <c>true</c>: Shows the type of log and the time the log was made.
        /// <para><c>false</c>: Only shows the message.</para>
        /// </summary>
        public static bool ShowAdditionalInfo { get; set; }
        /// <summary>
        /// This property is only used when the <c>Log.ShowAdditionalInfo</c> property is true.
        /// <para>By default the property and empty.</para>
        /// </summary>
        public static string AdditionalInfoFormat { get; set; }
        
        /// <summary>
        /// The file path to which log entries are written.
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
        /// Emitted when an <see cref="Entry"/> has just been written to the log file.
        /// </summary>
        public static event Action<Entry>? EntryWritten;
        
        /// <summary>
        /// Writes the text representation of <paramref name="entry"/> to the log file. Also writes to the console in debug mode.
        /// </summary>
        /// <param name="entry">The <see cref="Entry"/> to write.</param>
        public static void Write(Entry entry)
        {
            if (!ShowAdditionalInfo)
                Write("{5}", entry);
            else Write(AdditionalInfoFormat, entry);
        }
        
        /// <summary>
        /// Writes <paramref name="arg"/> to the log file, encoding it as a notification.
        /// </summary>
        /// <param name="arg">The message to write.</param>
        public static void Write(object arg) {
            Log.Write("{0}", arg);
        }

        /// <summary>
        /// Writes <paramref name="args"/> to the log file, encoding it as a notification.
        /// </summary>
        public static void Write(string format, params object[] args) {
            Log.Write(new Entry(string.Format(format, args), Entry.MessageSeverity.Notification));
        }
        
        /// <summary>
        /// Writes <paramref name="message"/> to the log file, encoding it as a warning.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public static void Warning(object message)
        {
            Log.Warning("{0}", message);
        }

        /// <summary>
        /// Writes <paramref name="args"/> to the log file, encoding it as a warning.
        /// </summary>
        public static void Warning(string format, params object[] args) {
            Log.Write(new Entry(string.Format(format, args), Entry.MessageSeverity.Warning));
        }
        
        /// <summary>
        /// Writes the text representation of <paramref name="exception"/> to the log file, encoding it as a warning.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> to write.</param>
        public static void Warning(Exception exception) {
            StringBuilder builder = new();
            builder.Append("[{1}{2}{3}{4}]==========>\r\n").
            Append("{5}\r\n").Append("==========>\r\n");
            Log.Write(builder.ToString(), new Entry(exception.ToString(), Entry.MessageSeverity.Warning));
        }
        
        /// <summary>
        /// Writes <paramref name="message"/> to the log file, encoding it as an error.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public static void Error(object message)
        {
            Log.Error("{0}", message);
        }

        /// <summary>
        /// Writes <paramref name="args"/> to the log file, encoding it as an error.
        /// </summary>
        public static void Error(string format, params object[] args) {
            Log.Write(new Entry(string.Format(format, args), Entry.MessageSeverity.Error));
        }
        
        /// <summary>
        /// Writes the text representation of <paramref name="exception"/> to the log file, encoding it as an error.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> to write.</param>
        public static void Error(Exception exception) {
            StringBuilder builder = new();
            builder.Append("[{1}{2}{3}{4}]==========>\r\n").
            Append("{5}\r\n").Append("==========>\r\n");
            Log.Write(builder.ToString(), new Entry(exception.ToString(), Entry.MessageSeverity.Error));
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

        private static void GodotPrint(Entry entry)
        {
            switch (entry.Severity)
            {
                case Entry.MessageSeverity.Notification:
                    GD.Print(entry);
                    break;
                case Entry.MessageSeverity.Warning:
                    GD.PushWarning(entry.ToString());
                    break;
                case Entry.MessageSeverity.Error:
                    GD.PushError(entry.ToString());
                    break;
            }
        }
        
        private static void OnUnhandledException(object source, UnhandledExceptionEventArgs arguments)
        {
            Log.Write(new Entry(arguments.ExceptionObject.ToString(), Entry.MessageSeverity.Error));
            if (!arguments.IsTerminating)
            {
                return;
            }
            
            if (Log.file.IsOpen())
            {
                Log.file.Close();
            }
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

        private static void Write(string format, Entry entry) {
            Log.GodotPrint(entry);
            if (string.IsNullOrWhiteSpace(format))
                Log.file.StoreLine(entry.ToString());
            else Log.file.StoreLine(entry.ToString(format));
            if (Log.entries.Count is Log.maxEntryCount)
            {
                Log.entries.Dequeue();
            }
            Log.entries.Enqueue(entry);
            Log.Flush();
            Log.EntryWritten?.Invoke(entry);
        }
    }
}