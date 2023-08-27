using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace NicUtils {
    // TODO: null for empty cell
    // TODO: support column headers
    public static class CSVReader {
        public static List<List<string>> ReadCSV(string filepath, char splitChar = ',') {
            StreamReader reader = new StreamReader(filepath);
            List<List<string>> table = new();
            while (!reader.EndOfStream) {
                table.Add(reader.ReadLine().Split(splitChar)
                    .Select(el => {
                        string trimmed = el.Trim();
                        if (trimmed.Length == 0) return null;
                        return trimmed;
                    })
                    .ToList());
            }
            reader.Close();
            return table;
        }
    }
}

