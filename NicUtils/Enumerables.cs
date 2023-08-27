using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NicUtils {
    public class Enumerables<T> {
        public static bool ContainsEnumerable(IEnumerable<IEnumerable<T>> enumOfEnums,
                                                IEnumerable<T> searchEnum) {
            foreach (IEnumerable<T> e in enumOfEnums) {
                if (e.SequenceEqual(searchEnum)) return true;
            }
            return false;
        }

        //public static T[][] MultiDimToJagged2D(T[,] multiDimArray) {
        //    int rows = multiDimArray.GetLength(0);
        //    T[][] jagged = new T[rows][];
        //    for (int i = 0; i < rows; i++) {
        //        jagged[0] = multiDimArray.SliceRow(i).ToArray();
        //    }
        //    return jagged;
        //}

        //public static List<T> Unroll2DMultiDimArray(T[,] multiDimArray, bool traverseColumnsFirst = false) {
        //    List<T> list = new();
        //    int rows = multiDimArray.GetLength(0);
        //    int cols = multiDimArray.GetLength(1);
        //    if (traverseColumnsFirst) {
        //        for (int j = 0; j < cols; j++) {
        //            for (int i = 0; i < rows; i++) {
        //                list.Add(multiDimArray[i, j]);
        //            }
        //        }
        //    } else {
        //        for (int i = 0; i < rows; i++) {
        //            for (int j = 0; j < cols; j++) {
        //                list.Add(multiDimArray[i, j]);
        //            }
        //        }
        //    }
        //    return list;
        //}

        public static List<T> Unroll2DEnumerable(IEnumerable<IEnumerable<T>> enumOfEnums, bool traverseColumnsFirst = false) {
            List<T> list = new();
            if (traverseColumnsFirst) {
                int[] innerEnumLengths = (from i in Enumerable.Range(0, enumOfEnums.Count()) select enumOfEnums.ElementAt(i).Count()).ToArray();
                for (int col = 0; col < innerEnumLengths.Max(); col++) {
                    foreach (IEnumerable<T> e in enumOfEnums) {
                        if (col < e.Count()) list.Add(e.ElementAt(col));
                    }
                }
                innerEnumLengths[1] = innerEnumLengths[1] + 1;
            } else {
                foreach (IEnumerable<T> e in enumOfEnums) {
                    list = list.Concat(e).ToList();
                }
            }

            return list;
        }

        public static IEnumerable<int> ComplementaryIndices(IEnumerable<int> givenIndices, int indexCount) {
            List<int> sortedGivenIndices = givenIndices.OrderBy(x => x).ToList();
            List<int> complementaryIndices = new();
            int nextGivenIndexInd = 0;
            for (int i = 0; i < indexCount; i++) {
                if (nextGivenIndexInd < sortedGivenIndices.Count) {
                    if (i < sortedGivenIndices.ElementAt(nextGivenIndexInd)) {
                        complementaryIndices.Add(i);
                    } else {
                        nextGivenIndexInd++;
                    }
                } else {
                    complementaryIndices.Add(i);
                }
            }
            return complementaryIndices;
        }
    }
}
