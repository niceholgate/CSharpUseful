using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static NicUtils.TestHelpers;
using NicUtils.FiniteStateMachines;


namespace NickUtilsTest
{
    
[TestClass]
public class PostfixCalculatorTests {
    private readonly PostfixCalculator sut = new();

    [TestMethod]
    public void TestAddition1() {
        sut.Calculate("0.2|-0.2|+");
        AssertEqualWithinAbsoluteTolerance(-0.0, sut.Result, 1E-9);
        Assert.IsNull(sut.Error);
    }

    [TestMethod]
    public void TestAddition2() {
        sut.Calculate("-8002|107|+");
        AssertEqualWithinAbsoluteTolerance(-7895, sut.Result, 1E-9);
        Assert.IsNull(sut.Error);
    }

    [TestMethod]
    public void TestSubtraction1() {
        sut.Calculate("-8002|107|-");
        AssertEqualWithinAbsoluteTolerance(-8109, sut.Result, 1E-9);
        Assert.IsNull(sut.Error);
    }

    [TestMethod]
    public void TestSubtraction2() {
        sut.Calculate("5.8|1.0008|-");
        AssertEqualWithinAbsoluteTolerance(4.7992, sut.Result, 1E-9);
        Assert.IsNull(sut.Error);
    }

    [TestMethod]
    public void TestMultiplication1() {
        sut.Calculate("-0.2|-0.2|*");
        AssertEqualWithinAbsoluteTolerance(0.04, sut.Result, 1E-9);
        Assert.IsNull(sut.Error);
    }

    [TestMethod]
    public void TestMultiplication2() {
        sut.Calculate("-8002|107|*");
        AssertEqualWithinAbsoluteTolerance(-856214, sut.Result, 1E-9);
        Assert.IsNull(sut.Error);
    }

    [TestMethod]
    public void TestDivision1() {
        sut.Calculate("5.2|2.6|/");
        AssertEqualWithinAbsoluteTolerance(2.0, sut.Result, 1E-9);
        Assert.IsNull(sut.Error);
    }

    [TestMethod]
    public void TestDivision2() {
        sut.Calculate("-8|2.0|/");
        AssertEqualWithinTolerance(-4, sut.Result, 1E-9);
        Assert.IsNull(sut.Error);
    }

    [TestMethod]
    public void TestLongCalculation_AllSuccessful() {
        sut.Calculate("-8|2.0|/|0.5|+|100|-|2|*|-0.321|-");
        AssertEqualWithinTolerance(-206.679, sut.Result, 1E-9);
        Assert.IsNull(sut.Error);
    }

    [TestMethod]
    public void TestLongCalculation_UnknownOperatorPutsLatestCalcAsResult1() {
        sut.Calculate("-8|2.0|/|0.5|+|100|-|2|*|&&&");
        AssertEqualWithinTolerance(-207, sut.Result, 1E-9);
        Assert.IsNull(sut.Error);
    }

    [TestMethod]
    public void TestLongCalculation_UnknownOperatorPutsLatestCalcAsResult2() {
        sut.Calculate("2|3|*|&2.5||||");
        AssertEqualWithinTolerance(6, sut.Result, 1E-9);
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

    [TestMethod]
    public void TestGetMermaidDiagram() {
        string diagram = sut.GetMermaidDiagram();
        string filepath = "../../../Resources/PostfixCalculatorStateDiagram.mmd";

        //using (StreamWriter outputFile = new StreamWriter(filepath)) {
        //    outputFile.WriteLine(diagram);
        //}

        List<string> generatedLines = diagram.Split('\n').ToList();
        List<string> persistedLines = new NicUtils.TextLineReader(filepath).GetData();

        NicUtils.TestHelpers.AssertSequencesAreEqual(generatedLines, persistedLines);
    }
}
}