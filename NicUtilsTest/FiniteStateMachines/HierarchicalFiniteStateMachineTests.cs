using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NicUtils.FiniteStateMachines;
using static NicUtils.TestHelpers;

namespace NickUtilsTest {
    [TestClass]
    public class HierarchicalFiniteStateMachineTests {
        public enum State { Off, On, On_Idle, On_Working, End }
        public enum Event { PowerOn, PowerOff, Work, Stop, Finish }

        private List<string> log = new();

        private void Log(string msg) => log.Add(msg);

        [TestMethod]
        public void TestHSM_FullWorkflow() {
            var transitions = new Dictionary<(State, Event), (State, Expression<Action>)> {
                { (State.Off, Event.PowerOn), (State.On, () => Log("Action: Powering On")) },
                { (State.On_Idle, Event.Work), (State.On_Working, () => Log("Action: Starting Work")) },
                { (State.On_Working, Event.Stop), (State.On_Idle, () => Log("Action: Stopping Work")) },
                { (State.On, Event.PowerOff), (State.Off, () => Log("Action: Powering Off")) },
                { (State.Off, Event.Finish), (State.End, () => Log("Action: Finishing")) }
            };

            var parents = new Dictionary<State, State> {
                { State.On_Idle, State.On },
                { State.On_Working, State.On }
            };

            var initialSubStates = new Dictionary<State, State> {
                { State.On, State.On_Idle }
            };

            var entryActions = new Dictionary<State, Expression<Action>> {
                { State.Off, () => Log("Entry: Off") },
                { State.On, () => Log("Entry: On") },
                { State.On_Idle, () => Log("Entry: On_Idle") },
                { State.On_Working, () => Log("Entry: On_Working") }
            };

            var exitActions = new Dictionary<State, Expression<Action>> {
                { State.Off, () => Log("Exit: Off") },
                { State.On, () => Log("Exit: On") },
                { State.On_Idle, () => Log("Exit: On_Idle") },
                { State.On_Working, () => Log("Exit: On_Working") }
            };

            var hsm = new HierarchicalFiniteStateMachine<State, Event>(
                transitions,
                State.Off,
                () => log.Clear(),
                parents,
                initialSubStates,
                entryActions,
                exitActions
            );

            // Initial state is Off. EnterState(Off) should have been called in constructor.
            Assert.AreEqual(State.Off, hsm.CurrentState);
            AssertSequencesAreEqual(ListOf("Entry: Off"), log);
            log.Clear();

            // Transition Off -> On. 
            // Should: Exit Off, Transition Action, Entry On, Entry On_Idle (Deep Entry)
            hsm.Accept(Event.PowerOn);
            Assert.AreEqual(State.On_Idle, hsm.CurrentState);
            AssertSequencesAreEqual(ListOf(
                "Exit: Off",
                "Action: Powering On",
                "Entry: On",
                "Entry: On_Idle"
            ), log);
            log.Clear();

            // Transition On_Idle -> On_Working
            // LCA of On_Idle and On_Working is On.
            // Should: Exit On_Idle, Transition Action, Entry On_Working
            hsm.Accept(Event.Work);
            Assert.AreEqual(State.On_Working, hsm.CurrentState);
            AssertSequencesAreEqual(ListOf(
                "Exit: On_Idle",
                "Action: Starting Work",
                "Entry: On_Working"
            ), log);
            log.Clear();

            // Bubbling: PowerOff is defined on On. Current state is On_Working.
            // Should: Bubble to On. LCA of On_Working and Off is implicit root (null).
            // Should: Exit On_Working, Exit On, Transition Action, Entry Off.
            hsm.Accept(Event.PowerOff);
            Assert.AreEqual(State.Off, hsm.CurrentState);
            AssertSequencesAreEqual(ListOf(
                "Exit: On_Working",
                "Exit: On",
                "Action: Powering Off",
                "Entry: Off"
            ), log);
            log.Clear();
        }

        [TestMethod]
        public void TestHSM_ToMermaidDiagram_AllCombinations() {
            var transitions = new Dictionary<(State, Event), (State, Expression<Action>)> {
                { (State.Off, Event.PowerOn), (State.On, () => Log("Action: Powering On")) },
                { (State.On, Event.PowerOff), (State.Off, () => Log("Action: Powering Off")) },
                { (State.Off, Event.Finish), (State.End, () => Log("Action: Finishing")) }
            };

            var entryActions = new Dictionary<State, Expression<Action>> {
                { State.On, () => Log("Entry: On") },
                { State.On_Working, () => Log("Entry: On_Working") }
            };

            var exitActions = new Dictionary<State, Expression<Action>> {
                { State.On_Idle, () => Log("Exit: On_Idle") },
                { State.On_Working, () => Log("Exit: On_Working") }
            };

            var hsm = new HierarchicalFiniteStateMachine<State, Event>(
                transitions,
                State.Off,
                () => log.Clear(),
                entryActions: entryActions,
                exitActions: exitActions,
                allStates: new HashSet<State> { State.Off, State.On, State.On_Idle, State.On_Working, State.End }
            );

            string diagram = hsm.ToMermaidDiagram();
            
            // Verify decorations are GONE
            Assert.IsTrue(diagram.Contains("Off"), "Off should be in diagram");
            Assert.IsTrue(diagram.Contains("On"), "On should be in diagram");
            Assert.IsFalse(diagram.Contains("entry/"), "No entry decorations should be present");
            Assert.IsFalse(diagram.Contains("exit/"), "No exit decorations should be present");
            Assert.IsTrue(diagram.Contains("Off --> On:"), "On should be used in a transition without quotes");

            // Verify CSV
            string csv = hsm.ToActionsCsv();
            Assert.IsTrue(csv.Contains("State,Entry Action,Exit Action"));
            Assert.IsTrue(csv.Contains("On,\"Log(\"\"Entry: On\"\")\","));
            Assert.IsTrue(csv.Contains("On_Idle,,\"Log(\"\"Exit: On_Idle\"\")\""));
            Assert.IsTrue(csv.Contains("On_Working,\"Log(\"\"Entry: On_Working\"\")\",\"Log(\"\"Exit: On_Working\"\")\""));

            // Verify Round-trip with CSV
            var hsm2 = HierarchicalFiniteStateMachine<State, Event>.FromMermaidDiagram(diagram, csv, instanceContext: new[] { this });
            Assert.AreEqual(State.Off, hsm2.CurrentState);
            
            // Perform transitions to verify actions were recovered from CSV
            log.Clear();
            hsm2.Accept(Event.PowerOn);
            Assert.AreEqual(State.On, hsm2.CurrentState);
            AssertSequencesAreEqual(ListOf("Action: Powering On", "Entry: On"), log);
        }

        [TestMethod]
        public void TestHSM_ToMermaidDiagram() {
            var transitions = new Dictionary<(State, Event), (State, Expression<Action>)> {
                { (State.Off, Event.PowerOn), (State.On, () => Log("Action: Powering On")) },
                { (State.On_Idle, Event.Work), (State.On_Working, () => Log("Action: Starting Work")) },
                { (State.On_Working, Event.Stop), (State.On_Idle, () => Log("Action: Stopping Work")) },
                { (State.On, Event.PowerOff), (State.Off, () => Log("Action: Powering Off")) },
                { (State.Off, Event.Finish), (State.End, () => Log("Action: Finishing")) }
            };

            var parents = new Dictionary<State, State> {
                { State.On_Idle, State.On },
                { State.On_Working, State.On }
            };

            var initialSubStates = new Dictionary<State, State> {
                { State.On, State.On_Idle }
            };

            var entryActions = new Dictionary<State, Expression<Action>> {
                { State.Off, () => Log("Entry: Off") },
                { State.On, () => Log("Entry: On") },
                { State.On_Idle, () => Log("Entry: On_Idle") },
                { State.On_Working, () => Log("Entry: On_Working") }
            };

            var exitActions = new Dictionary<State, Expression<Action>> {
                { State.Off, () => Log("Exit: Off") },
                { State.On, () => Log("Exit: On") },
                { State.On_Idle, () => Log("Exit: On_Idle") },
                { State.On_Working, () => Log("Exit: On_Working") }
            };

            var hsm = new HierarchicalFiniteStateMachine<State, Event>(
                transitions,
                State.Off,
                () => log.Clear(),
                parents,
                initialSubStates,
                entryActions,
                exitActions
            );

            string diagram = hsm.ToMermaidDiagram();
            string filepath = "../../../Resources/RegressionTestHSMDiagram.mmd";
            
            List<string> generatedLines = diagram.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.TrimEnd()).ToList();
            List<string> persistedLines = new NicUtils.TextLineReader(filepath).GetData()
                .Select(l => l.TrimEnd()).ToList();

            AssertSequencesAreEqual(persistedLines, generatedLines);
        }

        [TestMethod]
        public void TestHSM_FromMermaidDiagram() {
            string diagramPath = "../../../Resources/RegressionTestHSMDiagram.mmd";
            string actionsPath = "../../../Resources/RegressionTestHSMActions.csv";
            
            string mermaid = new NicUtils.TextLineReader(diagramPath).GetJoinedLines();
            string actions = new NicUtils.TextLineReader(actionsPath).GetJoinedLines();

            var hsm = HierarchicalFiniteStateMachine<State, Event>.FromMermaidDiagram(mermaid, actions, instanceContext: new[] { this });

            Assert.AreEqual(State.Off, hsm.CurrentState);
            log.Clear();

            // Perform a series of transitions to verify it works as expected
            hsm.Accept(Event.PowerOn);
            Assert.AreEqual(State.On_Idle, hsm.CurrentState);
            AssertSequencesAreEqual(ListOf(
                "Exit: Off",
                "Action: Powering On",
                "Entry: On",
                "Entry: On_Idle"
            ), log);
        }
    }
}
