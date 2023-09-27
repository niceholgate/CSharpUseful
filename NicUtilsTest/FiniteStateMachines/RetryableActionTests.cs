namespace NickUtilsTest;

using NicUtils.FiniteStateMachines;
using System.Threading.Tasks;
using static NicUtils.FiniteStateMachines.RetryableAction;
using static NicUtils.TestHelpers;

[TestClass]
public class RetryableActionTests {

    static bool delay100ThenTrue(CancellationToken cancellationToken) {
        Thread.Sleep(100);
        return !cancellationToken.IsCancellationRequested;
    }

    Func<CancellationToken, bool> delayThenTrueFunc = delay100ThenTrue;

    [TestMethod]
    public async Task TestAttemptSucceeded() {
        RetryableAction action = new(
            delayThenTrueFunc,
            new int[] { 1000, 2000 },
            new int[] { 200 });

        AttemptOutcome outcome = await action.AttemptAsync();
        Assert.IsTrue(outcome.Succeeded);
        Assert.IsFalse(outcome.ShouldRetry);
        Assert.AreEqual(1, outcome.AttemptNumber);
        Assert.AreEqual("Attempt 1 succeeded", outcome.Message);
    }

    [TestMethod]
    public async Task TestAttemptFailedNoRetry() {
        RetryableAction action = new(
            delayThenTrueFunc,
            new int[] { 50 },
            new int[] { });

        AttemptOutcome outcome = await action.AttemptAsync();
        Assert.IsFalse(outcome.Succeeded);
        Assert.IsFalse(outcome.ShouldRetry);
        Assert.AreEqual(1, outcome.AttemptNumber);
        Assert.AreEqual("Attempt 1 failed", outcome.Message);
    }

    [TestMethod]
    public async Task TestAttemptFailedShouldRetryThenSucceeds() {
        RetryableAction action = new(
            delayThenTrueFunc,
            new int[] { 50, 200 },
            new int[] {  });

        AttemptOutcome outcome = await action.AttemptAsync();
        Assert.IsFalse(outcome.Succeeded);
        Assert.IsFalse(outcome.ShouldRetry);
        Assert.AreEqual(1, outcome.AttemptNumber);
        Assert.AreEqual("Attempt 1 failed", outcome.Message);
    }


}
