using System;
using System.Collections.Generic;
using System.Linq;

namespace NicUtils.FiniteStateMachines
{
    public class PostfixCalculator
    {

        public static readonly char delimiter = '|';

        public static readonly Dictionary<string, Func<double, double, double>> knownOperators = new() {
            { "+", (l, r) => Math.FusedMultiplyAdd(1.0, l, r) },
            { "-", (l, r) => Math.FusedMultiplyAdd(-1.0, r, l) },
            { "*", (l, r) => Math.FusedMultiplyAdd(l, r, 0.0) },
            { "/", (l, r) => Math.FusedMultiplyAdd(l, 1 / r, 0.0) }};

        public double? Result { get; private set; } = null;

        public string Error { get; private set; } = null;

        private enum PostfixState
        {
            End,
            NeitherOperand,
            LeftOperand,
            BothOperands
        }

        private enum PostfixEvent
        {
            Operand,
            Operator,
            Unknown
        }

        private readonly FiniteStateMachine<PostfixState, PostfixEvent> stateMachine;

        private readonly Dictionary<(PostfixState currentState, PostfixEvent evnt), (PostfixState newState, Action action)> stateTransitions;

        private double left;

        private double right;

        private Func<double, double, double> currentOperator = knownOperators.Values.First();

        private int lastEventPosition = -1;

        private string lastRawEvent = "";

        private double lastDouble = 0.0;

        public PostfixCalculator() {
            stateTransitions = new() {
                    { (PostfixState.NeitherOperand, PostfixEvent.Operand), (PostfixState.LeftOperand, SetLeft) },
                    { (PostfixState.NeitherOperand, PostfixEvent.Operator), (PostfixState.End, SetErrorUnexpectedOperator) },
                    { (PostfixState.NeitherOperand, PostfixEvent.Unknown), (PostfixState.End, SetErrorUnknownSymbol) },

                    { (PostfixState.LeftOperand, PostfixEvent.Operand), (PostfixState.BothOperands, SetRight) },
                    { (PostfixState.LeftOperand, PostfixEvent.Operator), (PostfixState.End, SetErrorUnexpectedOperator) },
                    { (PostfixState.LeftOperand, PostfixEvent.Unknown), (PostfixState.End, SetResult) },

                    { (PostfixState.BothOperands, PostfixEvent.Operand), (PostfixState.End, SetErrorUnexpectedOperand) },
                    { (PostfixState.BothOperands, PostfixEvent.Operator), (PostfixState.LeftOperand, CalcLeft) },
                    { (PostfixState.BothOperands, PostfixEvent.Unknown), (PostfixState.End, SetErrorUnknownSymbol) },
            };
            stateMachine = new(stateTransitions, PostfixState.NeitherOperand, OnReset);
        }

        public void Calculate(string postfixString) {
            if (postfixString.Count(f => f == delimiter) < 2)
            {
                Error = "Bad postfix expression: there should be at least 2 delimiters (|)";
                return;
            }
            stateMachine.Reset();

            string[] rawEvents = postfixString.Split(delimiter);
            foreach (string rawEvent in rawEvents)
            {
                if (stateMachine.HasEnded) return;
                lastRawEvent = rawEvent;
                lastEventPosition++;
                ProcessEvent(rawEvent);
            }
            if (!stateMachine.HasEnded) ProcessEvent("=");
        }

        private void OnReset() {
            Result = null;
            Error = null;
            lastEventPosition = -1;
            lastRawEvent = "";
            lastDouble = 0.0;
            left = 0.0;
            right = 0.0;
        }

        private void ProcessEvent(string rawEvent) {
            PostfixEvent evnt = PostfixEvent.Unknown;
            if (knownOperators.ContainsKey(rawEvent))
            {
                currentOperator = knownOperators[rawEvent];
                evnt = PostfixEvent.Operator;
            }
            else if (double.TryParse(rawEvent, out lastDouble))
            {
                evnt = PostfixEvent.Operand;
            }

            stateMachine.Accept(evnt);
        }

        private void SetLeft() {
            left = lastDouble;
        }

        private void SetRight() {
            right = lastDouble;
        }

        private void CalcLeft() {
            left = currentOperator(left, right);
        }

        private void SetResult() {
            Result = left;
        }

        private void SetErrorUnexpectedOperator() {
            Error = $"Bad postfix expression: unexpected operator \"{lastRawEvent}\" in position {lastEventPosition}";
        }

        private void SetErrorUnexpectedOperand() {
            Error = $"Bad postfix expression: unexpected operand \"{lastRawEvent}\" in position {lastEventPosition}";
        }

        private void SetErrorUnknownSymbol() {
            Error = $"Bad postfix expression: unknown symbol \"{lastRawEvent}\" in position {lastEventPosition}";
        }

    }
}
