using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using NicUtils.ExtensionMethods;

namespace NicUtils {

    public class CSVReader {

        public bool WithHeaders { get; private set; }

        public char SplitChar { get; private set; }

        public List<string> Headers { get; private set; } = null;

        private readonly List<List<string>> stringData = new();

        public CSVReader(string filepath, bool withHeaders, char splitChar = ',') {
            WithHeaders = withHeaders;
            SplitChar = splitChar;
            ReadCSV(filepath);
        }

        public List<List<T>> GetData<T>() {
            List<List<T>> dataConverted = new();
            foreach (List<string> row in stringData) {
                dataConverted.Add(row.Select(el => el.AttemptConversion<T>().result).ToList());
            }
            return dataConverted;
        }

        private void ReadCSV(string filepath) {
            StreamReader reader = new(filepath);
            while (!reader.EndOfStream) {
                string line = reader.ReadLine();
                if (WithHeaders && Headers == null) {
                    Headers = line.Split(SplitChar).Select(el => el.Trim()).ToList();
                    continue;
                }
                stringData.Add(line.Split(SplitChar)
                    .Select(el => {
                        string trimmed = el.Trim();
                        if (trimmed.Length == 0) return null;
                        return trimmed;
                    })
                    .ToList());
            }
            reader.Close();
        }

    }

    public class TextLineReader {

        private readonly List<string> stringData = new();

        public TextLineReader(string filepath) {
            ReadText(filepath);
        }

        public List<string> GetData() {
            return stringData;
        }

        private void ReadText(string filepath) {
            StreamReader reader = new(filepath);
            while (!reader.EndOfStream) {
                string line = reader.ReadLine() ?? "";
                stringData.Add(line);
            }
            reader.Close();
        }
    }

}

