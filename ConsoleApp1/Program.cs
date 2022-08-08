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

namespace Hometask1 //Don't work with:| ' |,| “ |,| ” |
{
    class Program
    {
        static void Main(string[] args)
        {
            string directoryName = @"C:\Users\sergi\OneDrive\Рабочий стол\Radency\Hometask1\folder_a\";
            DirectoryInfo folderA = new DirectoryInfo(directoryName);
            List<Person> people = new List<Person>();
            var files = folderA.GetFiles()
                .Where(f => (f.Name.EndsWith(".txt") || f.Name.EndsWith(".csv")) && !f.Name.StartsWith("(Done)"))
                .OrderBy(f => f.LastWriteTime);
            int fileNumber = 1;


            foreach (FileInfo file in files)
            {
                Console.WriteLine(file.FullName);

                using (TextFieldParser parser = new TextFieldParser($@"{file}"))
                {

                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    while (!parser.EndOfData)
                    {
                        string[]? fields = parser.ReadFields();


                        Person newPerson = new Person();


                        newPerson.name = fields[0] + " " + fields[1];

                        string[] addressSep = fields[2].Split(',');

                        newPerson.city = addressSep[0];

                        decimal valPay;
                        if (Decimal.TryParse(fields[3], out valPay))
                        {
                            newPerson.payment = valPay;
                        }

                        DateTime valDate;
                        if (DateTime.TryParseExact(fields[4], "yyyy-dd-MM", null, DateTimeStyles.None, out valDate))
                        {
                            newPerson.date = valDate;
                        }

                        long valLong;
                        if (Int64.TryParse(fields[5], out valLong))
                        {
                            newPerson.account_number = valLong;
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

                string name = string.Format(CultureInfo.InvariantCulture, "{0:MM-dd-yyyy}", DateTime.Today);
            string folderName = @"C:\Users\sergi\OneDrive\Рабочий стол\Radency\Hometask1\folder_b";
            string pathString = System.IO.Path.Combine(folderName, $"{name}");
            System.IO.Directory.CreateDirectory(pathString);
            string json = JsonConvert.SerializeObject(groupByCityThenByService,
                                                      Newtonsoft.Json.Formatting.Indented,
                                                      new IsoDateTimeConverter() { DateTimeFormat = "yyyy-dd-MM" });

            if (Directory.GetFileSystemEntries(pathString).Length != 0)
            {
                DirectoryInfo folderB = new DirectoryInfo(pathString);
                string lastOutputName = null;
                string lastOutputIndex = null;
                var lastOutputFiles = folderB.GetFiles()
                        .Where(f => f.Name.EndsWith($".json"))
                        .OrderByDescending(f => f.Name).Take(1);
                foreach (var lastOutputFile in lastOutputFiles)
                    lastOutputName = lastOutputFile.Name.ToString();
                foreach (char findIndex in lastOutputName)
                    if (Char.IsDigit(findIndex))
                    {
                        lastOutputIndex += findIndex;
                    }
                int lastIndex = Convert.ToInt32(lastOutputIndex);
                fileNumber = lastIndex + 1;
                Console.WriteLine(fileNumber);
            }

            //Console.WriteLine(json);
            File.WriteAllText($@"C:\Users\sergi\OneDrive\Рабочий стол\Radency\Hometask1\folder_b\{name}\output{fileNumber}.json", json);
            fileNumber++;
            FileInfo newfile = file.CopyTo($@"C:\Users\sergi\OneDrive\Рабочий стол\Radency\Hometask1\folder_a\(Done){file.Name}");
            file.Delete();

        }
    }
}
}