namespace NickUtilsTest;

using NicUtils.FiniteStateMachines;


[TestClass]
public class PostfixCalculatorTests {
    private readonly PostfixCalculator sut = new();

    [TestMethod]
    public void TestAddition1() {
        sut.Calculate("0.2|-0.2|+");
        AssertApproxEqual(-0.0, sut.Result);
        Assert.IsNull(sut.Error);
    }

    [TestMethod]
    public void TestAddition2() {
        sut.Calculate("-8002|107|+");
        AssertApproxEqual(-7895, sut.Result);
        Assert.IsNull(sut.Error);
    }

    [TestMethod]
    public void TestSubtraction1() {
        sut.Calculate("-8002|107|-");
        AssertApproxEqual(-8109, sut.Result);
        Assert.IsNull(sut.Error);
    }

    [TestMethod]
    public void TestSubtraction2() {
        sut.Calculate("5.8|1.0008|-");
        AssertApproxEqual(4.7992, sut.Result);
        Assert.IsNull(sut.Error);
    }

    [TestMethod]
    public void TestMultiplication1() {
        sut.Calculate("-0.2|-0.2|*");
        AssertApproxEqual(0.04, sut.Result);
        Assert.IsNull(sut.Error);
    }

    [TestMethod]
    public void TestMultiplication2() {
        sut.Calculate("-8002|107|*");
        AssertApproxEqual(-856214, sut.Result);
        Assert.IsNull(sut.Error);
    }

    [TestMethod]
    public void TestDivision1() {
        sut.Calculate("5.2|2.6|/");
        AssertApproxEqual(2.0, sut.Result);
        Assert.IsNull(sut.Error);
    }

    [TestMethod]
    public void TestDivision2() {
        sut.Calculate("-8|2.0|/");
        AssertApproxEqual(-4, sut.Result);
        Assert.IsNull(sut.Error);
    }

    [TestMethod]
    public void TestLongCalculation_AllSuccessful() {
        sut.Calculate("-8|2.0|/|0.5|+|100|-|2|*|-0.321|-");
        AssertApproxEqual(-206.679, sut.Result);
        Assert.IsNull(sut.Error);
    }

    [TestMethod]
    public void TestLongCalculation_UnknownOperatorPutsLatestCalcAsResult1() {
        sut.Calculate("-8|2.0|/|0.5|+|100|-|2|*|&&&");
        AssertApproxEqual(-207, sut.Result);
        Assert.IsNull(sut.Error);
    }

    [TestMethod]
    public void TestLongCalculation_UnknownOperatorPutsLatestCalcAsResult2() {
        sut.Calculate("2|3|*|&2.5||||");
        AssertApproxEqual(6, sut.Result);
        Assert.IsNull(sut.Error);
    }

    [TestMethod]
    public void TestEmptyPostfixStringSetsPostfixError() {
        sut.Calculate("");
        Assert.IsNull(sut.Result);
        Assert.AreEqual("Bad postfix expression: there should be at least 2 delimiters (|)", sut.Error);
    }

    [TestMethod]
    public void TestNoDelimitersPostfixStringSetsPostfixError() {
        sut.Calculate("2.5*1.9");
        Assert.IsNull(sut.Result);
        Assert.AreEqual("Bad postfix expression: there should be at least 2 delimiters (|)", sut.Error);
    }

    [TestMethod]
    public void TestTooFewDelimitersPostfixStringSetsPostfixError() {
        sut.Calculate("2.5|1.9");
        Assert.IsNull(sut.Result);
        Assert.AreEqual("Bad postfix expression: there should be at least 2 delimiters (|)", sut.Error);
    }

    [TestMethod]
    public void TestMisplacedOperatorSetsPostfixError() {
        sut.Calculate("-8|2.0|/|0.5|+|100|-|+");
        Assert.IsNull(sut.Result);
        Assert.AreEqual("Bad postfix expression: unexpected operator \"+\" in position 7", sut.Error);
    }

    [TestMethod]
    public void TestMisplacedOperandSetsPostfixError() {
        sut.Calculate("-8|2.0|/|0.5|+|100|-|2|88");
        Assert.IsNull(sut.Result);
        Assert.AreEqual("Bad postfix expression: unexpected operand \"88\" in position 8", sut.Error);
    }

    [TestMethod]
    public void TestUnknownOperatorWithDanglingOperandSetsPostfixError() {
        sut.Calculate("-8|2.0|/|0.5|+|100|-|2|*|-0.321|&&&");
        Assert.IsNull(sut.Result);
        Assert.AreEqual("Bad postfix expression: unknown symbol \"&&&\" in position 10", sut.Error);
    }

    private static void AssertApproxEqual(double? expected, double? actual) {
        Assert.IsTrue(ApproxEqual(expected ?? Double.MinValue, actual ?? Double.MaxValue));
    }

    private static bool ApproxEqual(double a, double b) {
        return Math.Abs(a - b) / ((a + b + 0.00000001) / 2) < 0.00000001;
    }
}