using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NicUtils.ExtensionMethods {
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
}
