namespace ConsoleApp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using ConsoleApp.Models;

    public class DataReader
    {
        public void ImportAndPrintData(string fileToImport)
        {
            var fileData = ImportFile(fileToImport);
            var dbData = ImportDataAndCleaning(fileData);
            DataPrinter(dbData);
        }

        public List<string> ImportFile(string fileToImport)
        {
            List<string> importedLines = new List<string>();
            using (var streamReader = new StreamReader(fileToImport))
            {
                while (!streamReader.EndOfStream)
                {
                    var line = streamReader.ReadLine();
                    if (!string.IsNullOrEmpty(line))
                    {
                        importedLines.Add(line);
                    }
                }
            }
            return importedLines;
        }

        public List<ImportedObject> ImportDataAndCleaning(List<string> importedLines)
        {

            List<ImportedObject> ImportedObjects = new List<ImportedObject>();
            for (int i = 0; i < importedLines.Count; i++)
            {
                var importedLine = importedLines[i];
                var values = importedLine.Split(';').ToList();
                var valuesCount = values.Count();
                var importedObject = new ImportedObject
                {
                    Type = values[0].Trim().ToUpper(),
                    Name = values[1].Trim(),
                    Schema = values[2].Trim(),
                    ParentName = values[3].Trim(),
                    ParentType = values[4].Trim().ToUpper(),
                    DataType = values[5]
                };
                if (valuesCount == 7)
                {
                    importedObject.IsNullable = values[6];
                }


                ImportedObjects.Add(importedObject);

            }

            foreach (var impObj in ImportedObjects)
            {
                impObj.NumberOfChildren = ImportedObjects.Count(obj =>
                obj.ParentType == impObj.Type &&
                obj.ParentName == impObj.Name);

            }
            return ImportedObjects;
        }


        public void DataPrinter(List<ImportedObject> dbData)
        {
            foreach (var database in dbData.Where(obj => obj.Type == "DATABASE"))
            {
                Console.WriteLine($"Database '{database.Name}' ({database.NumberOfChildren} tables)");

                // print all database's tables
                foreach (var table in dbData.Where(obj =>
                    obj.ParentType.ToUpper() == database.Type &&
                    obj.ParentName == database.Name))
                {

                    Console.WriteLine($"\tTable '{table.Schema}.{table.Name}' ({table.NumberOfChildren} columns)");

                    // print all table's columns
                    foreach (var column in dbData.Where(obj =>
                        obj.ParentType.ToUpper() == table.Type &&
                        obj.ParentName == table.Name))
                    {

                        Console.WriteLine($"\t\tColumn '{column.Name}' with {column.DataType} data type {(column.IsNullable == "1" ? "accepts nulls" : "with no nulls")}");

                    }
                }
            }
            Console.ReadLine();
        }
    }
}

