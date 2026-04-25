using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace NicUtils.FiniteStateMachines {
    public class HierarchicalFiniteStateMachine<TState, TEvent> : FiniteStateMachine<TState, TEvent>
        where TState : struct, Enum where TEvent : struct, Enum {

        private readonly Dictionary<TState, TState> parents;
        private readonly Dictionary<TState, TState> initialSubStates;
        private readonly Dictionary<TState, Action> compiledEntryActions = new();
        private readonly Dictionary<TState, Action> compiledExitActions = new();
        
        private readonly Dictionary<TState, Expression<Action>> entryActionExpressions;
        private readonly Dictionary<TState, Expression<Action>> exitActionExpressions;

        public HierarchicalFiniteStateMachine(
            Dictionary<(TState currentState, TEvent evnt), (TState newState, Expression<Action> action)> transitions,
            TState initialState,
            Expression<Action> resetAction,
            Dictionary<TState, TState> parents = null,
            Dictionary<TState, TState> initialSubStates = null,
            Dictionary<TState, Expression<Action>> entryActions = null,
            Dictionary<TState, Expression<Action>> exitActions = null,
            HashSet<TState> allStates = null)
            : base(transitions, initialState, resetAction, allStates) {

            this.parents = parents ?? new Dictionary<TState, TState>();
            this.initialSubStates = initialSubStates ?? new Dictionary<TState, TState>();
            this.entryActionExpressions = entryActions ?? new Dictionary<TState, Expression<Action>>();
            this.exitActionExpressions = exitActions ?? new Dictionary<TState, Expression<Action>>();

            foreach (var kvp in entryActionExpressions) {
                compiledEntryActions[kvp.Key] = kvp.Value.Compile();
            }

            foreach (var kvp in exitActionExpressions) {
                compiledExitActions[kvp.Key] = kvp.Value.Compile();
            }

            // Perform initial deep entry if necessary
            EnterState(initialState);
        }

        protected override void Validate() {
            // HSM validation might be more relaxed than FSM.
            // For now, we allow the base validation but it might need to be overridden 
            // if we want to allow states that only have transitions via their parents.
            // base.Validate(); 
        }

        public override void Accept(TEvent evnt) {
            if (HasEnded) {
                throw new IOException($"Received an event while at End state: {evnt}");
            }

            TState? sourceState = CurrentState;
            (TState newState, Action action)? transition = null;

            // Bubbling: Look for transition in current state, then its parents
            while (sourceState.HasValue) {
                if (transitions.TryGetValue((sourceState.Value, evnt), out var t)) {
                    transition = (t.newState, compiledActions[(sourceState.Value, evnt)]);
                    break;
                }
                sourceState = GetParent(sourceState.Value);
            }

            if (transition == null) {
                throw new IOException($"Received illegal event (\"{evnt}\") for the present state (\"{CurrentState}\") or its ancestors.");
            }

            ExecuteTransition(sourceState.Value, transition.Value.newState, transition.Value.action);
        }

        private void ExecuteTransition(TState source, TState target, Action transitionAction) {
            TState? lca = FindLCA(CurrentState, target);

            // 1. Exit from CurrentState up to (but not including) LCA
            TState? s = CurrentState;
            while (s.HasValue && !s.Value.Equals(lca)) {
                if (compiledExitActions.TryGetValue(s.Value, out var exitAction)) {
                    exitAction.Invoke();
                }
                s = GetParent(s.Value);
            }

            // 2. Execute transition action
            transitionAction.Invoke();

            // 3. Enter from LCA's child down to target, then deep entry
            EnterState(target, lca);
        }

        private void EnterState(TState target, TState? lca = null) {
            List<TState> path = GetPathToRoot(target);
            // Reverse to get path from root to target
            path.Reverse();

            bool startEntering = !lca.HasValue;
            foreach (var state in path) {
                if (!startEntering) {
                    if (state.Equals(lca.Value)) {
                        startEntering = true;
                    }
                    continue;
                }

                // If we are entering a child of LCA (or the root if lca is null)
                if (compiledEntryActions.TryGetValue(state, out var entryAction)) {
                    entryAction.Invoke();
                }
            }

            CurrentState = target;

            // Deep Entry: If target has an initial sub-state, enter it recursively
            if (initialSubStates.TryGetValue(target, out var subState)) {
                EnterState(subState, target);
            }
        }

        private TState? FindLCA(TState s1, TState s2) {
            var path1 = GetPathToRoot(s1);
            var path2 = GetPathToRoot(s2);

            TState? lastCommon = null;
            int i = path1.Count - 1;
            int j = path2.Count - 1;

            while (i >= 0 && j >= 0 && path1[i].Equals(path2[j])) {
                lastCommon = path1[i];
                i--;
                j--;
            }

            return lastCommon;
        }

        private List<TState> GetPathToRoot(TState state) {
            var path = new List<TState>();
            TState? current = state;
            while (current.HasValue) {
                path.Add(current.Value);
                current = GetParent(current.Value);
            }
            return path;
        }

        private TState? GetParent(TState state) {
            if (parents.TryGetValue(state, out var parent)) {
                return parent;
            }
            return null;
        }

        public override string ToMermaidDiagram() {
            StringBuilder diagram = new StringBuilder();
            diagram.AppendLine("stateDiagram-v2");

            // Group transitions by LCA parent
            var transitionsByLCA = transitions
                .GroupBy(t => FindLCA(t.Key.currentState, t.Value.newState))
                .ToList();

            // Initial transition
            diagram.AppendLine($"    [*] --> {InitialState}");

            // Group states by parent
            var rootChildren = AllStates.Where(s => GetParent(s) == null).ToList();
            var childrenByParent = AllStates.Where(s => GetParent(s) != null)
                .GroupBy(s => GetParent(s).Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Process states and transitions starting from root
            AppendStateDefinitions(diagram, rootChildren, childrenByParent, transitionsByLCA, null, 1);

            return diagram.ToString().TrimEnd();
        }

        private void AppendStateDefinitions(
            StringBuilder diagram, 
            List<TState> children, 
            Dictionary<TState, List<TState>> childrenByParent, 
            List<IGrouping<TState?, KeyValuePair<(TState currentState, TEvent evnt), (TState newState, Expression<Action> action)>>> transitionsByLCA,
            TState? currentParent,
            int indentLevel) {
            
            string indent = new string(' ', indentLevel * 4);

            // Add states that are parents
            foreach (var child in children) {
                if (childrenByParent.ContainsKey(child)) {
                    diagram.AppendLine($"{indent}state {child} {{");
                    
                    if (initialSubStates.TryGetValue(child, out var initial)) {
                        diagram.AppendLine($"{indent}    [*] --> {initial}");
                    }

                    AppendStateDefinitions(diagram, childrenByParent[child], childrenByParent, transitionsByLCA, child, indentLevel + 1);
                    diagram.AppendLine($"{indent}}}");
                }
            }

            // Add transitions belonging to this LCA
            var localTransitions = transitionsByLCA.FirstOrDefault(g => EqualityComparer<TState?>.Default.Equals(g.Key, currentParent));
            if (localTransitions != null) {
                foreach (var item in localTransitions) {
                    string actionStr = item.Value.action.ToString();
                    // Strip "() => " and "value(...)."
                    actionStr = Regex.Replace(actionStr, @"^.*?\s*=>\s*", "");
                    actionStr = Regex.Replace(actionStr, @"value\(.*?\)\.", "");
                    
                    diagram.AppendLine($"{indent}{item.Key.currentState} --> {item.Value.newState}: {item.Key.evnt}|{actionStr}");
                }
            }
        }

        public string ToActionsCsv() {
            StringBuilder csv = new StringBuilder();
            csv.AppendLine("State,Entry Action,Exit Action");
            foreach (var state in AllStates.OrderBy(s => s.ToString())) {
                entryActionExpressions.TryGetValue(state, out var entry);
                exitActionExpressions.TryGetValue(state, out var exit);

                string entryStr = entry != null ? entry.ToString() : "";
                string exitStr = exit != null ? exit.ToString() : "";

                // Strip "() => " and "value(...)."
                entryStr = Regex.Replace(entryStr, @"^.*?\s*=>\s*", "");
                entryStr = Regex.Replace(entryStr, @"value\(.*?\)\.", "");
                exitStr = Regex.Replace(exitStr, @"^.*?\s*=>\s*", "");
                exitStr = Regex.Replace(exitStr, @"value\(.*?\)\.", "");

                csv.AppendLine($"{EscapeCsvField(state.ToString())},{EscapeCsvField(entryStr)},{EscapeCsvField(exitStr)}");
            }
            return csv.ToString().TrimEnd();
        }

        private string EscapeCsvField(string field) {
            if (string.IsNullOrEmpty(field)) return "";
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r")) {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }
            return field;
        }

        private string EscapeQuotes(string s) => s.Replace("\"", "\\\"");

        public static HierarchicalFiniteStateMachine<TState, TEvent> FromMermaidDiagram(string diagramCode, string actionsCsv, IEnumerable<Type> typeContext = null, IEnumerable<object> instanceContext = null) {
            var entryActions = new Dictionary<TState, Expression<Action>>();
            var exitActions = new Dictionary<TState, Expression<Action>>();

            if (!string.IsNullOrEmpty(actionsCsv)) {
                var rows = ParseCsv(actionsCsv);
                foreach (var row in rows.Skip(1)) { // Skip header
                    if (row.Count < 3) continue;
                    TState state = ParseEnumOrThrow<TState>(row[0]);
                    if (!string.IsNullOrEmpty(row[1])) {
                        entryActions[state] = ParseActionExpression(row[1], typeContext, instanceContext);
                    }
                    if (!string.IsNullOrEmpty(row[2])) {
                        exitActions[state] = ParseActionExpression(row[2], typeContext, instanceContext);
                    }
                }
            }

            return FromMermaidDiagramInternal(diagramCode, typeContext, instanceContext, entryActions, exitActions);
        }

        public static new HierarchicalFiniteStateMachine<TState, TEvent> FromMermaidDiagram(string diagramCode, IEnumerable<Type> typeContext = null, IEnumerable<object> instanceContext = null) {
            return FromMermaidDiagramInternal(diagramCode, typeContext, instanceContext, new(), new());
        }

        private static HierarchicalFiniteStateMachine<TState, TEvent> FromMermaidDiagramInternal(
            string diagramCode, 
            IEnumerable<Type> typeContext, 
            IEnumerable<object> instanceContext,
            Dictionary<TState, Expression<Action>> entryActions,
            Dictionary<TState, Expression<Action>> exitActions) {
            
            var transitions = new Dictionary<(TState currentState, TEvent evnt), (TState newState, Expression<Action> action)>();
            var parents = new Dictionary<TState, TState>();
            var initialSubStates = new Dictionary<TState, TState>();
            TState? globalInitialState = null;

            string[] lines = diagramCode.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            Stack<TState> stateStack = new Stack<TState>();

            const string quotedOrUnquoted = @"(""(?:[^""\\]|\\.)*""|[\w\[\]\*]+)";

            bool foundHeader = false;
            foreach (var line in lines) {
                string trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine)) continue;

                if (!foundHeader) {
                    if (trimmedLine != "stateDiagram-v2") continue;
                    foundHeader = true;
                    continue;
                }

                // Handle state block start: state "Decorated" {
                var stateStartMatch = Regex.Match(trimmedLine, $@"^state\s+{quotedOrUnquoted}\s*\{{$");
                if (stateStartMatch.Success) {
                    TState state = ParseDecoratedName(UnescapeQuotes(stateStartMatch.Groups[1].Value.Trim('"')), typeContext, instanceContext, out var entry, out var exit);
                    if (stateStack.Count > 0) parents[state] = stateStack.Peek();
                    if (entry != null) entryActions[state] = entry;
                    if (exit != null) exitActions[state] = exit;
                    stateStack.Push(state);
                    continue;
                }

                // Handle state block end: }
                if (trimmedLine == "}") {
                    if (stateStack.Count > 0) stateStack.Pop();
                    continue;
                }

                // Handle standalone state definition: state "Decorated"
                var stateDefMatch = Regex.Match(trimmedLine, $@"^state\s+{quotedOrUnquoted}$");
                if (stateDefMatch.Success) {
                    TState state = ParseDecoratedName(UnescapeQuotes(stateDefMatch.Groups[1].Value.Trim('"')), typeContext, instanceContext, out var entry, out var exit);
                    if (stateStack.Count > 0 && !parents.ContainsKey(state)) parents[state] = stateStack.Peek();
                    if (entry != null) entryActions[state] = entry;
                    if (exit != null) exitActions[state] = exit;
                    continue;
                }

                // Handle initial state and transitions: source --> target [: label]
                // Supports quoted names and unquoted ones
                var transitionMatch = Regex.Match(trimmedLine, $@"^{quotedOrUnquoted}\s*-->\s*{quotedOrUnquoted}(?:\s*:\s*(.*))?$");
                if (transitionMatch.Success) {
                    string sourceStr = UnescapeQuotes(transitionMatch.Groups[1].Value.Trim('"'));
                    string targetStr = UnescapeQuotes(transitionMatch.Groups[2].Value.Trim('"'));
                    string label = UnescapeQuotes(transitionMatch.Groups[3].Value);

                    if (sourceStr == "[*]") {
                        TState initial = ParseDecoratedName(targetStr, typeContext, instanceContext, out var entry, out var exit);
                        if (stateStack.Count > 0) {
                            initialSubStates[stateStack.Peek()] = initial;
                        } else {
                            globalInitialState = initial;
                        }
                        if (entry != null) entryActions[initial] = entry;
                        if (exit != null) exitActions[initial] = exit;
                        continue;
                    }

                    if (targetStr == "[*]") targetStr = "end";

                    TState source = ParseDecoratedName(sourceStr, typeContext, instanceContext, out var sEntry, out var sExit);
                    TState target = ParseDecoratedName(targetStr, typeContext, instanceContext, out var tEntry, out var tExit);

                    if (stateStack.Count > 0) {
                        if (!source.Equals(stateStack.Peek()) && !parents.ContainsKey(source)) parents[source] = stateStack.Peek();
                        if (!target.Equals(stateStack.Peek()) && !IsEndState(target) && !parents.ContainsKey(target)) parents[target] = stateStack.Peek();
                    }

                    if (sEntry != null) entryActions[source] = sEntry;
                    if (sExit != null) exitActions[source] = sExit;
                    if (tEntry != null) entryActions[target] = tEntry;
                    if (tExit != null) exitActions[target] = tExit;

                    if (string.IsNullOrEmpty(label)) continue;

                    string evntStr;
                    Expression<Action> action = () => NoOp();

                    if (label.Contains('|')) {
                        string[] parts = label.Split('|');
                        evntStr = parts[0];
                        action = ParseActionExpression(parts[1], typeContext, instanceContext);
                    } else {
                        evntStr = label;
                    }

                    TEvent evnt = ParseEnumOrThrow<TEvent>(evntStr);
                    transitions[(source, evnt)] = (target, action);
                }
            }

            if (!globalInitialState.HasValue) throw new ArgumentException("Initial state not found");

            return new HierarchicalFiniteStateMachine<TState, TEvent>(
                transitions,
                globalInitialState.Value,
                () => NoOp(),
                parents,
                initialSubStates,
                entryActions,
                exitActions
            );

        }

        private static List<List<string>> ParseCsv(string csv) {
            var results = new List<List<string>>();
            using (var reader = new StringReader(csv)) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    var fields = new List<string>();
                    bool inQuotes = false;
                    StringBuilder currentField = new StringBuilder();
                    for (int i = 0; i < line.Length; i++) {
                        char c = line[i];
                        if (c == '\"') {
                            if (inQuotes && i + 1 < line.Length && line[i + 1] == '\"') {
                                currentField.Append('\"');
                                i++;
                            } else {
                                inQuotes = !inQuotes;
                            }
                        } else if (c == ',' && !inQuotes) {
                            fields.Add(currentField.ToString());
                            currentField.Clear();
                        } else {
                            currentField.Append(c);
                        }
                    }
                    fields.Add(currentField.ToString());
                    results.Add(fields);
                }
            }
            return results;
        }

        private static string UnescapeQuotes(string s) => s.Replace("\\\"", "\"");

        private static TState ParseDecoratedName(string decorated, IEnumerable<Type> typeContext, IEnumerable<object> instanceContext, out Expression<Action> entry, out Expression<Action> exit) {
            entry = null;
            exit = null;

            // Match Name(entry/E/exit/X) - use non-greedy matching for the entry action to avoid capturing /exit/
            var match = Regex.Match(decorated, @"^(\w+)\(entry/(.*?)/exit/(.*)\)$");
            if (match.Success) {
                string name = match.Groups[1].Value;
                string entryStr = match.Groups[2].Value;
                string exitStr = match.Groups[3].Value;

                if (!string.IsNullOrEmpty(entryStr)) {
                    entry = ParseActionExpression(entryStr, typeContext, instanceContext);
                }
                if (!string.IsNullOrEmpty(exitStr)) {
                    exit = ParseActionExpression(exitStr, typeContext, instanceContext);
                }
                return ParseEnumOrThrow<TState>(name);
            }

            return ParseEnumOrThrow<TState>(decorated);
        }
    }
}
