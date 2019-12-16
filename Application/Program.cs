using System;
using System.Threading;
using LogTest;

namespace LogUsers
{
    // Refactored: Logger rewritten, Logger has dependency on IFolderNameProvider to be able to test requirement 2 and to change that behaviour without modification of existing code.
    // Logger should maybe also have a dependency on a service handling log errors, as these are not thrown up the call stack due to requirement 4

    // Removed LogLine - didn't see its usage and task mentions only numbers should be in log files, not dates or times.

    // I tried with async/await solution, but hit a few walls such as not being able to force stop tasks using cancellation token quickly enough
    // "It is possible that a task may continue to process some items after cancellation is requested."
    // https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-cancellation
    // Making it difficult to adhere to the requirement of the task to have a "forceful" stop

    // In the end a solution using a queue seemed to fit well with the requirement 1 of the .Write to be fast - simply add it to a queue and move on, and requirement 3 felt natural with the queue

    class Program
    {
        static void Main(string[] args)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            ILog logger = new Logger(path, "log1.txt", new UtcTimeFolderNameProvider());

            for (int i = 0; i < 15; i++)
                logger.Write(i.ToString());

            logger.StopWithFlush();

            ILog logger2 = new Logger(path, "log2.txt", new UtcTimeFolderNameProvider());

            for (int i = 50; i > -1000; i--)
                logger2.Write(i.ToString());

            Thread.Sleep(20);

            logger2.StopWithoutFlush();

            Console.WriteLine("Program done, hit enter to exit");
            Console.ReadLine();
        }
    }
}
