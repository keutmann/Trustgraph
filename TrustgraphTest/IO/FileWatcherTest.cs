using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TrustgraphCore.IO;

namespace TrustpathTest.IO
{
    [TestFixture]
    public class FileWatcherTest
    {
        int created = 0;
        int existed = 0;
        string testFile = "test.txt";


        [Test]
        public void RunRecoveringWatcher()
        {
            //var TestPath = Path.GetTempPath()+"\\test";
            var TestPath = Path.GetTempPath();
            var testFilePath = Path.Combine(TestPath, testFile);
            Console.WriteLine("Will auto-detect unavailability of watched directory");
            Console.WriteLine(" - Windows timeout accessing network shares: ~110 sec on start, ~45 sec while watching.");
            using (var watcher = new RecoveringFileSystemWatcher(TestPath, "*.txt"))
            {

                //watcher.All += Watcher_All;
                //watcher.All += (_, e) => { Console.WriteLine("{0} {1}", e.ChangeType, e.Name); };
                //watcher.Error += (_, e) => { Console.WriteLine("Error: "+ e.Error.Message); };
                watcher.Created += (_, e) => {Console.WriteLine("{0} {1}", e.ChangeType, e.Name); created++; };
                watcher.Existed += Watcher_Existed;
                //watcher.Created += Watcher_Created;
                //watcher.Error += (_, e) =>
                //    { WriteLineInColor($"Suppressing auto handling of error {e.Error.Message}", ConsoleColor.Red);
                //        //...
                //        e.Handled = true;
                //    };
                watcher.DirectoryMonitorInterval = TimeSpan.FromSeconds(2);
                watcher.EventQueueCapacity = 1;
                watcher.EnableRaisingEvents = true;
                //watcher.NotifyFilter = NotifyFilters.Attributes |
                //                        NotifyFilters.CreationTime |
                //                        NotifyFilters.FileName |
                //                        NotifyFilters.LastAccess |
                //                        NotifyFilters.LastWrite |
                //                        NotifyFilters.Size |
                //                        NotifyFilters.Security;

                //PromptUser("Processing...");
                if (File.Exists(testFilePath))
                    File.Delete(testFilePath);

                var deadmanswitch = 0;
                do
                {
                    Thread.Sleep(100);
                    deadmanswitch++;
                    File.WriteAllText(testFilePath, "Hello world!");
                }
                while (created == 0 && deadmanswitch < 100);
                Assert.IsTrue(created > 0);
                Console.WriteLine("Existed! " + existed);
            }
        }

        private void Watcher_Existed(object sender, FileSystemEventArgs e)
        {
            existed++;
            if (e.Name == testFile)
                Console.WriteLine("Test file found!");
        }

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            created++;
        }
    }
}

