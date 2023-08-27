namespace NickUtilsTest;

using Microsoft.VisualStudio.TestPlatform.Utilities;
using NicUtils;
using System.Numerics;
using static NicUtils.TestHelpers;

[TestClass]
public class ExtensionMethodsTests {
    [TestMethod]
    public void ConvertTo_String_Double() {
        (bool, double) conversion = "2.54".AttemptConversion<double>();
        Assert.IsTrue(conversion.Item1);
        Assert.AreEqual(2.54, conversion.Item2);
    }

    [TestMethod]
    public void ConvertTo_EmptyString_Double() {
        (bool, double) conversion = "".AttemptConversion<double>();
        Assert.IsFalse(conversion.Item1);
    }

    [TestMethod]
    public void ConvertTo_Whitespace_Double() {
        (bool, double) conversion = "  ".AttemptConversion<double>();
        Assert.IsFalse(conversion.Item1);
    }

    [TestMethod]
    public void ConvertTo_String_DoubleNullable() {
        (bool, double?) conversion = "2.54".AttemptConversion<double?>();
        Assert.IsTrue(conversion.Item1);
        Assert.AreEqual(2.54, conversion.Item2);
    }

    [TestMethod]
    public void ConvertTo_EmptyString_DoubleNullable() {
        (bool, double?) conversion = "".AttemptConversion<double?>();
        Assert.IsFalse(conversion.Item1);
        Assert.IsNull(conversion.Item2);
    }

    [TestMethod]
    public void ConvertTo_Whitespace_DoubleNullable() {
        (bool, double?) conversion = "  ".AttemptConversion<double?>();
        Assert.IsFalse(conversion.Item1);
        Assert.IsNull(conversion.Item2);
    }

    [TestMethod]
    public void ConvertTo_String_Int() {
        (bool, int) conversion = "2".AttemptConversion<int>();
        Assert.IsTrue(conversion.Item1);
        Assert.AreEqual(2, conversion.Item2);
    }

    [TestMethod]
    public void ConvertTo_String_IntTruncation() {
        (bool, int) conversion = "2.78".AttemptConversion<int>();
        Assert.IsFalse(conversion.Item1);
    }

    [TestMethod]
    public void ConvertTo_EmptyString_Int() {
        (bool, int) conversion = "".AttemptConversion<int>();
        Assert.IsFalse(conversion.Item1);
    }

    [TestMethod]
    public void ConvertTo_Whitespace_Int() {
        (bool, int) conversion = " ".AttemptConversion<int>();
        Assert.IsFalse(conversion.Item1);
    }

    [TestMethod]
    public void ConvertTo_String_IntNullable() {
        (bool, int?) conversion = "2".AttemptConversion<int?>();
        Assert.IsTrue(conversion.Item1);
        Assert.AreEqual(2, conversion.Item2);
    }

    [TestMethod]
    public void ConvertTo_String_IntTruncationNullable() {
        (bool, int?) conversion = "2.78".AttemptConversion<int?>();
        Assert.IsFalse(conversion.Item1);
        Assert.IsNull(conversion.Item2);
    }

    [TestMethod]
    public void ConvertTo_EmptyString_IntNullable() {
        (bool, int?) conversion = "".AttemptConversion<int?>();
        Assert.IsFalse(conversion.Item1);
        Assert.IsNull(conversion.Item2);
    }

    [TestMethod]
    public void ConvertTo_Whitespace_IntNullable() {
        (bool, int?) conversion = " ".AttemptConversion<int?>();
        Assert.IsFalse(conversion.Item1);
        Assert.IsNull(conversion.Item2);
    }

    [TestMethod]
    public void ConvertTo_Double_String() {
        (bool, string) conversion = (-2.54).AttemptConversion<string>();
        Assert.IsTrue(conversion.Item1);
        Assert.AreEqual("-2.54", conversion.Item2);
    }

    [TestMethod]
    public void ConvertTo_ZeroesDouble_String() {
        (bool, string) conversion = (-2.54000).AttemptConversion<string>();
        Assert.IsTrue(conversion.Item1);
        Assert.AreEqual("-2.54", conversion.Item2);
    }

    [TestMethod]
    public void ConvertTo_ZeroDouble_String() {
        (bool, string) conversion = (0.0).AttemptConversion<string>();
        Assert.IsTrue(conversion.Item1);
        Assert.AreEqual("0", conversion.Item2);
    }
}

