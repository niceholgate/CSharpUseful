
using System.Collections.Generic;
using System.Linq;

namespace NicUtils.ExtensionMethods {
    public static class Array2DExtensions {
        public static IEnumerable<T> SliceRow<T>(this T[,] array, int row) {
            for (var i = 0; i < array.GetLength(0); i++) {
                yield return array[row, i];
            }
        }

        public static IEnumerable<T> SliceColumn<T>(this T[,] array, int col) {
            for (var j = 0; j < array.GetLength(1); j++) {
                yield return array[j, col];
            }
        }

        public static T[][] ToJagged<T>(this T[,] array) {
            int rows = array.GetLength(0);
            T[][] jagged = new T[rows][];
            for (int i = 0; i < rows; i++) {
                jagged[i] = array.SliceRow(i).ToArray();
            }
            return jagged;
        }

        public static IEnumerable<T> Unroll2DMultiDimArray<T>(this T[,] array, bool traverseColumns = false) {
            List<T> list = new();
            if (traverseColumns) {
                for (int j = 0; j < array.GetLength(1); j++) list.AddRange(array.SliceColumn(j));
            } else {
                for (int i = 0; i < array.GetLength(0); i++) list.AddRange(array.SliceRow(i));
            }
            return list;
        }
        
        public static T[,] Transpose<T>(this T[,] array)
        {
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            T[,] result = new T[cols, rows];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++) result[j, i] = array[i, j];
            }

            return result;
        }
    }
}
