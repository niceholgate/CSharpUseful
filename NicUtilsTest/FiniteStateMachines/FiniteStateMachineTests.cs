using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NickUtilsTest {

using System;
using System.Collections.Generic;
using System.Linq;

using static NicUtils.TestHelpers;
using NicUtils.FiniteStateMachines;


[TestClass]
public class FiniteStateMachineTests {

    public enum TestStateEnum { initial, interim, end, interim1, interim2, unknown_initial }
    public enum TestEventEnum { eventA, eventB, eventC }

    private static readonly List<TestEventEnum> eventHistory = new();

    private static readonly List<TestStateEnum> stateHistory = new() { TestStateEnum.initial };

    private readonly Dictionary<(TestStateEnum, TestEventEnum), (TestStateEnum, Expression<Action>)> transitionsGood = new() {
            { (TestStateEnum.initial, TestEventEnum.eventA), (TestStateEnum.interim, ()=>UpdateHistories(TestEventEnum.eventA, TestStateEnum.interim)) },
            { (TestStateEnum.interim, TestEventEnum.eventA), (TestStateEnum.initial, ()=>UpdateHistories(TestEventEnum.eventA, TestStateEnum.initial)) },
            { (TestStateEnum.interim, TestEventEnum.eventB), (TestStateEnum.end, ()=>UpdateHistories(TestEventEnum.eventB, TestStateEnum.end)) }
        };

    static void UpdateHistories(TestEventEnum evnt, TestStateEnum newState) {
        eventHistory.Add(evnt);
        stateHistory.Add(newState);
    }

    static void ResetEventAndStateHistories() {
        eventHistory.Clear();
        stateHistory.Clear();
        stateHistory.Add(TestStateEnum.initial);
    }

    static void NoOp() { }
    
    [TestMethod]
    public void TestHappyPathToEndStateAndReset() {
        FiniteStateMachine<TestStateEnum, TestEventEnum> sut = new(transitionsGood, TestStateEnum.initial, () => ResetEventAndStateHistories());

        sut.Accept(TestEventEnum.eventA);
        sut.Accept(TestEventEnum.eventA);
        sut.Accept(TestEventEnum.eventA);
        sut.Accept(TestEventEnum.eventB);

        Assert.IsTrue(sut.HasEnded);
        Assert.IsTrue(eventHistory.SequenceEqual(ListOf(TestEventEnum.eventA, TestEventEnum.eventA, TestEventEnum.eventA, TestEventEnum.eventB)));
        AssertSequencesAreEqual(stateHistory, ListOf(TestStateEnum.initial, TestStateEnum.interim, TestStateEnum.initial, TestStateEnum.interim, TestStateEnum.end));

        sut.Reset();
        Assert.IsFalse(sut.HasEnded);
        Assert.IsTrue(eventHistory.SequenceEqual(ListOf<TestEventEnum>()));
        Assert.IsTrue(stateHistory.SequenceEqual(ListOf(TestStateEnum.initial)));
    }

    [TestMethod]
    public void TestInit_UnknownInitialState() {
        AssertThrowsExceptionWithMessage<ArgumentException>(() => { FiniteStateMachine<TestStateEnum, TestEventEnum> sut = new(transitionsGood, TestStateEnum.unknown_initial, () => NoOp()); },
            "The requested initialState (\"unknown_initial\") is not among the states defined in the transitions matrix.");
    }

    [TestMethod]
    public void TestInit_NoEndState() {
        Dictionary<(TestStateEnum, TestEventEnum), (TestStateEnum, Expression<Action>)> transitionsNoEndState = new() {
            { (TestStateEnum.initial, TestEventEnum.eventA), (TestStateEnum.interim, () => NoOp()) },
            { (TestStateEnum.interim, TestEventEnum.eventA), (TestStateEnum.initial, () => NoOp()) }
        };

        AssertThrowsExceptionWithMessage<ArgumentException>(() => {FiniteStateMachine<TestStateEnum, TestEventEnum> sut = new(transitionsNoEndState, TestStateEnum.initial, () => NoOp());},
            "There is no END state!");
    }

    [TestMethod]
    public void TestInit_AllNonEndStatesMustBeExitable() {
        Dictionary<(TestStateEnum, TestEventEnum), (TestStateEnum, Expression<Action>)> transitionsCantExitInterims = new() {
            { (TestStateEnum.initial, TestEventEnum.eventA), (TestStateEnum.interim1, () => NoOp()) },
            { (TestStateEnum.initial, TestEventEnum.eventB), (TestStateEnum.interim2, () => NoOp()) },
            { (TestStateEnum.initial, TestEventEnum.eventC), (TestStateEnum.end, () => NoOp()) }
        };
        AssertThrowsExceptionWithMessage<ArgumentException>(() => { FiniteStateMachine<TestStateEnum, TestEventEnum> sut = new(transitionsCantExitInterims, TestStateEnum.initial, () => NoOp()); },
            "The following states cannot be exited (only the END state should be un-exitable): interim1, interim2, end");
    }

    [TestMethod]
    public void TestInit_EndStateMustNotBeExitable() {
        Dictionary<(TestStateEnum, TestEventEnum), (TestStateEnum, Expression<Action>)> transitionsCanExitEndState = new() {
            { (TestStateEnum.initial, TestEventEnum.eventA), (TestStateEnum.interim1, () => NoOp()) },
            { (TestStateEnum.initial, TestEventEnum.eventB), (TestStateEnum.end, () => NoOp()) },
            { (TestStateEnum.end, TestEventEnum.eventC), (TestStateEnum.interim1, () => NoOp()) }
        };
        AssertThrowsExceptionWithMessage<ArgumentException>(() => { FiniteStateMachine<TestStateEnum, TestEventEnum> sut = new(transitionsCanExitEndState, TestStateEnum.initial, () => NoOp()); },
            "The END state should not be exitable!");
    }

    [TestMethod]
    public void TestGetMermaidDiagram() {
        FiniteStateMachine<TestStateEnum, TestEventEnum> sut = new(transitionsGood, TestStateEnum.initial, () => ResetEventAndStateHistories());

        string diagram = sut.ToMermaidDiagram();
        string filepath = "../../../Resources/RegressionTestStateDiagram.mmd";

        //using (StreamWriter outputFile = new StreamWriter(filepath)) {
        //    outputFile.WriteLine(diagram);
        //}

        List<string> generatedLines = diagram.Split('\n').ToList();
        List<string> persistedLines = new NicUtils.TextLineReader(filepath).GetData();

        AssertSequencesAreEqual(generatedLines, persistedLines);
    }

    [TestMethod]
    public void TestFromMermaidDiagram() {
        string filepath = "../../../Resources/RegressionTestStateDiagram.mmd";
        string mermaid = new NicUtils.TextLineReader(filepath).GetJoinedLines();

        ResetEventAndStateHistories();
        var sut = FiniteStateMachine<TestStateEnum, TestEventEnum>.FromMermaidDiagram(mermaid, typeContext: new[] { typeof(FiniteStateMachineTests) });

        Assert.AreEqual(TestStateEnum.initial, sut.CurrentState);
        Assert.IsFalse(sut.HasEnded);

        Assert.AreEqual(transitionsGood.Count, sut.transitions.Count);
        foreach (var entry in transitionsGood) {
            Assert.IsTrue(sut.transitions.ContainsKey(entry.Key));
            Assert.AreEqual(entry.Value.Item1, sut.transitions[entry.Key].newState);
            Assert.AreEqual(entry.Value.Item2.ToString(), sut.transitions[entry.Key].action.ToString());
        }

        // Verify actions are actually parsed and executable
        sut.Accept(TestEventEnum.eventA);
        Assert.AreEqual(TestStateEnum.interim, sut.CurrentState);
        Assert.IsTrue(eventHistory.Contains(TestEventEnum.eventA));
        Assert.IsTrue(stateHistory.Contains(TestStateEnum.interim));
    }

    [TestMethod]
    public void TestFromMermaidDiagram_InvalidHeader() {
        string mermaid = @"invalid-header
                            [*] --> initial
                            initial --> end: event";

        AssertThrowsExceptionWithMessage<ArgumentException>(() => {
            FiniteStateMachine<TestStateEnum, TestEventEnum>.FromMermaidDiagram(mermaid);
        }, "Invalid Mermaid diagram header. Expected 'stateDiagram-v2', but found 'invalid-header'.");
    }
}

}
