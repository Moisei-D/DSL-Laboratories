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

            foreach (var production in Productions)
            {
                foreach (var rhs in production.Value)
                {
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

            return "Type-2 (Context-Free)";
        }
    }
}
