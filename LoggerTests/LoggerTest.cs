using System;
using System.IO;
using LogTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LoggerTests
{
    [TestClass]
    public class LoggerTest
    {
        private (ILog loger, FakeTimeFolderNameProvider folderNameProvider) GetLoggerOnTempPath()
        {
            string path = Path.GetTempPath();
            string id = Guid.NewGuid().ToString() + ".txt";
            var folderNameProvider = new FakeTimeFolderNameProvider(23, 30);
            ILog logger = new Logger(path, id, folderNameProvider);
            return (logger, folderNameProvider);
        }

        [TestMethod]
        public void WriteDoesWriteToFile()
        {
            // Arrange
            var (logger, _) = GetLoggerOnTempPath();
            const string teststring = "test";

            // Act
            logger.Write(teststring);
            logger.StopWithFlush();

            // Assert
            Assert.IsTrue(File.Exists(logger.LastWrittenPath));
            var contents = File.ReadAllText(logger.LastWrittenPath);
            Assert.AreEqual(teststring + Environment.NewLine, contents);

            // Cleanup
            File.Delete(logger.LastWrittenPath);
        }

        [TestMethod]
        public void NewFilesGeneratedAtMidnight()
        {
            // Arrange
            var (logger, folderNameProvider) = GetLoggerOnTempPath();
            const string teststring = "test";

            // Act
            logger.Write(teststring);
            while (logger.LastWrittenPath == null) { };
            string path1 = logger.LastWrittenPath;

            folderNameProvider.AdvanceTimeByOneHour();

            logger.Write(teststring);
            logger.StopWithFlush();
            string path2 = logger.LastWrittenPath;

            // Assert
            Assert.AreNotEqual(path1, path2);

            // Cleanup
            File.Delete(path1);
            File.Delete(path2);
        }

        [TestMethod]
        public void StopNoFlushWontLogAll()
        {
            // Arrange
            var (logger, _) = GetLoggerOnTempPath();
            const int max = 100000;

            // Act
            for (int i = 1; i <= max; i++)
                logger.Write(i.ToString());

            logger.StopWithoutFlush();

            // Assert
            Assert.IsTrue(File.Exists(logger.LastWrittenPath));
            var contents = File.ReadAllText(logger.LastWrittenPath);
            Assert.IsFalse(contents.Contains(max.ToString()));

            // Cleanup
            File.Delete(logger.LastWrittenPath);
        }

        [TestMethod]
        public void StopWithFlushWillLogAll()
        {
            // Arrange
            var (logger, _) = GetLoggerOnTempPath();
            const int max = 100;

            // Act
            for (int i = 1; i <= max; i++)
                logger.Write(i.ToString());

            logger.StopWithFlush();

            // Assert
            Assert.IsTrue(File.Exists(logger.LastWrittenPath));
            var contents = File.ReadAllText(logger.LastWrittenPath);
            Assert.IsTrue(contents.Contains(max.ToString()));

            // Cleanup
            File.Delete(logger.LastWrittenPath);
        }
    }
}
