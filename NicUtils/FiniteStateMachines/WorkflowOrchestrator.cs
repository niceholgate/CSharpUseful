using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace NicUtils.FiniteStateMachines {
    /*
    * TODO: Capabilities for a WorkflowOrchestrator:
    * -Count retries
    * -Max duration
    * -Rollback
    * -Accumulate state 
    * -History
    */



    public class WorkflowOrchestrator<TState, TEvent> : AbstractFiniteStateMachine<TState, TEvent, (RetryableAction forwardAction, RetryableAction rollbackAction)> {

        public WorkflowStatus Status { get; private set; }

        private List<TState> stateHistory = new();
        private List<TEvent> eventHistory = new();
        private List<int> attemptCounts = new();
        private List<int> rollbackAttemptCounts = new();
        private List<RetryableAction> rollbackActions = new();

        public WorkflowOrchestrator(Dictionary<(TState currentState, TEvent evnt), (TState newState, (RetryableAction action, RetryableAction rollback) action)> transitions,
                                  TState initialState) : base(transitions, initialState) {
            Status = WorkflowStatus.RUNNING;
            stateHistory.Add(initialState);
        }

        public enum WorkflowStatus {
            RUNNING,
            SUCCEEDED,
            ROLLBACK_RUNNING,
            ROLLBACK_SUCCEEDED,
            ROLLBACK_FAILED
        }

        // TODO: need to lock
        public async override void Accept(TEvent evnt) {
            eventHistory.Add(evnt);
            if (HasEnded) {
                throw new IOException($"Received an event while at End state: {evnt}");
            } else if (!transitions.ContainsKey((CurrentState, evnt))) {
                throw new IOException($"Received illegal event (\"{evnt}\") for the present state (\"{CurrentState}\")");
            } else {
                RetryableAction forwardAction = transitions[(CurrentState, evnt)].action.forwardAction;
                RetryableAction rollbackAction = transitions[(CurrentState, evnt)].action.rollbackAction;
                RetryableAction.AttemptOutcome outcome = await forwardAction.AttemptAsync();
                while (outcome.ShouldRetry) outcome = await forwardAction.AttemptAsync();
                attemptCounts.Add(outcome.AttemptNumber);

                // For now assume that you always rollback if any action fails permanently
                if (outcome.Succeeded) {
                    if (rollbackAction != null) rollbackActions.Add(rollbackAction);
                    CurrentState = transitions[(CurrentState, evnt)].newState;
                    stateHistory.Add(CurrentState);
                    if (HasEnded) Status = WorkflowStatus.SUCCEEDED;
                } else {
                 
                    Rollback();
                }
            }
        }

        private async void Rollback() {
            // Perform rollback actions in reverse order to forward actions
            Status = WorkflowStatus.ROLLBACK_RUNNING;
            for (int i = rollbackActions.Count - 1; i >= 0; i--) {
                RetryableAction.AttemptOutcome outcome = await rollbackActions[i].AttemptAsync();
                while (outcome.ShouldRetry) outcome = await rollbackActions[i].AttemptAsync();
                rollbackAttemptCounts.Add(outcome.AttemptNumber);

                // If rollback fails, persist the error then finish
                if (!outcome.Succeeded) {
                    Status = WorkflowStatus.ROLLBACK_FAILED;
                    return;
                }
            }
            Status = WorkflowStatus.ROLLBACK_SUCCEEDED;
        }
    }
}
