using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace NicUtils.FiniteStateMachines {
    // TODO: what is the purpose of this rather than just a consolidated FiniteStateMachine class?
    public abstract class AbstractFiniteStateMachine<TState, TEvent, TAction>
        where TState : notnull where TEvent : notnull {

        public bool HasEnded { get { return CurrentState.Equals(endState); } }

        public TState InitialState { get; private set; }

        public TState CurrentState { get; protected set; }

        public HashSet<TState> AllStates { get; private set; }

        public readonly Dictionary<(TState currentState, TEvent evnt), (TState newState, TAction action)> transitions;

        protected readonly TState endState;

        public AbstractFiniteStateMachine(Dictionary<(TState currentState, TEvent evnt), (TState newState, TAction action)> transitions,
                                          TState initialState) {
            AllStates = transitions.Keys
                .Select(k => k.currentState)
                .Union(transitions.Values
                    .Select(v => v.newState)
                    .OfType<TState>())
                .ToHashSet();

            // Validate initialState is in AllStates
            if (!AllStates.Contains(initialState)) {
                throw new ArgumentException($"The requested initialState (\"{initialState}\") is not among the states defined in the transitions matrix.");
            }

            // Validate that there is an "END" state
            IEnumerable<TState> endStates = AllStates.Where(IsEndState);
            if (endStates.Count() != 1) {
                throw new ArgumentException("There is no END state!");
            } else {
                endState = endStates.First();
            }

            // Validate END state cannot be exited, but all other states can be
            HashSet<TState> exitableStates = transitions
                .Where(transition => !transition.Key.currentState.Equals(transition.Value.newState))
                .Select(transition => transition.Key.currentState)
                .ToHashSet();
            HashSet<TState> unexitableStates = AllStates.Except(exitableStates).ToHashSet();

            if (unexitableStates.Count != 1) {
                throw new ArgumentException($"The following states cannot be exited (only the END state should be un-exitable): {String.Join(", ", unexitableStates)}");
            }
            if (!IsEndState(unexitableStates.First())) {
                throw new ArgumentException($"The END state should not be exitable!");
            }

            this.transitions = transitions;
            InitialState = initialState;
            CurrentState = initialState;
        }
        
        // TODO: private static void ValidateTransitions

        public abstract void Accept(TEvent evnt);

        protected static bool IsEndState(TState state) {
            return state.ToString().ToUpper().Equals("END");
        }

        public string ToMermaidDiagram() {
            StringBuilder diagram = new StringBuilder();

            diagram.Append("stateDiagram-v2");
            diagram.Append('\n');
            diagram.Append($"    [*] --> {InitialState}");
            diagram.Append('\n');

            foreach (var item in transitions) {
                diagram.Append($"    {item.Key.currentState} --> {item.Value.newState}: {item.Key.evnt}");
                diagram.Append('\n');
            }
            diagram.Remove(diagram.Length-1, 1);

            return diagram.ToString();
        }

        public static FiniteStateMachine<string, string> FromMermaidDiagram(string diagramCode) {
            var transitions = new Dictionary<(string currentState, string evnt), (string newState, Action action)>();
            string initialState = null;
            Action noop = () => { };

            string[] lines = diagramCode.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            
            bool foundHeader = false;
            foreach (var line in lines) {
                string trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine)) continue;

                if (!foundHeader) {
                    if (trimmedLine != "stateDiagram-v2") {
                        throw new ArgumentException($"Invalid Mermaid diagram header. Expected 'stateDiagram-v2', but found '{trimmedLine}'.");
                    }
                    foundHeader = true;
                    continue;
                }

                var transitionMatch = Regex.Match(trimmedLine, @"^([\w\[\]\*]+)\s*-->\s*([\w\[\]\*]+)(?:\s*:\s*(.*))?$");
                if (transitionMatch.Success) {
                    string currentState = transitionMatch.Groups[1].Value;
                    string newState = transitionMatch.Groups[2].Value;
                    string evnt = transitionMatch.Groups[3].Value;

                    if (currentState == "[*]") {
                        initialState = newState;
                        continue;
                    }

                    if (newState == "[*]") {
                        newState = "end";
                    }

                    if (string.IsNullOrEmpty(evnt))
                    {
                        throw new ArgumentException("Must specify an event that causes this state transition.");
                    }

                    transitions[(currentState, evnt)] = (newState, noop);

                    if (initialState == null && currentState != "[*]") initialState = currentState;
                }
            }

            if (initialState == null) {
                throw new ArgumentException("Could not determine initial state from Mermaid diagram.");
            }

            return new FiniteStateMachine<string, string>(transitions, initialState, noop);
        }
    }
}
