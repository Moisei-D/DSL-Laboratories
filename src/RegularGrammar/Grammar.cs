using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab1
{
    public class Grammar
    {
        public HashSet<char> Vn { get; set; } = new HashSet<char> { 'S', 'A', 'B', 'C' };
        public HashSet<char> Vt { get; set; } = new HashSet<char> { 'a', 'b', 'c', 'd' };
        public char S { get; set; } = 'S';
        public Dictionary<char, List<string>> P { get; set; } = new Dictionary<char, List<string>>
        {
            { 'S', new List<string> { "dA" } },
            { 'A', new List<string> { "aB", "bA" } },
            { 'B', new List<string> { "bC", "aB", "d" } },
            { 'C', new List<string> { "cB" } }
        };
        
        public string GenerateString()
        {
            Random rand = new Random();
            string current = S.ToString();
            while (current.Any(c => Vn.Contains(c)))
            {
                for (int i = 0; i < current.Length; i++)
                {
                    if (Vn.Contains(current[i]))
                    {
                        var options = P[current[i]];
                        string replacement = options[rand.Next(options.Count)];
                        current = current.Remove(i, 1).Insert(i, replacement);
                        break;
                    }
                }
            }
            return current;
        }

        public FiniteAutomaton ToFiniteAutomaton()
        {
            var fa = new FiniteAutomaton();
            fa.InitialState = S.ToString();
            fa.FinalStates.Add("X"); // Special state for terminal productions like B -> d

            foreach (var rule in P)
            {   
                foreach (var production in rule.Value)
                {
                    char symbol = production[0];
                    string fromState = rule.Key.ToString();

                    if (production.Length > 1)
                        fa.Transitions[fromState + "|" + symbol] = production[1].ToString();
                    else
                        fa.Transitions[fromState + "|" + symbol] = "X";
                }
            }
            return fa;
        }

        public string ClassifyChomskyHierarchy()
        {
            bool hasLeftLinear = false;
            bool hasRightLinear = false;
            bool isRegular = true;
            bool isContextSensitive = true;

            foreach (var rule in P)
            {
                foreach (var production in rule.Value)
                {
                    if (string.IsNullOrEmpty(production))
                    {
                        if (rule.Key != S)
                        {
                            isContextSensitive = false;
                        }
                        continue;
                    }

                    if (production.Length == 1)
                    {
                        if (!Vt.Contains(production[0]))
                        {
                            isRegular = false;
                        }
                    }
                    else if (production.Length == 2)
                    {
                        bool firstIsTerminal = Vt.Contains(production[0]);
                        bool secondIsNonTerminal = Vn.Contains(production[1]);
                        bool firstIsNonTerminal = Vn.Contains(production[0]);
                        bool secondIsTerminal = Vt.Contains(production[1]);

                        if (firstIsTerminal && secondIsNonTerminal)
                        {
                            hasRightLinear = true;
                        }
                        else if (firstIsNonTerminal && secondIsTerminal)
                        {
                            hasLeftLinear = true;
                        }
                        else
                        {
                            isRegular = false;
                        }
                    }
                    else
                    {
                        isRegular = false;
                    }

                    if (production.Length < 1)
                    {
                        isContextSensitive = false;
                    }
                }
            }

            if (isRegular && !(hasLeftLinear && hasRightLinear))
            {
                return "Type-3 (Regular)";
            }

            if (isContextSensitive)
            {
                return "Type-1 (Context-Sensitive)";
            }

            return "Type-2 (Context-Free)";
        }
    }
}
