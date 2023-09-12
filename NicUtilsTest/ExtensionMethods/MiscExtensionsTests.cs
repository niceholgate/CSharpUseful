namespace NickUtilsTest;

using NicUtils.ExtensionMethods;

[TestClass]
public class MiscExtensionsTests {
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
}
