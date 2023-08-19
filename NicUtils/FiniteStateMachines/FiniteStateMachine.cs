using System;
using System.Collections.Generic;
using System.Linq;

namespace NicUtils.FiniteStateMachines {
    // Events cannot be null, doesn't mean anything?
    // State null means Done?
    // Must define an Error state?
    public class FiniteStateMachine<TState, TEvent> where TState : notnull where TEvent : notnull {
        
        public bool HasEnded { get { return CurrentState.Equals(endState); } }

        public TState InitialState { get; private set; }

        public TState CurrentState { get; private set; }

        public HashSet<TState> AllStates { get; private set; }

        private readonly Dictionary<(TState currentState, TEvent evnt), (TState newState, Action action)> transitions;

        private readonly TState endState;

        private readonly Action resetAction;

        public FiniteStateMachine(Dictionary<(TState currentState, TEvent evnt), (TState newState, Action action)> transitions,
                                  TState initialState,
                                  Action resetAction) {
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
            } else if (!IsEndState(unexitableStates.First())) {
                throw new ArgumentException($"The END state should not be exitable!");
            }

            this.transitions = transitions;
            this.resetAction = resetAction;
            InitialState = initialState;
            CurrentState = initialState;
        }

        private static bool IsEndState(TState state) {
            return state.ToString().ToUpper().Equals("END");
        }

        public void Reset() {
            CurrentState = InitialState;
            resetAction.Invoke();
        }

        public void Accept(TEvent evnt) {
            if (HasEnded) {
                throw new IOException($"Received an event while at End state: {evnt}");
            } else { 
                if (!transitions.ContainsKey((CurrentState, evnt))) {
                    throw new IOException($"Received illegal event (\"{evnt}\") for the present state (\"{CurrentState}\")");
                }
                transitions[(CurrentState, evnt)].action.Invoke();
                CurrentState = transitions[(CurrentState, evnt)].newState;
            }
        }
    }
}
