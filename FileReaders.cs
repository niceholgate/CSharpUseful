using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace NicUtils
{
    public static class CSVReader<T>
    {
        public static List<T[]> ReadCSV(string filepath, char splitChar = ',') {
            var reader = new StreamReader(filepath);
            var table = new List<T[]>();
            while (!reader.EndOfStream) {
                string[] stringValues = reader.ReadLine().Split(splitChar);
                table.Add((from i in Enumerable.Range(0, stringValues.Count()) select stringValues[i].ConvertTo<T>()).ToArray());
            }
            reader.Close();
            return table;
        }
    }
}

