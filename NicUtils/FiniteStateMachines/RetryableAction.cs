using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NicUtils.FiniteStateMachines {
    public class RetryableAction {
        public int AttemptCount { get; private set; } = 0;

        public int? MaxAttempts { get { return MaxAttemptDurationsMilliseconds.Length; } }

        public int[] MaxAttemptDurationsMilliseconds { get; private set; }

        public int[] RetryDelayDurationsMilliseconds { get; private set; }

        public long LastFailedAttemptTimeUnixMs { get; private set; }

        public long TimeUntilNextAttemptAllowed { get {
                return Math.Max(LastFailedAttemptTimeUnixMs + RetryDelayDurationsMilliseconds.ElementAt(AttemptCount) - DateTimeOffset.UtcNow.ToUnixTimeSeconds(), 0);
            } }

        // TODO: is one input right?
        private readonly Func<CancellationToken, bool> failableTask;

        public RetryableAction(Func<CancellationToken, bool> failableTask,
                                int[] maxAttemptDurationsMilliseconds,
                                int[] retryDelayDurationsMilliseconds) {
            this.failableTask = failableTask;
            MaxAttemptDurationsMilliseconds = maxAttemptDurationsMilliseconds;
            RetryDelayDurationsMilliseconds = retryDelayDurationsMilliseconds;
            if (RetryDelayDurationsMilliseconds.Length != MaxAttempts - 1) {
                throw new ArgumentException("There should be one fewer retry delay than maximum attempts");
            }
        }

        /*
         * Returns true if the task needs another attempt.
         */
        public async Task<AttemptOutcome> AttemptAsync() {
            if (AttemptCount >= MaxAttempts) return new AttemptOutcome(false, false, AttemptCount, "Exceeded allowed attempts");
            
            CancellationTokenSource cts = new();
            cts.CancelAfter(MaxAttemptDurationsMilliseconds[AttemptCount]);
            
            AttemptCount++;
            bool succeeded = await Task.Run(() => failableTask(cts.Token));

            if (succeeded) {
                // In case the action is performed again later, need to reset the AttemptCount
                int successAttemptCount = AttemptCount;
                AttemptCount = 0;
                return new AttemptOutcome(true, false, successAttemptCount, $"Attempt {successAttemptCount} succeeded");
            }
            LastFailedAttemptTimeUnixMs = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return new AttemptOutcome(false, AttemptCount < MaxAttempts, AttemptCount, $"Attempt {AttemptCount} failed");
        }

        public readonly struct AttemptOutcome {
            public readonly bool Succeeded { get; }
            public readonly bool ShouldRetry { get; }
            public readonly int AttemptNumber { get; }
            public readonly string Message { get; }

            public AttemptOutcome(bool succeeded, bool shouldRetry, int attemptNumber, string message) {
                Succeeded = succeeded;
                ShouldRetry = shouldRetry;
                AttemptNumber = attemptNumber;
                Message = message;
            }
        }
    }
}
