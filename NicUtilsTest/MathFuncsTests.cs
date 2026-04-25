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
        var coords1 = ( 0.0f, 0.0f );
        var coords2 = ( 1.0f, 2.0f );
        AssertEqualWithinTolerance(5.0, Distances2D.GetDistance(coords1, coords2, Distances2D.HeuristicType.EuclidianSquared), 1E-9f);
        AssertEqualWithinTolerance(MathF.Sqrt(5.0f), Distances2D.GetDistance(coords1, coords2, Distances2D.HeuristicType.Euclidian), 1E-9f);
        AssertEqualWithinTolerance(MathF.Sqrt(2.0f) + 1.0f, Distances2D.GetDistance(coords1, coords2, Distances2D.HeuristicType.Octile), 1E-9f);
        AssertEqualWithinTolerance(3.0, Distances2D.GetDistance(coords1, coords2, Distances2D.HeuristicType.Manhattan), 1E-9f);
    }
}
}