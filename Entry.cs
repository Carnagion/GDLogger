using System;
using System.Globalization;

namespace Godot {
    /// <summary>
    /// Represents a log entry.
    /// </summary>
    public sealed record Entry : IFormattable
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
        /// <returns>A <see cref="String"/> in the format "[Severity] at Timestamp - Message".</returns>
        public override string ToString()
        {
            //return $"[{this.Severity}] at {this.Timestamp.Hour}:{this.Timestamp.Minute}:{this.Timestamp.Second}:{this.Timestamp.Millisecond} - {this.Message}";
            return ToString("[{0}] at {1}:{2}:{3}:{4} - {5}");
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents the <see cref="Entry"/>.
        /// </summary>
        /// <param name="format">The format in which the message will be presented.
        /// The arguments passed to <c>string.Format</c> are 
        /// <code>
        ///     <see cref="object"/>[]{
        ///         this.Severity,//index[0]
        ///         this.Timestamp.Hour,//index[1]
        ///         this.Timestamp.Minute,//index[2]
        ///         this.Timestamp.Second,//index[3]
        ///         this.Timestamp.Millisecond,//index[4]
        ///         this.Message//index[5]
        ///     }
        /// </code>
        /// </param>
        /// <returns>A <see cref="String"/> in the format "[Severity] at Timestamp - Message".</returns>
        public string ToString(string format) {
            return ToString(format, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents the <see cref="Entry"/>.
        /// </summary>
        /// <param name="format">The format in which the message will be presented.
        /// The arguments passed to <c>string.Format</c> are 
        /// <code>
        ///     <see cref="object"/>[]{
        ///         this.Severity,//index[0]
        ///         this.Timestamp.Hour,//index[1]
        ///         this.Timestamp.Minute,//index[2]
        ///         this.Timestamp.Second,//index[3]
        ///         this.Timestamp.Millisecond,//index[4]
        ///         this.Message//index[5]
        ///     }
        /// </code>
        /// </param>
        /// <returns>A <see cref="String"/> in the format "[Severity] at Timestamp - Message".</returns>
#pragma warning disable CS1573 // O parâmetro não tem nenhuma tag param correspondente no comentário XML (mas outros parâmetros têm)
        public string ToString(string format, IFormatProvider formatProvider) {
            return string.Format(formatProvider, format,
                this.Severity,
                this.Timestamp.Hour,
                this.Timestamp.Minute,
                this.Timestamp.Second,
                this.Timestamp.Millisecond,
                this.Message
            );
        }
#pragma warning restore CS1573 // O parâmetro não tem nenhuma tag param correspondente no comentário XML (mas outros parâmetros têm)

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