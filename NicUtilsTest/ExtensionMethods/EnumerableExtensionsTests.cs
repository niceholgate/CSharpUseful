

using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using static NicUtils.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NicUtils.ExtensionMethods
{


    [TestClass]
    public class EnumerableExtensionsTests
    {

        private static readonly List<int> enumerable = ListOf(2, 1, -1, 4, 3);

        [TestMethod]
        public void Enumerable_IndexOfMax()
        {
            Assert.AreEqual(3, enumerable.IndexOfMax());
        }

        [TestMethod]
        public void Enumerable_IndexOfMax_EmptyEnumerable()
        {
            Assert.AreEqual(-1, ListOf<int>().IndexOfMax());
        }

        [TestMethod]
        public void Enumerable_IndexOfMin()
        {
            Assert.AreEqual(2, enumerable.IndexOfMin());
        }

        [TestMethod]
        public void Enumerable_IndexOfMin_EmptyEnumerable()
        {
            Assert.AreEqual(-1, ListOf<int>().IndexOfMin());
        }

        [TestMethod]
        public void Enumerable_ContainedBy_True()
        {
            Assert.IsTrue(enumerable.ContainedBy(ListOf(ListOf(1, 2, 3), new List<int>(enumerable))));
        }

        [TestMethod]
        public void Enumerable_ContainedBy_False()
        {
            Assert.IsFalse(enumerable.ContainedBy(ListOf(ListOf(1, 2, 3), ListOf(1, 2, 2))));
        }

        [TestMethod]
        public void Enumerable_Unroll2DEnumerable_TraverseRows()
        {
            AssertSequencesAreEqual(ListOf(1, 2, 3, 4, 5), ListOf(ListOf(1, 2, 3), ListOf(4, 5)).Unroll2DEnumerable());
        }

        [TestMethod]
        public void Enumerable_Unroll2DEnumerable_TraverseColumns()
        {
            AssertSequencesAreEqual(ListOf(1, 4, 2, 5, 3),
                ListOf(ListOf(1, 2, 3), ListOf(4, 5)).Unroll2DEnumerable(true));
        }
    }
}