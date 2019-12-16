﻿using System.Linq;
using System.Threading.Tasks;

namespace LogTest
{
    public interface ILog
    {
        /// <summary>
        /// Stop the logging. If any outstadning logs theses will not be written to Log
        /// </summary>
        void StopWithoutFlush();

        /// <summary>
        /// Stop the logging. The call will not return until all all logs have been written to Log.
        /// </summary>
        void StopWithFlush();

        /// <summary>
        /// Write a message to the Log.
        /// </summary>
        /// <param name="text">The text to written to the log</param>
        void Write(string text);

        /// <summary>
        /// Gets the path of the file in which the logger last wrote to.
        /// </summary>
        string LastWrittenPath { get; }
    }
}
