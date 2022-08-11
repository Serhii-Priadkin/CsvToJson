using Microsoft.VisualBasic.FileIO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Converters;
using Serilog;
using NLog.Fluent;

namespace Hometask1 //Don't work with:| ' |,| “ |,| ” |
{
    class Program
    {
        static void Main(string[] args)
        {
            int parsedFiles = 0;
            int parsedLines = 0;
            int numberOfErrors = 0;
            List<string> invalidFilesList = new List<string>();
            string invalidFiles = "";
            string errors = "";

            try
            {
                bool isWork = true;
                string allCommands = "\n1 - Start \n2 - Reset (stop and remove read file mark) \n3 - Exit\n-------------------------------------\n";

                while (isWork)
                {
                    Console.WriteLine(allCommands);
                    string inputCommandStr = Console.ReadLine();
                    int inputCommand = 0;

                    try
                    {
                        inputCommand = int.Parse(inputCommandStr);
                    }

                    catch (FormatException e)
                    {
                        Console.Write($"\n{inputCommandStr}   -   It is a wrong format of command\n");
                        numberOfErrors++;
                        errors += $"\n{numberOfErrors})  {e.Message}\n";
                    }

                    switch (inputCommand)
                    {
                        case 1:
                            {
                                string directoryName = @"C:\Users\sergi\OneDrive\Рабочий стол\Radency\Hometask1\folder_a\";
                                DirectoryInfo folderA = new DirectoryInfo(directoryName);
                                string name = string.Format(CultureInfo.InvariantCulture, "{0:MM-dd-yyyy}", DateTime.Today);
                                int lastIndex = 0;
                                List<Person> people = new List<Person>();
                                var files = folderA.GetFiles()
                                    .Where(f => (f.Name.EndsWith(".txt") || f.Name.EndsWith(".csv")) && !f.Name.StartsWith("(Done)"))
                                    .OrderByDescending(f => f.LastWriteTime);
                                if (files == null || !files.Any())
                                {
                                    Console.WriteLine("\nThere are no files to process\n");
                                }

                                else
                                {
                                    FileInfo newfiles;
                                    int fileNumber = 1;
                                    foreach (FileInfo file in files)
                                    {

                                        try
                                        {
                                            using (TextFieldParser parser = new TextFieldParser($@"{file}"))
                                            {
                                                parser.TextFieldType = FieldType.Delimited;
                                                parser.SetDelimiters(",");
                                                while (!parser.EndOfData)
                                                {
                                                    string[] fields = parser.ReadFields();


                                                    Person newPerson = new Person();
                                                    string[] addressSep = fields[2].Split(',');

                                                    if (fields[0] == null || fields[0] == string.Empty ||
                                                        fields[1] == null || fields[1] == string.Empty ||
                                                        fields[2] == null || fields[2] == string.Empty ||
                                                        fields[3] == null || fields[3] == string.Empty ||
                                                        fields[4] == null || fields[4] == string.Empty ||
                                                        fields[5] == null || fields[5] == string.Empty ||
                                                        fields[6] == null || fields[6] == string.Empty)
                                                    {
                                                        numberOfErrors++;
                                                    }

                                                    newPerson.city = addressSep[0];
                                                    newPerson.name = fields[0] + " " + fields[1];
                                                    try
                                                    {
                                                        newPerson.payment = Convert.ToDecimal(fields[3]);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        numberOfErrors++;
                                                        errors += $"\n{numberOfErrors})  {e.Message}\n";
                                                        invalidFilesList.Add($"{file.Name.Replace(".txt", "").Replace(".csv", "")},");
                                                    }
                                                    try
                                                    {
                                                        DateTime valDate;
                                                        if (DateTime.TryParseExact(fields[4], "yyyy-dd-MM", null, DateTimeStyles.None, out valDate))
                                                        {
                                                            newPerson.date = valDate;
                                                        }
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        numberOfErrors++;
                                                        errors += $"\n{numberOfErrors})  {e.Message}\n";
                                                        invalidFilesList.Add($"{file.Name.Replace(".txt", "").Replace(".csv", "")},");
                                                    }
                                                    try
                                                    {
                                                        newPerson.account_number = Convert.ToInt64(fields[5]);
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        numberOfErrors++;
                                                        errors += $"\n{numberOfErrors})  {e.Message}\n";
                                                        invalidFilesList.Add($"{file.Name.Replace(".txt", "").Replace(".csv", "")},");
                                                    }

                                                    newPerson.service = fields[6];

                                                    people.Add(newPerson);

                                                }

                                            }

                                            List<Person> personInfo = new List<Person>();
                                            var groupByCityThenByService = people
                                                             .GroupBy(c => c.city)
                                                             .Select(g => new
                                                             {
                                                                 city = g.Key,
                                                                 services = g
                                                                        .GroupBy(g => g.service)
                                                                        .Select(s => new
                                                                        {
                                                                            name = s.Key,
                                                                            payers = s.Select(a => new
                                                                            {
                                                                                a.name,
                                                                                a.payment,
                                                                                a.date,
                                                                                a.account_number
                                                                            }),
                                                                            total = s.Sum(c => c.payment)
                                                                        }),
                                                                 total = g.Sum(c => c.payment)
                                                             });


                                            string folderName = @"C:\Users\sergi\OneDrive\Рабочий стол\Radency\Hometask1\folder_b";
                                            string pathString = System.IO.Path.Combine(folderName, $"{name}");
                                            System.IO.Directory.CreateDirectory(pathString);
                                            string json = JsonConvert.SerializeObject(groupByCityThenByService,
                                                                                      Newtonsoft.Json.Formatting.Indented,
                                                                                      new IsoDateTimeConverter() { DateTimeFormat = "yyyy-dd-MM" });

                                            if (Directory.GetFileSystemEntries(pathString).Length != 0)
                                            {
                                                DirectoryInfo folderB = new DirectoryInfo(pathString);
                                                string lastOutputName = "";
                                                string lastOutputIndex = "";
                                                var lastOutputFiles = folderB.GetFiles()
                                                                             .Where(f => f.Name.EndsWith($".json"))
                                                                             .OrderBy(f => f.Name.Length).TakeLast(1);
                                                foreach (var lastOutputFile in lastOutputFiles)
                                                {
                                                    lastOutputName = lastOutputFile.Name.ToString();
                                                }
                                                for (int i = 0; i < lastOutputName.Length; i++)
                                                {
                                                    if (Char.IsNumber(lastOutputName[i]))
                                                    {
                                                        lastOutputIndex += lastOutputName[i];
                                                    }
                                                }
                                                lastOutputIndex.Reverse();

                                                int valInt;
                                                if (Int32.TryParse(lastOutputIndex, out valInt))
                                                {
                                                    lastIndex = valInt;
                                                }
                                                fileNumber = lastIndex + 1;
                                            }

                                            File.WriteAllText($@"C:\Users\sergi\OneDrive\Рабочий стол\Radency\Hometask1\folder_b\{name}\output{fileNumber}.json", json);
                                            Console.WriteLine($"\n-------------------------------------------------\n{file.Name}   convert to   output{fileNumber}.json");
                                            fileNumber++;
                                            newfiles = file.CopyTo($@"C:\Users\sergi\OneDrive\Рабочий стол\Radency\Hometask1\folder_a\(Done){file.Name}");
                                            Console.WriteLine($"{file.Name}   =>   {newfiles.Name}");
                                            file.Delete();


                                        }

                                        catch (Exception ex)
                                        {
                                            numberOfErrors++;
                                            invalidFilesList.Add($"{file.Name.Replace(".txt", "").Replace(".csv", "")},");
                                        }
                                    }
                                }
                            }
                            break;
                        case 2:
                            {
                                string directoryName = @"C:\Users\sergi\OneDrive\Рабочий стол\Radency\Hometask1\folder_a\";
                                DirectoryInfo folderA = new DirectoryInfo(directoryName);
                                var newfiles = folderA.GetFiles().Where(n => n.Name.StartsWith("(Done)"));
                                foreach (FileInfo newfile in newfiles)
                                {
                                    string newfileName = newfile.Name;
                                    newfileName = newfileName.Replace("(Done)", "");
                                    Console.WriteLine($"{newfile.Name}   =>   {newfileName}");
                                    FileInfo file = newfile.CopyTo(($@"C:\Users\sergi\OneDrive\Рабочий стол\Radency\Hometask1\folder_a\{newfileName}"));
                                    newfile.Delete();
                                }

                                break;
                            }
                        case 3:
                            {
                                isWork = false;
                                Console.WriteLine("\nBye\n");
                                break;
                            }
                        default:
                            {
                                Console.Write($"\n{inputCommandStr}   -    There are no such command\n");

                                break;
                            }

                    }
                }
            }
            catch (Exception ex)
            {
                numberOfErrors++;

            }
            string meta = "";

            try
            {
                string name = string.Format(CultureInfo.InvariantCulture, "{0:MM-dd-yyyy}", DateTime.Today);
                parsedFiles = Directory.GetFiles($@"C:\Users\sergi\OneDrive\Рабочий стол\Radency\Hometask1\folder_b\{name}", "*.json").Length;
                var jsonFiles = Directory.GetFiles($@"C:\Users\sergi\OneDrive\Рабочий стол\Radency\Hometask1\folder_b\{name}", "*.json");
                foreach (var jsonFile in jsonFiles)
                {
                    var lineCount = 0;
                    using (var reader = File.OpenText(jsonFile))
                    {
                        while (reader.ReadLine() != null)
                        {
                            lineCount++;
                        }
                    }
                    parsedLines += lineCount;
                }
                if (numberOfErrors == 0)
                {
                    meta = $"parsed_files: {parsedFiles}\nparsed_lines: {parsedLines}\nfound_errors: 0\ninvalid_files: none";
                    System.IO.File.WriteAllText($@"C:\Users\sergi\OneDrive\Рабочий стол\Radency\Hometask1\folder_b\{name}\meta.log", meta);
                }
                else
                {
                    invalidFiles = string.Join("", invalidFilesList.Distinct());
                    meta = $"parsed_files: {parsedFiles}\nparsed_lines: {parsedLines}\nfound_errors: {numberOfErrors}\ninvalid_files: [{invalidFiles}]";
                    System.IO.File.WriteAllText($@"C:\Users\sergi\OneDrive\Рабочий стол\Radency\Hometask1\folder_b\{name}\meta.log", meta);
                }
            }

            catch (Exception e)
            {

                Console.WriteLine($"While writting meta.log\n{e}");
            }

            Console.WriteLine($"{meta}");

        }
    }
}
