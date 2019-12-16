using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace LogTest
{
    public class Logger : ILog
    {
        private readonly ConcurrentQueue<Message> queue = new ConcurrentQueue<Message>();
        private readonly Thread thread;
        private readonly string basePath;
        private readonly string filename;
        private readonly IFolderNameProvider folderNameProvider;
        public string LastWrittenPath { get; private set; }

        public Logger(string basePath, string filename, IFolderNameProvider folderNameProvider)
        {
            this.basePath = basePath;
            this.filename = filename;
            this.folderNameProvider = folderNameProvider;
            thread = new Thread(Run);
            thread.Start();
        }

        ~Logger()
        {
            if (thread.IsAlive)
                StopWithFlush();
        }

        public void StopWithoutFlush()
        {
            if (!thread.IsAlive)
                ReportLogError(new Exception("Log was used after is was stopped"));
            else
            {
                while (!queue.IsEmpty)
                    queue.TryDequeue(out var _);
                queue.Enqueue(new Message(MessageType.ForceStop, null));
                thread.Join();
            }
        }

        public void StopWithFlush()
        {
            if (!thread.IsAlive)
                ReportLogError(new Exception("Log was used after is was stopped"));
            else
            {
                queue.Enqueue(new Message(MessageType.GraceStop, null));
                thread.Join();
            }
        }

        public void Write(string line)
        {
            if (!thread.IsAlive)
                ReportLogError(new Exception("Log was used after is was stopped"));
            else
                queue.Enqueue(new Message(MessageType.Log, line));
        }

        private void ReportLogError(Exception e)
        {
            // Logging has failed, fallback to other log or health check mechanisms, this handling could be marshaled to another class which is dependency injected
            Console.WriteLine(e.Message + " " + e.StackTrace);
        }

        private void Run()
        {
            try
            {
                Loop();
            }
            catch (Exception e)
            {
                ReportLogError(e);
            }
        }

        private void Loop()
        {
            bool go = true;
            bool graceStop = false;
            const int stdMaxWaitMs = 10;
            int maxWaitMs = stdMaxWaitMs;
            StreamWriter writer = null;
            try
            {
                while (go)
                {
                    Thread.Sleep(maxWaitMs);

                    if (queue.TryDequeue(out Message msg))
                    {
                        if (msg.MessageType == MessageType.ForceStop)
                            break;
                        else if (msg.MessageType == MessageType.GraceStop)
                            graceStop = true;
                        else
                        {
                            string fullpath = Path.Combine(basePath, folderNameProvider.GetFolderName() + "_" + filename);

                            if (writer == null)
                                writer = new StreamWriter(fullpath, true);
                            else if (fullpath != LastWrittenPath)
                            {
                                writer.Dispose();
                                writer = new StreamWriter(fullpath, true);
                            }

                            writer.Write(msg.Data + Environment.NewLine);
                            LastWrittenPath = fullpath;
                        }
                    }

                    if (graceStop)
                    {
                        maxWaitMs = 0;
                        if (queue.IsEmpty)
                            go = false;     // If another thread runs enqueue now, that won't be logged without notice, Logger not thread-safe
                    }
                    else
                        maxWaitMs = queue.Count > 10 ? 0 : stdMaxWaitMs;
                }
            }
            finally
            {
                writer?.Dispose();
            }
        }
    }
}
