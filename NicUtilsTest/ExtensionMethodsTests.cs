namespace NickUtilsTest;

using MathNet.Numerics.LinearAlgebra;
using NicUtils;
using System.Collections.Generic;
using static NicUtils.TestHelpers;

[TestClass]
public class ExtensionMethodsTests {

    private static int?[,] array2d = { { 1, 2 }, { 3, null } };


    private static Matrix<Double> matrix = Matrix<Double>.Build.DenseOfArray(
        new Double[,] { { 1.0, 2.0, 3.0 }, { 4.0, 5.0, 6.0 } });

    private static readonly IEnumerable<int> enumerable = ListOf(2, 1, -1, 4, 3);

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

    [TestMethod]
    public void AttemptConversion_String_Double() {
        (bool, double) conversion = "2.54".AttemptConversion<double>();
        Assert.IsTrue(conversion.Item1);
        Assert.AreEqual(2.54, conversion.Item2);
    }

    [TestMethod]
    public void AttemptConversion_EmptyString_Double() {
        (bool, double) conversion = "".AttemptConversion<double>();
        Assert.IsFalse(conversion.Item1);
    }

    [TestMethod]
    public void AttemptConversion_Whitespace_Double() {
        (bool, double) conversion = "  ".AttemptConversion<double>();
        Assert.IsFalse(conversion.Item1);
    }

    [TestMethod]
    public void AttemptConversion_String_DoubleNullable() {
        (bool, double?) conversion = "2.54".AttemptConversion<double?>();
        Assert.IsTrue(conversion.Item1);
        Assert.AreEqual(2.54, conversion.Item2);
    }

    [TestMethod]
    public void AttemptConversion_EmptyString_DoubleNullable() {
        (bool, double?) conversion = "".AttemptConversion<double?>();
        Assert.IsFalse(conversion.Item1);
        Assert.IsNull(conversion.Item2);
    }

    [TestMethod]
    public void AttemptConversion_Whitespace_DoubleNullable() {
        (bool, double?) conversion = "  ".AttemptConversion<double?>();
        Assert.IsFalse(conversion.Item1);
        Assert.IsNull(conversion.Item2);
    }

    [TestMethod]
    public void AttemptConversion_String_Int() {
        (bool, int) conversion = "2".AttemptConversion<int>();
        Assert.IsTrue(conversion.Item1);
        Assert.AreEqual(2, conversion.Item2);
    }

    [TestMethod]
    public void AttemptConversion_String_IntTruncation() {
        (bool, int) conversion = "2.78".AttemptConversion<int>();
        Assert.IsFalse(conversion.Item1);
    }

    [TestMethod]
    public void AttemptConversion_EmptyString_Int() {
        (bool, int) conversion = "".AttemptConversion<int>();
        Assert.IsFalse(conversion.Item1);
    }

    [TestMethod]
    public void AttemptConversion_Whitespace_Int() {
        (bool, int) conversion = " ".AttemptConversion<int>();
        Assert.IsFalse(conversion.Item1);
    }

    [TestMethod]
    public void AttemptConversion_String_IntNullable() {
        (bool, int?) conversion = "2".AttemptConversion<int?>();
        Assert.IsTrue(conversion.Item1);
        Assert.AreEqual(2, conversion.Item2);
    }

    [TestMethod]
    public void AttemptConversion_String_IntTruncationNullable() {
        (bool, int?) conversion = "2.78".AttemptConversion<int?>();
        Assert.IsFalse(conversion.Item1);
        Assert.IsNull(conversion.Item2);
    }

    [TestMethod]
    public void AttemptConversion_EmptyString_IntNullable() {
        (bool, int?) conversion = "".AttemptConversion<int?>();
        Assert.IsFalse(conversion.Item1);
        Assert.IsNull(conversion.Item2);
    }

    [TestMethod]
    public void AttemptConversion_Whitespace_IntNullable() {
        (bool, int?) conversion = " ".AttemptConversion<int?>();
        Assert.IsFalse(conversion.Item1);
        Assert.IsNull(conversion.Item2);
    }

    [TestMethod]
    public void AttemptConversion_Double_String() {
        (bool, string) conversion = (-2.54).AttemptConversion<string>();
        Assert.IsTrue(conversion.Item1);
        Assert.AreEqual("-2.54", conversion.Item2);
    }

    [TestMethod]
    public void AttemptConversion_ZeroesDouble_String() {
        (bool, string) conversion = (-2.54000).AttemptConversion<string>();
        Assert.IsTrue(conversion.Item1);
        Assert.AreEqual("-2.54", conversion.Item2);
    }

    [TestMethod]
    public void AttemptConversion_ZeroDouble_String() {
        (bool, string) conversion = (0.0).AttemptConversion<string>();
        Assert.IsTrue(conversion.Item1);
        Assert.AreEqual("0", conversion.Item2);
    }

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

    [TestMethod]
    public void Matrix_ToVector_TraverseRows() {
        Vector<Double> expected = Vector<Double>.Build.DenseOfEnumerable(ListOf(1.0, 2.0, 3.0, 4.0, 5.0, 6.0));
        Assert.AreEqual(expected, matrix.ToVector());
    }

    [TestMethod]
    public void Matrix_ToVector_TraverseColumns() {
        Vector<Double> expected = Vector<Double>.Build.DenseOfEnumerable(ListOf(1.0, 4.0, 2.0, 5.0, 3.0, 6.0));
        Assert.AreEqual(expected, matrix.ToVector(true));
    }

    [TestMethod]
    public void Matrix_ExcludeColumns() {
        Matrix<Double> expected = Matrix<Double>.Build.DenseOfArray(new Double[,] { { 1.0 }, { 4.0 } });
        Assert.AreEqual(expected, matrix.ExcludeColumns(Enumerable.Range(1, 2)));
    }

    [TestMethod]
    public void Matrix_ExcludeRows() {
        Matrix<Double> expected = Matrix<Double>.Build.DenseOfArray(new Double[,] { { 4.0, 5.0, 6.0 } });
        Assert.AreEqual(expected, matrix.ExcludeRows(Enumerable.Range(0, 1)));
    }

    [TestMethod]
    public void Matrix_KeepColumns() {
        Matrix<Double> expected = Matrix<Double>.Build.DenseOfArray(new Double[,] { { 2.0, 3.0 }, { 5.0, 6.0 } });
        Assert.AreEqual(expected, matrix.KeepColumns(Enumerable.Range(1, 2)));
    }

    [TestMethod]
    public void Matrix_KeepRows() {
        Matrix<Double> expected = Matrix<Double>.Build.DenseOfArray(new Double[,] { { 4.0, 5.0, 6.0 } });
        Assert.AreEqual(expected, matrix.KeepRows(Enumerable.Range(1, 1)));
    }

    [TestMethod]
    public void Matrix_Min() {
        Assert.AreEqual(1.0, matrix.Min());
    }

    [TestMethod]
    public void Matrix_Min_NullForEmptyMatrix() {
        Matrix<Double> emptyMatrix = Matrix<Double>.Build.DenseOfArray(new Double[,] { });
        Assert.IsNull(emptyMatrix.Min());
    }

    [TestMethod]
    public void Matrix_Max() {
        Assert.AreEqual(6.0, matrix.Max());
    }

    [TestMethod]
    public void Matrix_Max_NullForEmptyMatrix() {
        Matrix<Double> emptyMatrix = Matrix<Double>.Build.DenseOfArray(new Double[,] { });
        Assert.IsNull(emptyMatrix.Max());
    }
}

