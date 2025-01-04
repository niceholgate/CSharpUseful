namespace NickUtilsTest;

using System;
using System.Collections.Generic;
using System.Linq;

using static NicUtils.TestHelpers;
using NicUtils.FiniteStateMachines;


[TestClass]
public class FiniteStateMachineTests {

    private static readonly List<String> eventHistory = new();

    private static readonly List<String> stateHistory = new() { "initial" };

    private static readonly Dictionary<(String, String), (String, Action)> transitionsGood = new() {
            { ("initial", "eventA"), ("interim", ()=>{eventHistory.Add("A"); stateHistory.Add("interim"); }) },
            { ("interim", "eventA"), ("initial", ()=>{eventHistory.Add("A"); stateHistory.Add("initial"); }) },
            { ("interim", "eventB"), ("end", ()=>{eventHistory.Add("B"); stateHistory.Add("end"); }) }
        };

    [TestMethod]
    public void TestHappyPathToEndStateAndReset() {
        FiniteStateMachine<String, String> sut = new(transitionsGood, "initial", () => { eventHistory.Clear(); stateHistory.Clear(); stateHistory.Add("initial"); });

        sut.Accept("eventA");
        sut.Accept("eventA");
        sut.Accept("eventA");
        sut.Accept("eventB");

        Assert.IsTrue(sut.HasEnded);
        Assert.IsTrue(eventHistory.SequenceEqual(ListOf("A", "A", "A", "B")));
        AssertSequencesAreEqual(stateHistory, ListOf("initial", "interim", "initial", "interim", "end"));

        sut.Reset();
        Assert.IsFalse(sut.HasEnded);
        Assert.IsTrue(eventHistory.SequenceEqual(ListOf<string>()));
        Assert.IsTrue(stateHistory.SequenceEqual(ListOf("initial")));
    }

    [TestMethod]
    public void TestInit_UnknownInitialState() {
        AssertThrowsExceptionWithMessage<ArgumentException>(() => { FiniteStateMachine<String, String> sut = new(transitionsGood, "unknown_initial", () => {}); },
            "The requested initialState (\"unknown_initial\") is not among the states defined in the transitions matrix.");
    }

    [TestMethod]
    public void TestInit_NoEndState() {
        Dictionary<(String, String), (String, Action)> transitionsNoEndState = new() {
            { ("initial", "eventA"), ("interim", () => {}) },
            { ("interim", "eventA"), ("initial", () => {}) }
        };

        AssertThrowsExceptionWithMessage<ArgumentException>(() => {FiniteStateMachine<String, String> sut = new(transitionsNoEndState, "initial", () => {});},
            "There is no END state!");
    }

    [TestMethod]
    public void TestInit_AllNonEndStatesMustBeExitable() {
        Dictionary<(String, String), (String, Action)> transitionsCantExitInterims = new() {
            { ("initial", "eventA"), ("interim1", () => {}) },
            { ("initial", "eventB"), ("interim2", () => {}) },
            { ("initial", "eventC"), ("end", () => {}) }
        };
        AssertThrowsExceptionWithMessage<ArgumentException>(() => { FiniteStateMachine<String, String> sut = new(transitionsCantExitInterims, "initial", () => {}); },
            "The following states cannot be exited (only the END state should be un-exitable): interim1, interim2, end");
    }

    [TestMethod]
    public void TestInit_EndStateMustNotBeExitable() {
        Dictionary<(String, String), (String, Action)> transitionsCanExitEndState = new() {
            { ("initial", "eventA"), ("interim1", () => {}) },
            { ("initial", "eventB"), ("end", () => {}) },
            { ("end", "eventC"), ("interim1", () => {}) }
        };
        AssertThrowsExceptionWithMessage<ArgumentException>(() => { FiniteStateMachine<String, String> sut = new(transitionsCanExitEndState, "initial", () => { }); },
            "The END state should not be exitable!");
    }

    [TestMethod]
    public void TestGetMermaidDiagram() {
        FiniteStateMachine<String, String> sut = new(transitionsGood, "initial", () => { eventHistory.Clear(); stateHistory.Clear(); stateHistory.Add("initial"); });

        string diagram = sut.GetMermaidDiagram();
        string filepath = "../../../Resources/RegressionTestStateDiagram.mmd";

        //using (StreamWriter outputFile = new StreamWriter(filepath)) {
        //    outputFile.WriteLine(diagram);
        //}

        List<string> generatedLines = diagram.Split('\n').ToList();
        List<string> persistedLines = new NicUtils.TextLineReader(filepath).GetData();

        NicUtils.TestHelpers.AssertSequencesAreEqual(generatedLines, persistedLines);
    }
}

