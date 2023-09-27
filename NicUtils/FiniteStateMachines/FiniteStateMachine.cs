
namespace NicUtils.FiniteStateMachines {
    // Events cannot be null, doesn't mean anything?
    // State null means Done?
    // Must define an Error state?
    public class FiniteStateMachine<TState, TEvent> : AbstractFiniteStateMachine<TState, TEvent, Action> {

        private readonly Action resetAction;

        public FiniteStateMachine(Dictionary<(TState currentState, TEvent evnt), (TState newState, Action action)> transitions,
                                  TState initialState,
                                  Action resetAction) : base(transitions, initialState) {
            this.resetAction = resetAction;
        }

        public void Reset() {
            CurrentState = InitialState;
            resetAction.Invoke();
        }

        public override void Accept(TEvent evnt) {
            if (HasEnded) {
                throw new IOException($"Received an event while at End state: {evnt}");
            } else if (!transitions.ContainsKey((CurrentState, evnt))) {
                throw new IOException($"Received illegal event (\"{evnt}\") for the present state (\"{CurrentState}\")");
            } else { 
                transitions[(CurrentState, evnt)].action.Invoke();
                CurrentState = transitions[(CurrentState, evnt)].newState;
            }
        }
    }
}
