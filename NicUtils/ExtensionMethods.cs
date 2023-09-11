using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using System.ComponentModel;

namespace NicUtils {
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
    }

    public static class MiscExtensions {

        public static (bool success, T result) AttemptConversion<T>(this object input) {
            if (input is T variable)
                return (true, variable);
            else
                try {
                    // Handling Nullable types i.e, int?, double?, bool? .. etc
                    if (Nullable.GetUnderlyingType(typeof(T)) != null) {
                        TypeConverter conv = TypeDescriptor.GetConverter(typeof(T));
                        T val = (T)conv.ConvertFrom(input);
                        if (val == null) { return  (false, val); }
                        return (true, val);
                    } else {
                        return (true, (T)Convert.ChangeType(input, typeof(T)));
                    }
                } catch (Exception) {
                    return (false, default(T));
                }
        }
    }

    public static class EnumerableExtensions {

        public static int IndexOfMax<T>(this IEnumerable<T> enumerable) where T : IComparable<T> {
            if (!enumerable.Any()) {
                return -1;
            } else {
                IComparer<T> comparer = Comparer<T>.Default;
                int indexOfMax = 0;
                T max = enumerable.ElementAt(0);
                for (int i = 1; i < enumerable.Count(); i++) {
                    if (comparer.Compare(enumerable.ElementAt(i), max) > 0) {
                        max = enumerable.ElementAt(i);
                        indexOfMax = i;
                    }
                }
                return indexOfMax;
            }
        }

        public static int IndexOfMin<T>(this IEnumerable<T> enumerable) where T : IComparable<T> {
            if (!enumerable.Any()) {
                return -1;
            } else {
                IComparer<T> comparer = Comparer<T>.Default;
                int indexOfMin = 0;
                T min = enumerable.ElementAt(0);
                for (int i = 1; i < enumerable.Count(); i++) {
                    if (comparer.Compare(enumerable.ElementAt(i), min) < 0) {
                        min = enumerable.ElementAt(i);
                        indexOfMin = i;
                    }
                }
                return indexOfMin;
            }
        }

        public static bool ContainedBy<T>(this IEnumerable<T> searchEnum, IEnumerable<IEnumerable<T>> enumOfEnums) {
            foreach (IEnumerable<T> e in enumOfEnums) {
                if (e.SequenceEqual(searchEnum)) return true;
            }
            return false;
        }

        public static IEnumerable<T> Unroll2DEnumerable<T>(this IEnumerable<IEnumerable<T>> enumOfEnums, bool traverseColumns = false) {
            List<T> list = new();
            if (traverseColumns) {
                int maxInnerEnumLength = (from e in enumOfEnums select e.Count()).Max();
                for (int col = 0; col < maxInnerEnumLength; col++) {
                    foreach (IEnumerable<T> e in enumOfEnums) {
                        if (col < e.Count()) list.Add(e.ElementAt(col));
                    }
                }
            } else {
                foreach (IEnumerable<T> e in enumOfEnums) {
                    list.AddRange(e);
                }
            }
            return list;
        }
    }

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
            List<T[]> newRows = new List<T[]>();
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
