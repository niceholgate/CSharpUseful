using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NickUtilsTest {

using static NicUtils.TestHelpers;
using NicUtils.ExtensionMethods;

[TestClass]
public class Array2DExtensionsTests {

    private static readonly int?[,] array2d = { { 1, 2 }, { 3, null } };

    [TestMethod]
    public void Array2D_SliceRow() {
        AssertSequencesAreEqual(array2d.SliceRow(1), ListOf<int?>(3, null));
    }

    [TestMethod]
    public void Array2D_SliceColumn() {
        AssertSequencesAreEqual(array2d.SliceColumn(1), ListOf<int?>(2, null));
    }

    [TestMethod]
    public void Array2D_ToJagged() {
        int?[][] jagged = array2d.ToJagged();
        AssertSequencesAreEqual(jagged[0], ListOf<int?>(1, 2));
        AssertSequencesAreEqual(jagged[1], ListOf<int?>(3, null));
    }

    [TestMethod]
    public void Array2D_Unroll2DMultiDimArray_TraverseRows() {
        AssertSequencesAreEqual(array2d.Unroll2DMultiDimArray(), ListOf<int?>(1, 2, 3, null));
    }

    [TestMethod]
    public void Array2D_Unroll2DMultiDimArray_TraverseColumns() {
        AssertSequencesAreEqual(array2d.Unroll2DMultiDimArray(true), ListOf<int?>(1, 3, 2, null));
    }
}
}