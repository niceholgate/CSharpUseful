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

        private TState endState;

        public FiniteStateMachine(Dictionary<(TState currentState, TEvent evnt), (TState newState, Action action)> transitions, TState initialState) {
            AllStates = transitions.Keys
                .Select(k => k.currentState)
                .Union(transitions.Values
                    .Select(v => v.newState)
                    .OfType<TState>())
                .ToHashSet();

            // Validate that there is one "End" state
            if (AllStates.Where(state => state.ToString() == "End").Count() != 1) {
                throw new ArgumentException("Expected exactly one End state");
            } else {
                endState = AllStates.Where(state => state.ToString() == "End").First();
            }
            
            // Validate initialState is in AllStates
            if (!AllStates.Contains(initialState)) {
                throw new ArgumentException("The requested initialState is not among the states defined in the transitions matrix.");
            }

            // Validate all states can be exited (except for End)
            HashSet<TState> exitableStates = transitions.Keys
                .Select(k => k.currentState)
                .ToHashSet();
            HashSet<TState> unexitableStates = AllStates.Except(exitableStates).ToHashSet();
            if (unexitableStates.Count() != 1 || unexitableStates.First().ToString() != "End") {
                throw new ArgumentException($"The following states cannot be exited (only End should be un-exitable): {String.Join(", ", unexitableStates)}");
            }

            this.transitions = transitions;
            InitialState = initialState;
            CurrentState = initialState;
        }

        public void Reset() {
            CurrentState = InitialState;
        }

        public void Accept(TEvent evnt) {
            if (CurrentState != null) { 
                transitions[(CurrentState, evnt)].action.Invoke();
                CurrentState = transitions[(CurrentState, evnt)].newState;
            }
        }
    }
}
