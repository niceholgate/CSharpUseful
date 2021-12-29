using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using System.ComponentModel;

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

    public static class MiscExtensions {
        public static T ConvertTo<T>(this object value) {
            T returnValue;

            if (value is T variable)
                returnValue = variable;
            else
                try {
                    //Handling Nullable types i.e, int?, double?, bool? .. etc
                    if (Nullable.GetUnderlyingType(typeof(T)) != null) {
                        TypeConverter conv = TypeDescriptor.GetConverter(typeof(T));
                        returnValue = (T)conv.ConvertFrom(value);
                    } else {
                        returnValue = (T)Convert.ChangeType(value, typeof(T));
                    }
                } catch (Exception) {
                    returnValue = default(T);
                }

            return returnValue;
        }

        public static int IndexOfMax<T>(this IEnumerable<T> enumerable) where T : IComparable<T> {
            if (enumerable.Count() == 0) {
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
            if (enumerable.Count() == 0) {
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
            List<int> keepColumns = Enumerables<int>.ComplementaryIndices(excludeColumns, mat.ColumnCount);
            return mat.KeepColumns(keepColumns);
        }

        public static Matrix<T> ExcludeRows<T>(this Matrix<T> mat, IEnumerable<int> excludeRows) where T : struct, System.IEquatable<T>, System.IFormattable {
            List<int> keepRows = Enumerables<int>.ComplementaryIndices(excludeRows, mat.RowCount);
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

        public static double Min(this Matrix<double> mat) {
            double min = double.MaxValue;
            for (int i = 0; i < mat.RowCount; i++) {
                for (int j = 0; j < mat.ColumnCount; j++) {
                    double newEl = mat.Row(i).ElementAt(j);
                    if (newEl < min) { min = newEl; };
                }
            }
            return min;
        }

        public static double Max(this Matrix<double> mat) {
            double max = double.MinValue;
            for (int i = 0; i < mat.RowCount; i++) {
                for (int j = 0; j < mat.ColumnCount; j++) {
                    double newEl = mat.Row(i).ElementAt(j);
                    if (newEl > max) { max = newEl; };
                }
            }
            return max;
        }
    }
}
