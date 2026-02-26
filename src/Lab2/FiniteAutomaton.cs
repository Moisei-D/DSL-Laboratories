using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab2
{
    public class FiniteAutomaton
    {
        public const string Epsilon = "ε";

        public HashSet<string> States { get; set; } = new HashSet<string>();
        public HashSet<string> Alphabet { get; set; } = new HashSet<string>();
        public string InitialState { get; set; } = string.Empty;
        public HashSet<string> FinalStates { get; set; } = new HashSet<string>();
        public Dictionary<(string State, string Symbol), HashSet<string>> Transitions { get; set; }
            = new Dictionary<(string State, string Symbol), HashSet<string>>();

        public void AddTransition(string from, string symbol, string to)
        {
            var key = (from, symbol);
            if (!Transitions.TryGetValue(key, out var targets))
            {
                targets = new HashSet<string>();
                Transitions[key] = targets;
            }

            targets.Add(to);
            States.Add(from);
            States.Add(to);

            if (symbol != Epsilon)
            {
                Alphabet.Add(symbol);
            }
        }

        public bool IsDeterministic()
        {
            foreach (var transition in Transitions)
            {
                if (transition.Key.Symbol == Epsilon)
                {
                    return false;
                }

                if (transition.Value.Count > 1)
                {
                    return false;
                }
            }

            return true;
        }

        public Grammar ToRegularGrammar()
        {
            var grammar = new Grammar
            {
                NonTerminals = new HashSet<string>(States),
                Terminals = new HashSet<string>(Alphabet),
                StartSymbol = InitialState
            };

            foreach (var transition in Transitions)
            {
                string from = transition.Key.State;
                string symbol = transition.Key.Symbol;

                foreach (var to in transition.Value)
                {
                    grammar.AddProduction(from, symbol, to);

                    if (FinalStates.Contains(to))
                    {
                        grammar.AddProduction(from, symbol);
                    }
                }
            }

            if (FinalStates.Contains(InitialState))
            {
                grammar.AddProduction(InitialState);
            }

            return grammar;
        }

        public FiniteAutomaton ToDfa()
        {
            var dfa = new FiniteAutomaton
            {
                Alphabet = new HashSet<string>(Alphabet.Where(symbol => symbol != Epsilon))
            };

            var startSet = EpsilonClosure(new[] { InitialState });
            var queue = new Queue<HashSet<string>>();
            var seen = new HashSet<string>();

            string startName = GetStateName(startSet);
            dfa.InitialState = startName;
            dfa.States.Add(startName);
            seen.Add(startName);
            queue.Enqueue(startSet);

            if (startSet.Any(state => FinalStates.Contains(state)))
            {
                dfa.FinalStates.Add(startName);
            }

            while (queue.Count > 0)
            {
                var currentSet = queue.Dequeue();
                string currentName = GetStateName(currentSet);

                foreach (var symbol in dfa.Alphabet)
                {
                    var move = Move(currentSet, symbol);
                    var closure = EpsilonClosure(move);
                    string nextName = GetStateName(closure);

                    if (!seen.Contains(nextName))
                    {
                        seen.Add(nextName);
                        dfa.States.Add(nextName);
                        queue.Enqueue(closure);

                        if (closure.Any(state => FinalStates.Contains(state)))
                        {
                            dfa.FinalStates.Add(nextName);
                        }
                    }

                    dfa.AddTransition(currentName, symbol, nextName);
                }
            }

            return dfa;
        }

        private HashSet<string> Move(HashSet<string> states, string symbol)
        {
            var result = new HashSet<string>();

            foreach (var state in states)
            {
                if (Transitions.TryGetValue((state, symbol), out var targets))
                {
                    foreach (var target in targets)
                    {
                        result.Add(target);
                    }
                }
            }

            return result;
        }

        private HashSet<string> EpsilonClosure(IEnumerable<string> states)
        {
            var closure = new HashSet<string>(states);
            var stack = new Stack<string>(states);

            while (stack.Count > 0)
            {
                var state = stack.Pop();

                if (Transitions.TryGetValue((state, Epsilon), out var targets))
                {
                    foreach (var target in targets)
                    {
                        if (closure.Add(target))
                        {
                            stack.Push(target);
                        }
                    }
                }
            }

            return closure;
        }

        private static string GetStateName(HashSet<string> states)
        {
            if (states.Count == 0)
            {
                return "∅";
            }

            var ordered = states.OrderBy(state => state, StringComparer.Ordinal).ToArray();
            return ordered.Length == 1 ? ordered[0] : "{" + string.Join(",", ordered) + "}";
        }

    }
}
