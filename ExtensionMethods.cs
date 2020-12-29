using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using MathNet.Numerics.LinearAlgebra;

namespace NicUtils {
    public static class MultiDimArrayExtensions {
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
    }

    public static class MatrixExtensions {
        public static Vector<T> ToVector<T>(this Matrix<T> mat, bool traverseColumnsFirst = false) where T : struct, System.IEquatable<T>, System.IFormattable {
            List<T> unrolled = Enumerables<T>.Unroll2DEnumerable(mat.ToRowArrays(), traverseColumnsFirst);
            return Vector<T>.Build.DenseOfEnumerable(unrolled);
        }
        public static Matrix<T> ExcludeFirstColumn<T>(this Matrix<T> mat) where T : struct, System.IEquatable<T>, System.IFormattable {
            IEnumerable<int> keepColumns = Enumerable.Range(1, mat.ColumnCount - 1);
            return mat.KeepColumns(keepColumns);
        }

        public static Matrix<T> ExcludeFirstRow<T>(this Matrix<T> mat) where T : struct, System.IEquatable<T>, System.IFormattable {
            IEnumerable<int> keepRows = Enumerable.Range(1, mat.RowCount - 1);
            return mat.KeepRows(keepRows);
        }

        public static Matrix<T> ExcludeColumns<T>(this Matrix<T> mat, IEnumerable<int> excludeColumns) where T : struct, System.IEquatable<T>, System.IFormattable {
            List<int> keepColumns = ComplementaryIndices(excludeColumns, mat.ColumnCount);
            return mat.KeepColumns(keepColumns);
        }

        public static Matrix<T> ExcludeRows<T>(this Matrix<T> mat, IEnumerable<int> excludeRows) where T : struct, System.IEquatable<T>, System.IFormattable {
            List<int> keepRows = ComplementaryIndices(excludeRows, mat.RowCount);
            return mat.KeepRows(keepRows);
        }

        public static Matrix<T> KeepColumns<T>(this Matrix<T> mat, IEnumerable<int> keepColumns) where T : struct, System.IEquatable<T>, System.IFormattable {
            List<int> sortedKeepColumns = keepColumns.OrderBy(x => x).ToList();
            List<T[]> newColumns = new List<T[]>();
            foreach (int col in sortedKeepColumns) {
                newColumns.Add(mat.Column(col).ToArray());
            }
            return Matrix<T>.Build.DenseOfColumnArrays(newColumns);
        }

        public static Matrix<T> KeepRows<T>(this Matrix<T> mat, IEnumerable<int> keepRows) where T : struct, System.IEquatable<T>, System.IFormattable {
            List<int> sortedKeepRows = keepRows.OrderBy(x => x).ToList();
            List<T[]> newRows = new List<T[]>();
            foreach (int row in sortedKeepRows) {
                newRows.Add(mat.Row(row).ToArray());
            }
            return Matrix<T>.Build.DenseOfRowArrays(newRows);
        }

        private static List<int> ComplementaryIndices(IEnumerable<int> givenIndices, int indexCount) {
            List<int> sortedGivenIndices = givenIndices.OrderBy(x => x).ToList();
            List<int> complementaryIndices = new List<int>();
            int nextGivenIndexInd = 0;
            for (int i = 0; i < indexCount; i++) {
                if (nextGivenIndexInd < sortedGivenIndices.Count()) {
                    if (i < sortedGivenIndices.ElementAt(nextGivenIndexInd)) { complementaryIndices.Add(i); } else { nextGivenIndexInd++; }
                } else {
                    complementaryIndices.Add(i);
                }
            }
            return complementaryIndices;
        }
    }
}
