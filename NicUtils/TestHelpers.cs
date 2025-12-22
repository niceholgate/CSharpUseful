using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NicUtils {
    public static class TestHelpers {
        /* 
         * Useful for testing equality of expected near-zero results.
         */
        public static void AssertEqualWithinAbsoluteTolerance(double? expected, double? actual, double absoluteTolerance) {
            Assert.IsTrue(EqualWithinAbsoluteTolerance(expected ?? Double.MinValue, actual ?? Double.MaxValue, absoluteTolerance));
        }

        public static bool EqualWithinAbsoluteTolerance(double a, double b, double absoluteTolerance) {
            if (a == b) return true;
            return Math.Abs(a - b) < absoluteTolerance;
        }

        public static void AssertEqualWithinTolerance(double? expected, double? actual, double tolerance) {
            Assert.IsTrue(EqualWithinTolerance(expected ?? Double.MinValue, actual ?? Double.MaxValue, tolerance));
        }

        public static bool EqualWithinTolerance(double a, double b, double tolerance) {
            if (a == b) return true;
            return Math.Abs((a - b) / a)  < tolerance && Math.Abs((a - b) / b) < tolerance;
        }

        public static bool AllEqual<T>(params T[] elements) {
            for (int i = 1; i < elements.Length; i++) {
                if (!elements[i].Equals(elements[0])) return false;
            }
            return true;
        }

        /*
         * Assert sequences are equal with better clarity on failures. TODO: make generic - is reference equality a concern?
         */
        public static void AssertSequencesAreEqual<T>(IEnumerable<T> seqA, IEnumerable<T> seqB) {
            Assert.AreEqual(seqA.Count(), seqB.Count());
            foreach ((T a, T b) pair in seqA.Zip(seqB, (a, b) => { return (a, b); })) {
                Assert.AreEqual(pair.a, pair.b);
            }
        }
        
        public static void AssertSequencesAreEqualWithinTolerance(IEnumerable<float> seqA, IEnumerable<float> seqB, float tolerance) {
            Assert.AreEqual(seqA.Count(), seqB.Count());
            foreach ((float a, float b) pair in seqA.Zip(seqB, (a, b) => { return (a, b); })) {
                AssertEqualWithinTolerance(pair.a, pair.b, tolerance);
            }
        }

        public static void AssertThrowsExceptionWithMessage<TExpectedException>(Action action, String expectedMessage) where TExpectedException : Exception {
            var ex = Assert.ThrowsException<TExpectedException>(action);
            Assert.AreEqual(expectedMessage, ex.Message);
        }

        /*
         * A compact way to make lists.
         */
        public static List<T> ListOf<T>(params T[] elements) {
            return new List<T>(elements);
        }
    }
}
