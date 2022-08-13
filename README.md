# CsvToJson
Program to convert every new file ".csv" or ".txt" to new file  ".json"

  A file can be either in TXT or CSV format with the following content:
<first_name: string>, <last_name: string>, <address: string>, <payment: decimal>, <date: date>,
<account_number: long>, <service: string>


    Example (raw_data.txt):
    
Eddy, Murphy, "New York, Park av 30, 10", 50.0, 2022-24-02, 20222402, Beer
Mikky, Mouse, "San-Francisco, Disney str 5, 6", 100.0, 2022-24-02, 20222402, Cheese

  
    When the file is processed the service saves the results in a separate folder (B)
(the path must be specified in the config) in a subfolder (C) with the current date 
(i.g.09-21-2022). As a file name used “output” + today’s current file number + “.json”
At the end of the day (midnight) the service store in the subfolder (C) a file called
“meta.log”. 
    Interval of researching for a new files is 10 sec. Every old files marked its names 
with (Done) at the beggening of name.
    Ignoring everything except TXT and CSV. File name does not matter. Every line in the 
file may have errors (missing values or invalid types), program ignore those data. Counts 
all invalid lines and files, and writes them down in the “meta.log” file.
  
    The file have the following structure:
    
parsed_files: N
parsed_lines: K
found_errors: Z
invalid_files: [path1, path2, path3
  
  
    This program was my hometask.
