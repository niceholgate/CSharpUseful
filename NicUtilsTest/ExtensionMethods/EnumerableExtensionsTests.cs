namespace NicUtils.ExtensionMethods;

using MathNet.Numerics.LinearAlgebra;
using static NicUtils.TestHelpers;

[TestClass]
public class EnumerableExtensionsTests {

    private static readonly IEnumerable<int> enumerable = ListOf(2, 1, -1, 4, 3);

    [TestMethod]
    public void Enumerable_IndexOfMax() {
        Assert.AreEqual(enumerable.IndexOfMax(), 3);
    }

    [TestMethod]
    public void Enumerable_IndexOfMax_EmptyEnumerable() {
        Assert.AreEqual(ListOf<int>().IndexOfMax(), -1);
    }

    [TestMethod]
    public void Enumerable_IndexOfMin() {
        Assert.AreEqual(enumerable.IndexOfMin(), 2);
    }

    [TestMethod]
    public void Enumerable_IndexOfMin_EmptyEnumerable() {
        Assert.AreEqual(ListOf<int>().IndexOfMin(), -1);
    }

    [TestMethod]
    public void Enumerable_ContainedBy_True() {
        Assert.IsTrue(enumerable.ContainedBy(ListOf(ListOf(1, 2, 3), new List<int>(enumerable))));
    }

    [TestMethod]
    public void Enumerable_ContainedBy_False() {
        Assert.IsFalse(enumerable.ContainedBy(ListOf(ListOf(1, 2, 3), ListOf(1, 2, 2))));
    }

    [TestMethod]
    public void Enumerable_Unroll2DEnumerable_TraverseRows() {
        AssertSequencesAreEqual(ListOf(1, 2, 3, 4, 5), ListOf(ListOf(1, 2, 3), ListOf(4, 5)).Unroll2DEnumerable());
    }

    [TestMethod]
    public void Enumerable_Unroll2DEnumerable_TraverseColumns() {
        AssertSequencesAreEqual(ListOf(1, 4, 2, 5, 3), ListOf(ListOf(1, 2, 3), ListOf(4, 5)).Unroll2DEnumerable(true));
    }
}
