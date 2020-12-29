using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace NicUtils
{
    public class CSVReader
    {
        public static List<int[]> ReadCSVInt(string filepath, char splitChar = ',') {
            List<string[]> stringTable = ReadCSVString(filepath, splitChar);
            List<int[]> intTable = new List<int[]>();
            foreach (string[] stringValues in stringTable) {
                intTable.Add((from i in Enumerable.Range(0, stringValues.Count()) select int.Parse(stringValues[i])).ToArray());
            }
            return intTable;
        }

        public static List<double[]> ReadCSVDouble(string filepath, char splitChar = ',') {
            List<string[]> stringTable = ReadCSVString(filepath, splitChar);
            List<double[]> doubleTable = new List<double[]>();
            foreach (string[] stringValues in stringTable) {
                doubleTable.Add((from i in Enumerable.Range(0, stringValues.Count())select double.Parse(stringValues[i])).ToArray());
            }
            return doubleTable;
        }

        public static List<string[]> ReadCSVString(string filepath, char splitChar = ',') {
            var reader = new StreamReader(filepath);
            var table = new List<string[]>();
            while (!reader.EndOfStream) {
                string[] stringValues = reader.ReadLine().Split(splitChar);
                table.Add(stringValues);
            }
            reader.Close();
            return table;
        }
    }
}

