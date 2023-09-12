namespace NickUtilsTest;

using static NicUtils.TestHelpers;


[TestClass]
public class TestHelpersTests {
    [TestMethod]
    public void EqualWithinAbsoluteTolerance_True() {
        Assert.IsTrue(EqualWithinAbsoluteTolerance(2.4, 2.45, 0.1));
    }

    [TestMethod]
    public void EqualWithinAbsoluteTolerance_False() {
        Assert.IsFalse(EqualWithinAbsoluteTolerance(2.4, 2.55, 0.1));
    }

    [TestMethod]
    public void EqualWithinTolerance_True() {
        Assert.IsTrue(EqualWithinTolerance(15, 16.5, 0.11));
    }

    [TestMethod]
    public void EqualWithinTolerance_False() {
        Assert.IsFalse(EqualWithinTolerance(15, 16.5, 0.09));
    }

    [TestMethod]
    public void ListOf_Many() {
        AssertSequencesAreEqual(new List<int?>() { 1, 3, null }, ListOf<int?>(1, 3, null));
    }

    [TestMethod]
    public void ListOf_One() {
        AssertSequencesAreEqual(new List<int?>() { -7 }, ListOf<int?>(-7));
    }

    [TestMethod]
    public void ListOf_None() {
        AssertSequencesAreEqual(new List<int?>(), ListOf<int?>());
    }
}