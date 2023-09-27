using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NicUtils.FiniteStateMachines {
    public abstract class AbstractFiniteStateMachine<TState, TEvent, TAction>
        where TState : notnull where TEvent : notnull {

        public bool HasEnded { get { return CurrentState.Equals(endState); } }

        public TState InitialState { get; private set; }

        public TState CurrentState { get; protected set; }

        public HashSet<TState> AllStates { get; private set; }

        protected readonly Dictionary<(TState currentState, TEvent evnt), (TState newState, TAction action)> transitions;

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
            } else if (!IsEndState(unexitableStates.First())) {
                throw new ArgumentException($"The END state should not be exitable!");
            }

            this.transitions = transitions;
            InitialState = initialState;
            CurrentState = initialState;
        }
        
        //private static void ValidateTransitions

        public abstract void Accept(TEvent evnt);

        protected static bool IsEndState(TState state) {
            return state.ToString().ToUpper().Equals("END");
        }
    }
}
