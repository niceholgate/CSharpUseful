using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NicUtils.FiniteStateMachines {
    // Events cannot be null, doesn't mean anything?
    // State null means Done?
    // Must define an Error state?
    public class FiniteStateMachine<TState, TEvent> : AbstractFiniteStateMachine<TState, TEvent, Expression<Action>>
        where TState : struct, Enum where TEvent : struct, Enum {

        private readonly Action resetAction;
        private readonly Dictionary<(TState currentState, TEvent evnt), Action> compiledActions = new();

        public FiniteStateMachine(Dictionary<(TState currentState, TEvent evnt), (TState newState, Expression<Action> action)> transitions,
                                  TState initialState,
                                  Expression<Action> resetAction) : base(transitions, initialState) {
            this.resetAction = resetAction.Compile();
            foreach (var transition in transitions) {
                compiledActions[transition.Key] = transition.Value.action.Compile();
            }
        }

        public void Reset() {
            CurrentState = InitialState;
            resetAction.Invoke();
        }

        public override void Accept(TEvent evnt)
        {
            if (HasEnded) {
                throw new IOException($"Received an event while at End state: {evnt}");
            }

            if (!transitions.ContainsKey((CurrentState, evnt))) {
                throw new IOException($"Received illegal event (\"{evnt}\") for the present state (\"{CurrentState}\")");
            }

            compiledActions[(CurrentState, evnt)].Invoke();
            CurrentState = transitions[(CurrentState, evnt)].newState;
        }

        public string ToMermaidDiagram() {
            StringBuilder diagram = new StringBuilder();

            diagram.Append("stateDiagram-v2");
            diagram.Append('\n');
            diagram.Append($"    [*] --> {InitialState}");
            diagram.Append('\n');

            foreach (var item in transitions) {
                string actionStr = item.Value.action.ToString();
                // Strip the "value(...)." part for instance methods to make it look like static methods
                actionStr = System.Text.RegularExpressions.Regex.Replace(actionStr, @"value\(.*?\)\.", "");
                
                diagram.Append($"    {item.Key.currentState} --> {item.Value.newState}: {item.Key.evnt}|{actionStr}");
                diagram.Append('\n');
            }
            diagram.Remove(diagram.Length - 1, 1);

            return diagram.ToString();
        }

        public static FiniteStateMachine<TState, TEvent> FromMermaidDiagram(string diagramCode, IEnumerable<Type> typeContext = null, IEnumerable<object> instanceContext = null) {
            var transitions = new Dictionary<(TState currentState, TEvent evnt), (TState newState, Expression<Action> action)>();
            string initialState = null;
            Expression<Action> noop = () => NoOp();

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

                var transitionMatch = System.Text.RegularExpressions.Regex.Match(trimmedLine, @"^([\w\[\]\*]+)\s*-->\s*([\w\[\]\*]+)(?:\s*:\s*(.*))?$");
                if (transitionMatch.Success) {
                    string currentState = transitionMatch.Groups[1].Value;
                    string newState = transitionMatch.Groups[2].Value;
                    string label = transitionMatch.Groups[3].Value;

                    if (currentState == "[*]") {
                        initialState = newState;
                        continue;
                    }

                    if (newState == "[*]") {
                        newState = "end";
                    }

                    if (string.IsNullOrEmpty(label)) {
                        throw new ArgumentException("Must specify an event that causes this state transition.");
                    }

                    string evnt;
                    Expression<Action> action = noop;

                    if (label.Contains('|')) {
                        string[] parts = label.Split('|');
                        evnt = parts[0];
                        action = ParseActionExpression(parts[1], typeContext, instanceContext);
                        if (action == null) {
                            throw new ArgumentException($"Could not parse action expression: {parts[1]}. Ensure appropriate typeContext or instanceContext is provided.");
                        }
                    } else {
                        evnt = label;
                    }

                    TState currentStateEnum = ParseEnumOrThrow<TState>(currentState);
                    TState newStateEnum = ParseEnumOrThrow<TState>(newState);
                    TEvent evntEnum = ParseEnumOrThrow<TEvent>(evnt);
                    transitions[(currentStateEnum, evntEnum)] = (newStateEnum, action);

                    if (initialState == null && currentState != "[*]") initialState = currentState;
                }
            }

            if (initialState == null) {
                throw new ArgumentException("Could not determine initial state from Mermaid diagram.");
            }

            TState initialStateEnum = ParseEnumOrThrow<TState>(initialState);
            return new FiniteStateMachine<TState, TEvent>(transitions, initialStateEnum, () => NoOp());
        }
        
        private static T ParseEnumOrThrow<T>(string str) where T : struct, Enum
        {
            if (Enum.TryParse(str, out T result))
            {
                return result;
            }
            throw new ArgumentException($"Could not parse string '{str}' to enum of type {typeof(T).Name}.");
        }

        private static Expression<Action> ParseActionExpression(string expressionStr, IEnumerable<Type> typeContext, IEnumerable<object> instanceContext) {
            // Basic parser for "() => MethodName(args)" or "() => value(Type).MethodName(args)"
            // The value(Type). part is now optional
            var match = System.Text.RegularExpressions.Regex.Match(expressionStr, @"^\(\) => (?:value\(([^)]+)\)\.)?(\w+)\((.*)\)$");
            if (!match.Success) return null;

            string typeName = match.Groups[1].Value;
            string methodName = match.Groups[2].Value;
            string argsStr = match.Groups[3].Value;

            string[] args = string.IsNullOrWhiteSpace(argsStr)
                ? Array.Empty<string>()
                : System.Text.RegularExpressions.Regex.Split(argsStr, @",\s*(?=(?:[^""]*""[^""]*"")*[^""]*$)")
                    .Select(s => s.Trim().Trim('"')).ToArray();

            if (string.IsNullOrEmpty(typeName)) {
                // Try static method call or method in typeContext
                if (typeContext != null) {
                    foreach (var type in typeContext) {
                        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                        if (method != null) {
                            var paramInfos = method.GetParameters();
                            if (paramInfos.Length == args.Length) {
                                var argExpressions = args.Zip(paramInfos, (arg, param) => {
                                    object convertedArg;
                                    if (param.ParameterType.IsEnum) {
                                        convertedArg = Enum.Parse(param.ParameterType, arg);
                                    } else {
                                        convertedArg = Convert.ChangeType(arg, param.ParameterType);
                                    }
                                    return Expression.Constant(convertedArg);
                                }).ToArray();
                                return Expression.Lambda<Action>(Expression.Call(null, method, argExpressions));
                            }
                        }
                    }
                }
                
                // Also try instance method call if typeName is missing but instanceContext is provided
                if (instanceContext != null) {
                    foreach (var instance in instanceContext) {
                        var method = instance.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (method != null) {
                            var paramInfos = method.GetParameters();
                            if (paramInfos.Length == args.Length) {
                                var argExpressions = args.Zip(paramInfos, (arg, param) => {
                                    object convertedArg;
                                    if (param.ParameterType.IsEnum) {
                                        convertedArg = Enum.Parse(param.ParameterType, arg);
                                    } else {
                                        convertedArg = Convert.ChangeType(arg, param.ParameterType);
                                    }
                                    return Expression.Constant(convertedArg);
                                }).ToArray();
                                return Expression.Lambda<Action>(Expression.Call(Expression.Constant(instance), method, argExpressions));
                            }
                        }
                    }
                }
            } else {
                // Instance method call with explicit type name
                if (instanceContext != null) {
                    foreach (var instance in instanceContext) {
                        if (instance.GetType().FullName == typeName || instance.GetType().Name == typeName) {
                            var method = instance.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (method != null) {
                                var paramInfos = method.GetParameters();
                                if (paramInfos.Length == args.Length) {
                                    var argExpressions = args.Zip(paramInfos, (arg, param) => {
                                        object convertedArg;
                                        if (param.ParameterType.IsEnum) {
                                            convertedArg = Enum.Parse(param.ParameterType, arg);
                                        } else {
                                            convertedArg = Convert.ChangeType(arg, param.ParameterType);
                                        }
                                        return Expression.Constant(convertedArg);
                                    }).ToArray();
                                    return Expression.Lambda<Action>(Expression.Call(Expression.Constant(instance), method, argExpressions));
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
