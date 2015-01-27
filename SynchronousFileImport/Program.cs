using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SynchronousFileImport
{
    class Program
    {
        static void Main(string[] args)
        {
            new AsyncFileImporter().RunAsync().Wait();

            //this is so we can see the output before the console window
            //goes away
            Console.ReadLine();
        }
    }

    class FileImporter
    {
        public void Run()
        {
            string[] fileNames = GetFileNamesFromRemoteSource();
            foreach (string fileName in fileNames)
            {
                ImportFileFromRemoteSourceToStorageOfSomeKind(fileName);
                //report that we finished importing this file
                Console.WriteLine("Just finished importing {0}", fileName);
            }
        }

        /// <summary>
        /// synchronous method to retrieve the names of the files we want to import
        /// this could be a list of file names from a remote FTP server
        /// or file names stored in an SQL or NoSQL database or where ever
        /// </summary>
        /// <returns>an array of file names</returns>
        private string[] GetFileNamesFromRemoteSource()

        {
            //simulate the return of file names from the remote source
            return new string[] { "filename1.xml", "filename2.xml" };
        }

        /// <summary>
        /// synchronously import the file from the remote source to a new storage place
        /// doing it this way is ineffiecient because we can only do one at a time and 
        /// it takes time to pull the file down from the remote source and upload it to the
        /// new storage
        /// </summary>
        /// <param name="fileName"></param>
        private void ImportFileFromRemoteSourceToStorageOfSomeKind(string fileName)
        {
            //code to import here
            Console.WriteLine("Busy importing {0}.  And everything has to wait until I am finished!! (ha ha)", fileName);
        }
    }

    /// <summary>
    /// Async version of the file importer
    /// </summary>
    class AsyncFileImporter
    {
        /// <summary>
        /// Import the files asynchronously; process each file and continue
        /// execution to the next file even if it takes awhile to import any one
        /// file
        /// </summary>
        /// <returns></returns>
        async public Task RunAsync()
        {
            List<string> fileNames = GetFileNamesFromRemoteSource();

            //queue up the filenames to be processed asynchronously
            IEnumerable<Task<string>> filesToBeImportedQuery =
                from fileName in fileNames select ImportFileFromRemoteSourceToStorageOfSomeKindAsync(fileName);

            //execute the query to start importing the filenames
            List<Task<string>> importedFileNames = filesToBeImportedQuery.ToList();

            //now we can watch the list of running tasks for when they finish
            //and do some post processing if we have to
            //or have a progress bar, etc
            while (importedFileNames.Count > 0)
            {
                //await a task to be completed and do something with it if we want
                Task<string> finishedTask = await Task.WhenAny(importedFileNames);

                //remove the task from the list to prevent processing more than once
                importedFileNames.Remove(finishedTask);

                //wait to the task that was returned from above
                //this will give us the value the import task returns
                string fileName = await finishedTask;

                //now we can do other things like reporting progress
                Console.WriteLine("Just finished importing {0}.", fileName);
            }

        }

        /// <summary>
        /// synchronous method to retrieve the names of the files we want to import
        /// this could be a list of file names from a remote FTP server
        /// or file names stored in an SQL or NoSQL database or where ever
        /// 
        /// still getting this synchronously because it's just getting file names 
        /// which can be done in one fell swoop; there is no real wait time to
        /// consider (in theory)
        /// 
        /// </summary>
        /// <returns>an array of file names</returns>
        private List<string> GetFileNamesFromRemoteSource()
        {
            //simulate the return of file names from the remote source
            List<string> fileNames = new List<string>();
            int numberOfFiles = 5;
            for (int i = 1; i <= numberOfFiles; i++)
            {
                fileNames.Add(String.Format("filename{0}.xml", i));
            }
            return fileNames;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        async private Task<string> ImportFileFromRemoteSourceToStorageOfSomeKindAsync(string fileName)
        {
            //some long running operation that would hold up a synchronous running program
            //but we are asynchronous so no matter how long this takes the program will continue on
            //to the next task and come back to this when it is done
            //this could be reading a file async with StreamReader ReadToEndAsync
            //we are going to simulate it
            Console.WriteLine("Processing {0}.", fileName);
            var result = await SomeLongRunningProcess(fileName);
            //do other stuff here if we need to
            Console.WriteLine("Finished Processing {0}.", fileName);
            //return the result
            return result;
        }

        private Task<string> SomeLongRunningProcess(string fileName)
        {   
            return Task<string>.Run(() =>
            {
                //This operation takes a while 1 and 10 seconds
                //simulate by sleeping between 
                var random = new Random();                
                Thread.Sleep(random.Next(1000, 10000));
                return fileName;
            });
        }
    }
}
