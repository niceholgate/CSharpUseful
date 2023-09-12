namespace NicUtilsTest.ExtensionMethods;

using MathNet.Numerics.LinearAlgebra;
using NicUtils.ExtensionMethods;
using static NicUtils.TestHelpers;

[TestClass]
public class MatrixExtensionsTests {

    private static readonly MatrixBuilder<Double> Matrix = Matrix<Double>.Build;

    private static readonly VectorBuilder<Double> Vector = Vector<Double>.Build;

    private static readonly Matrix<Double> testMatrix = Matrix.DenseOfArray(
        new Double[,] { { 1.0, 2.0, 3.0 }, { 4.0, 5.0, 6.0 } });

    [TestMethod]
    public void Matrix_ToVector_TraverseRows() {
        Vector<Double> expected = Vector.DenseOfEnumerable(ListOf(1.0, 2.0, 3.0, 4.0, 5.0, 6.0));
        Assert.AreEqual(expected, testMatrix.ToVector());
    }

    [TestMethod]
    public void Matrix_ToVector_TraverseColumns() {
        Vector<Double> expected = Vector.DenseOfEnumerable(ListOf(1.0, 4.0, 2.0, 5.0, 3.0, 6.0));
        Assert.AreEqual(expected, testMatrix.ToVector(true));
    }

    [TestMethod]
    public void Matrix_ExcludeColumns() {
        Matrix<Double> expected = Matrix.DenseOfArray(new Double[,] { { 1.0 }, { 4.0 } });
        Assert.AreEqual(expected, testMatrix.ExcludeColumns(Enumerable.Range(1, 2)));
    }

    [TestMethod]
    public void Matrix_ExcludeRows() {
        Matrix<Double> expected = Matrix.DenseOfArray(new Double[,] { { 4.0, 5.0, 6.0 } });
        Assert.AreEqual(expected, testMatrix.ExcludeRows(Enumerable.Range(0, 1)));
    }

    [TestMethod]
    public void Matrix_KeepColumns() {
        Matrix<Double> expected = Matrix.DenseOfArray(new Double[,] { { 2.0, 3.0 }, { 5.0, 6.0 } });
        Assert.AreEqual(expected, testMatrix.KeepColumns(Enumerable.Range(1, 2)));
    }

    [TestMethod]
    public void Matrix_KeepRows() {
        Matrix<Double> expected = Matrix.DenseOfArray(new Double[,] { { 4.0, 5.0, 6.0 } });
        Assert.AreEqual(expected, testMatrix.KeepRows(Enumerable.Range(1, 1)));
    }

    [TestMethod]
    public void Matrix_Min() {
        Assert.AreEqual(1.0, testMatrix.Min());
    }

    [TestMethod]
    public void Matrix_Min_NullForEmptyMatrix() {
        Matrix<Double> emptyMatrix = Matrix.DenseOfArray(new Double[,] { });
        Assert.IsNull(emptyMatrix.Min());
    }

    [TestMethod]
    public void Matrix_Max() {
        Assert.AreEqual(6.0, testMatrix.Max());
    }

    [TestMethod]
    public void Matrix_Max_NullForEmptyMatrix() {
        Matrix<Double> emptyMatrix = Matrix.DenseOfArray(new Double[,] { });
        Assert.IsNull(emptyMatrix.Max());
    }
}
