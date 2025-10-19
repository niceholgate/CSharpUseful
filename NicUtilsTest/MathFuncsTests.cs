using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NickUtilsTest {

using static NicUtils.TestHelpers;
using NicUtils;


[TestClass]
public class MathFuncsTests {

    [TestMethod]
    public void Sigmoid_HalfAtZero() {
        AssertEqualWithinTolerance(0.5, Sigmoid.Value(0.0), 1E-9);
    }

    [TestMethod]
    public void Sigmoid_OneAtInf() {
        AssertEqualWithinTolerance(1, Sigmoid.Value(Double.MaxValue), 1E-9);
    }

    [TestMethod]
    public void Sigmoid_ZeroAtNegInf() {
        AssertEqualWithinTolerance(0, Sigmoid.Value(Double.MinValue), 1E-9);
    }

    [TestMethod]
    public void SigmoidDerivative_OneQuarterAtZero() {
        AssertEqualWithinTolerance(0.25, Sigmoid.Derivative(0), 1E-9);
        AssertEqualWithinTolerance(0.25, Sigmoid.DerivativeFromValue(0.5), 1E-9);
    }

    [TestMethod]
    public void SigmoidDerivative_ZeroAtInf() {
        AssertEqualWithinAbsoluteTolerance(0, Sigmoid.Derivative(Double.MaxValue), 1E-9);
        AssertEqualWithinAbsoluteTolerance(0, Sigmoid.DerivativeFromValue(0.999999999), 1E-9);
    }

    [TestMethod]
    public void SigmoidDerivative_ZeroAtNegInf() {
        AssertEqualWithinAbsoluteTolerance(0, Sigmoid.Derivative(Double.MinValue), 1E-9);
        AssertEqualWithinAbsoluteTolerance(0, Sigmoid.DerivativeFromValue(0.00000000001), 1E-9);
    }

    [TestMethod]
    public void HeuristicDistances() {
        (double, double) coords1 = ( 0.0, 0.0 );
        (double, double) coords2 = ( 1.0, 2.0 );
        AssertEqualWithinTolerance(Math.Sqrt(5.0), Distances2D.GetDistance(coords1, coords2, Distances2D.HeuristicType.Euclidian), 1E-9);
        AssertEqualWithinTolerance(Math.Sqrt(2.0) + 1.0, Distances2D.GetDistance(coords1, coords2, Distances2D.HeuristicType.Octile), 1E-9);
        AssertEqualWithinTolerance(3.0, Distances2D.GetDistance(coords1, coords2, Distances2D.HeuristicType.Manhattan), 1E-9);
    }
}
}