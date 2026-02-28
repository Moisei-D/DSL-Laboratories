using System;
using System.Collections.Generic;
using System.Linq;

namespace Lab2
{
    public class Grammar
    {
        public HashSet<string> NonTerminals { get; set; } = new HashSet<string>();
        public HashSet<string> Terminals { get; set; } = new HashSet<string>();
        public string StartSymbol { get; set; } = string.Empty;
        public Dictionary<string, List<List<string>>> Productions { get; set; } = new Dictionary<string, List<List<string>>>();

        public void AddProduction(string lhs, params string[] rhsSymbols)
        {
            if (!Productions.TryGetValue(lhs, out var list))
            {
                list = new List<List<string>>();
                Productions[lhs] = list;
            }

            list.Add(new List<string>(rhsSymbols));
        }

        public string ClassifyChomskyHierarchy()
        {
            bool hasLeftLinear = false;
            bool hasRightLinear = false;
            bool isRegular = true;
            bool isContextFree = true;
            bool isContextSensitive = true;
            bool isUnrestricted = true;

            foreach(var production in Productions)
            {
                var lhsSymbols = GetLhsSymbols(production.Key);
                if (lhsSymbols.Count == 0)
                {
                    isUnrestricted = false;
                    isContextSensitive = false;
                    isContextFree = false;
                    isRegular = false;
                    continue;
                }

                bool lhsIsSingleNonTerminal = lhsSymbols.Count == 1 && NonTerminals.Contains(lhsSymbols[0]);
                if (!lhsIsSingleNonTerminal)
                {
                    isContextFree = false;
                    isRegular = false;
                }

                bool lhsHasNonTerminal = lhsSymbols.Any(NonTerminals.Contains);

                foreach (var rhs in production.Value)
                {
                    if (!lhsHasNonTerminal)
                    {
                        isContextSensitive = false;
                    }

                    if (rhs.Count < lhsSymbols.Count && !(rhs.Count == 0 && production.Key == StartSymbol))
                    {
                        isContextSensitive = false;
                    }

                    if (!isRegular)
                    {
                        continue;
                    }
                    if (rhs.Count == 0)
                    {
                        continue;
                    }
                    if (rhs.Count == 1)
                    {
                        if (!Terminals.Contains(rhs[0]))
                        {
                            isRegular = false;
                        }
                    }
                    else if (rhs.Count == 2)
                    {
                        bool firstIsTerminal = Terminals.Contains(rhs[0]);
                        bool secondIsNonTerminal = NonTerminals.Contains(rhs[1]);
                        bool firstIsNonTerminal = NonTerminals.Contains(rhs[0]);
                        bool secondIsTerminal = Terminals.Contains(rhs[1]);

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
                }
            }
            if (isRegular && !(hasLeftLinear && hasRightLinear))
            {
                return "Type-3 (Regular)";
            }

            if (isContextFree)
            {
                return "Type-2 (Context-Free)";
            }

            if (isContextSensitive)
            {
                return "Type-1 (Context-Sensitive)";
            }

            return "Type-0 (Unrestricted)";
        }

        private static IReadOnlyList<string> GetLhsSymbols(string lhs)
        {
            if (string.IsNullOrWhiteSpace(lhs))
            {
                return Array.Empty<string>();
            }
            if (lhs.Contains(' '))
            {
                return lhs.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            }

            return new[] { lhs };
        }
    }
}
