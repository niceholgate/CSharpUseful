using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace NicUtils.ExtensionMethods {
    public static class MatrixExtensions {
        public static Vector<T> ToVector<T>(this Matrix<T> mat, bool traverseColumns = false) where T : struct, IEquatable<T>, IFormattable {
            IEnumerable<T> unrolled = mat.ToRowArrays().Unroll2DEnumerable(traverseColumns);
            return Vector<T>.Build.DenseOfEnumerable(unrolled);
        }

        public static Matrix<T> ExcludeColumns<T>(this Matrix<T> mat, IEnumerable<int> excludeColumns) where T : struct, IEquatable<T>, IFormattable {
            IEnumerable<int> keepColumns = Enumerable.Range(0, mat.ColumnCount).Except(excludeColumns);
            return mat.KeepColumns(keepColumns);
        }

        public static Matrix<T> ExcludeRows<T>(this Matrix<T> mat, IEnumerable<int> excludeRows) where T : struct, IEquatable<T>, IFormattable {
            IEnumerable<int> keepRows = Enumerable.Range(0, mat.RowCount).Except(excludeRows);
            return mat.KeepRows(keepRows);
        }

        public static Matrix<T> KeepColumns<T>(this Matrix<T> mat, IEnumerable<int> keepColumns) where T : struct, IEquatable<T>, IFormattable {
            List<int> sortedKeepColumns = keepColumns.OrderBy(x => x).ToList();
            List<T[]> newColumns = new();
            foreach (int col in sortedKeepColumns) {
                newColumns.Add(mat.Column(col).ToArray());
            }
            return Matrix<T>.Build.DenseOfColumnArrays(newColumns);
        }

        public static Matrix<T> KeepRows<T>(this Matrix<T> mat, IEnumerable<int> keepRows) where T : struct, IEquatable<T>, IFormattable {
            List<int> sortedKeepRows = keepRows.OrderBy(x => x).ToList();
            List<T[]> newRows = new();
            foreach (int row in sortedKeepRows) {
                newRows.Add(mat.Row(row).ToArray());
            }
            return Matrix<T>.Build.DenseOfRowArrays(newRows);
        }

        public static double? Min(this Matrix<double> mat) {
            if (mat.RowCount == 0) return null;
            double min = double.MaxValue;
            foreach (Vector<double> row in mat.EnumerateRows()) {
                foreach (double el in row) {
                    if (el < min) min = el;
                }
            }
            return min;
        }

        public static double? Max(this Matrix<double> mat) {
            if (mat.RowCount == 0) return null;
            double max = double.MinValue;
            foreach (Vector<double> row in mat.EnumerateRows()) {
                foreach (double el in row) {
                    if (el > max) max = el;
                }
            }
            return max;
        }
    }
}
