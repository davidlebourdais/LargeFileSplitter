using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace EMA.Tools
{
    class Program
    {
        private const long one_gigabyte = 1024 * 1024 * 1024;
        private const long min_large_file_size = (long)(2 * one_gigabyte);  // consider files with >2Gb to be large enough for spliting.
        private const long file_size_limit = one_gigabyte;  // indicates the size of final ouputs.

        static void Main(string[] args)
        {
            Console.WriteLine("--- Large text file splitter ---");

            var targetdir = (string)null;
            var min_large_file_size_gb = (double)min_large_file_size / (double)one_gigabyte;
            var file_size_limit_gb = (double)file_size_limit / (double)one_gigabyte;
            var start_gb = 0.0d;

            if (args.Length > 0)
            {
                for(int i = 0; i < args.Length; i++)
                {                
                    if (args[i] == "--help")
                    {
                        Console.WriteLine("Files must be in same directory as app.");
                        Console.WriteLine("Add -min x where x is size in GB to set file size detection threshold.");
                        Console.WriteLine("Add -size x where x is size in GB to set resulting file size.");
                        Console.WriteLine("Add -start x where x is position in GB to set a starting point for processing (default is 0).");
                    }
                    else if (args[i] == "-min" && i + 1 < args.Length)
                    {
                        if (double.TryParse(args[i + 1], NumberStyles.Any, CultureInfo.InvariantCulture, out double min_size))
                            min_large_file_size_gb = min_size;
                        else Console.WriteLine("Could not parse min argument");
                    }
                    else if (args[i] == "-size" && i + 1 < args.Length)
                    {
                        if (double.TryParse(args[i + 1], NumberStyles.Any, CultureInfo.InvariantCulture, out double min_size))
                            file_size_limit_gb = min_size;
                        else Console.WriteLine("Could not parse size argument");
                    }
                    else if (args[i] == "-start" && i + 1 < args.Length)
                    {
                        if (double.TryParse(args[i + 1], NumberStyles.Any, CultureInfo.InvariantCulture, out double min_size))
                            start_gb = min_size;
                        else Console.WriteLine("Could not parse start argument");
                    }
                    else if (targetdir == null)
                    {
                        try
                        {
                            // Try to find if a directory was passed as argument:
                            var targetpath =  Path.GetFullPath(args[i]);
                             targetdir = Directory.Exists(targetpath) ? targetpath : null;
                        }
                        catch {}
                    }
                }
            }

            Console.WriteLine("Target directory is: " + targetdir);
            Console.WriteLine("Considering large files when size > " + min_large_file_size_gb.ToString("F3") + " GB to be split into " + file_size_limit_gb.ToString("F3") + " GB files");
            Console.WriteLine("");

            var currentDir = new DirectoryInfo(targetdir ?? Directory.GetCurrentDirectory());
            var largeFileList = currentDir.GetFiles().Where(x => x.Length > min_large_file_size_gb * one_gigabyte).ToList();
            
            // Quit if no file found:
            if (largeFileList.Count == 0)
            {
                Console.WriteLine("No files >" + min_large_file_size_gb.ToString("F3") + " GB were found");
                Console.ReadKey();
                return;
            }

            // Else display large files:
            Console.WriteLine("Found " + largeFileList.Count + " files >" + min_large_file_size_gb + " GB");

            var timeOrderedFiles = largeFileList.OrderBy(x => x.LastWriteTime);  // order them by last modification
            int count = 0;
            foreach (var file in timeOrderedFiles)
            {
                Console.WriteLine(count++ + ". " + file.Name + " - " + file.LastWriteTime + " - " + file.Length + " bytes.");
            }

            var keep_going = true;
            Console.WriteLine("");

            while (keep_going)
            {
                Console.Write("Select a file number then press enter: ");

                var answer = "";

                try
                { 
                    answer = readLineWithCancel();
                    Console.WriteLine("");
                }
                catch
                {
                    Console.WriteLine("");
                    Console.WriteLine("Quitting...");
                    return;
                }

                if (UInt32.TryParse(answer, out uint selection) && selection < largeFileList.Count)
                {
                    keep_going = false;

                    var largeFileInfo = timeOrderedFiles.ToArray()[selection];
                    var reader = new StreamReader(largeFileInfo.FullName);
                    var writter = (StreamWriter)null;
                    var writter_base_path = Path.Combine(targetdir, largeFileInfo.Name.Remove(largeFileInfo.Name.Length - largeFileInfo.Extension.Length, largeFileInfo.Extension.Length));
                    var writting_size = 0;
                    var file_number = 1;
                    var progression_step = 0;
                    if (start_gb * one_gigabyte >= largeFileInfo.Length)
                    {
                        Console.WriteLine("Start point is greater than file size. There is nothing to do...");
                        Console.WriteLine("Quitting...");
                        return;
                    }

                    var start = (long)(start_gb * one_gigabyte);
                    var output_files_number = (int)((largeFileInfo.Length - start) / (file_size_limit_gb * one_gigabyte)) + 1;
                    var reading_size = (long)0;

                    Console.WriteLine("");
                    Console.WriteLine("Going to split " + largeFileInfo.Name + " into " + output_files_number + " files.");
                    if (start_gb > 0)
                    {
                        Console.WriteLine("Note that file processing starts from " + start_gb.ToString("F3") + " GB");
                        Console.WriteLine("Please wait while reading the first part of your large file...");
                    }
                    try
                    {
                        using (var largefile = new StreamReader(largeFileInfo.FullName))
                        {
                            while (!largefile.EndOfStream)
                            {
                                var line = largefile.ReadLine();
                                reading_size += line.Length;
                                if (reading_size < start)
                                    continue;

                                if (writter == null)
                                {
                                    var writter_path = writter_base_path + "-reduced-" + file_number + largeFileInfo.Extension;
                                    writter = new StreamWriter(writter_path, false, largefile.CurrentEncoding);
                                    Console.WriteLine("Creating " + writter_path);
                                    Console.Write(file_number++ + "/" + output_files_number + " ");
                                    Console.Write("=");
                                }

                                writter.WriteLine(line);
                                writting_size += line.Length;

                                var progress = writting_size / (file_size_limit_gb * one_gigabyte) * 100;
                                if ((int)(progress / 10) > progression_step)
                                {
                                    progression_step++;
                                    Console.Write("=");
                                }

                                if (writting_size > file_size_limit_gb * one_gigabyte)
                                {
                                    writter?.Dispose();
                                    writter = null;
                                    writting_size = 0;
                                    progression_step = 0;
                                    Console.WriteLine("");
                                }
                            }
                        }
                    }
                    finally
                    {
                        writter?.Dispose();
                        Console.WriteLine("");
                        Console.WriteLine("Done.");
                    }
                }
                else
                    Console.WriteLine("Invalid selection");
            }
        }

        private static string readLineWithCancel()
        {
            string result = null;
            var buffer = new StringBuilder();
            var info = Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter
                && info.Key != ConsoleKey.Escape)
            {
                Console.Write(info.KeyChar);
                buffer.Append(info.KeyChar);
                info = Console.ReadKey(true);
            }

            if (info.Key == ConsoleKey.Enter)
                result = buffer.ToString();

            if (info.Key == ConsoleKey.Escape)
                throw new Exception();

            return result;
        }
    }
}
