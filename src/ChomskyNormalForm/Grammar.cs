using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CNF
{
    /// <summary>
    /// Represents a Context-Free Grammar and provides methods to
    /// convert it step-by-step into Chomsky Normal Form (CNF).
    /// </summary>
    public class Grammar
    {
        public HashSet<string> NonTerminals { get; private set; }
        public HashSet<string> Terminals { get; private set; }
        public List<(string Lhs, List<string> Rhs)> Productions { get; private set; }
        public string Start { get; private set; }

        // Counter for freshly generated non-terminal names
        private int _freshCounter = 0;

        public Grammar(
            IEnumerable<string> nonTerminals,
            IEnumerable<string> terminals,
            IEnumerable<(string, List<string>)> productions,
            string start)
        {
            NonTerminals = new HashSet<string>(nonTerminals);
            Terminals = new HashSet<string>(terminals);
            Productions = productions.Select(p => (p.Item1, new List<string>(p.Item2))).ToList();
            Start = start;
        }

        // ─────────────────────────────────────────────
        //  Pretty printing
        // ─────────────────────────────────────────────

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"  Non-terminals : {string.Join(", ", NonTerminals.OrderBy(x => x))}");
            sb.AppendLine($"  Terminals     : {string.Join(", ", Terminals.OrderBy(x => x))}");
            sb.AppendLine($"  Start         : {Start}");
            sb.AppendLine("  Productions   :");
            foreach (var (lhs, rhs) in Productions)
                sb.AppendLine($"    {lhs} -> {(rhs.Count == 0 ? "ε" : string.Join("", rhs))}");
            return sb.ToString();
        }

        // ─────────────────────────────────────────────
        //  Step 1 – Eliminate ε-productions
        // ─────────────────────────────────────────────

        public Grammar EliminateEpsilon()
        {
            // 1a. Find all nullable non-terminals (those that can derive ε)
            var nullable = new HashSet<string>();

            // Direct nullable
            foreach (var (lhs, rhs) in Productions)
                if (rhs.Count == 0) nullable.Add(lhs);

            // Closure: if all symbols on the RHS are nullable, the LHS is nullable
            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (var (lhs, rhs) in Productions)
                    if (!nullable.Contains(lhs) && rhs.All(s => nullable.Contains(s)))
                    {
                        nullable.Add(lhs);
                        changed = true;
                    }
            }

            Console.WriteLine($"  Nullable non-terminals: {{ {string.Join(", ", nullable)} }}");

            // 1b. For every production, generate all subsets obtained by omitting
            //     one or more occurrences of nullable symbols.
            var newProds = new List<(string, List<string>)>();
            foreach (var (lhs, rhs) in Productions)
            {
                if (rhs.Count == 0) continue;   // drop the original ε-production

                // Indices of nullable positions
                var nullableIdx = rhs
                    .Select((sym, i) => nullable.Contains(sym) ? i : -1)
                    .Where(i => i >= 0)
                    .ToList();

                // Enumerate all 2^|nullableIdx| subsets of omission
                int subsets = 1 << nullableIdx.Count;
                for (int mask = 0; mask < subsets; mask++)
                {
                    var newRhs = new List<string>();
                    for (int i = 0; i < rhs.Count; i++)
                    {
                        int pos = nullableIdx.IndexOf(i);
                        bool omit = pos >= 0 && (mask & (1 << pos)) != 0;
                        if (!omit) newRhs.Add(rhs[i]);
                    }
                    if (newRhs.Count > 0)   // never add empty unless it's the start
                        AddIfAbsent(newProds, lhs, newRhs);
                }
            }

            // If start symbol is nullable, add S → ε as a special case
            // (only if needed for the language; in this variant S is not nullable)
            if (nullable.Contains(Start))
                AddIfAbsent(newProds, Start, new List<string>());

            return new Grammar(NonTerminals, Terminals, newProds, Start);
        }

        // ─────────────────────────────────────────────
        //  Step 2 – Eliminate unit (renaming) productions  A → B
        // ─────────────────────────────────────────────

        public Grammar EliminateRenaming()
        {
            // Build unit closure for each non-terminal
            var newProds = new List<(string, List<string>)>();

            foreach (var A in NonTerminals)
            {
                // BFS / closure of what A can reach via unit productions
                var reachable = new HashSet<string> { A };
                var queue = new Queue<string>();
                queue.Enqueue(A);

                while (queue.Count > 0)
                {
                    var cur = queue.Dequeue();
                    foreach (var (lhs, rhs) in Productions)
                        if (lhs == cur && rhs.Count == 1 && NonTerminals.Contains(rhs[0]))
                            if (reachable.Add(rhs[0]))
                                queue.Enqueue(rhs[0]);
                }

                // Add all non-unit productions reachable from A
                foreach (var B in reachable)
                    foreach (var (lhs, rhs) in Productions)
                        if (lhs == B && !(rhs.Count == 1 && NonTerminals.Contains(rhs[0])))
                            AddIfAbsent(newProds, A, rhs);
            }

            return new Grammar(NonTerminals, Terminals, newProds, Start);
        }

        // ─────────────────────────────────────────────
        //  Step 3 – Eliminate inaccessible symbols
        // ─────────────────────────────────────────────

        public Grammar EliminateInaccessible()
        {
            var accessible = new HashSet<string> { Start };
            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (var (lhs, rhs) in Productions)
                    if (accessible.Contains(lhs))
                        foreach (var sym in rhs)
                            if (NonTerminals.Contains(sym) && accessible.Add(sym))
                                changed = true;
            }

            Console.WriteLine($"  Accessible non-terminals: {{ {string.Join(", ", accessible)} }}");

            var newNT = new HashSet<string>(NonTerminals.Where(n => accessible.Contains(n)));
            var newProds = Productions
                .Where(p => accessible.Contains(p.Lhs))
                .Select(p => (p.Lhs, new List<string>(p.Rhs)))
                .ToList();

            return new Grammar(newNT, Terminals, newProds, Start);
        }

        // ─────────────────────────────────────────────
        //  Step 4 – Eliminate non-productive symbols
        // ─────────────────────────────────────────────

        public Grammar EliminateNonProductive()
        {
            // Productive: a NT is productive if it can derive some string of terminals
            var productive = new HashSet<string>(Terminals);

            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (var (lhs, rhs) in Productions)
                    if (!productive.Contains(lhs) && rhs.All(s => productive.Contains(s)))
                    {
                        productive.Add(lhs);
                        changed = true;
                    }
            }

            Console.WriteLine($"  Productive symbols: {{ {string.Join(", ", productive.Where(NonTerminals.Contains))} }}");

            var newNT = new HashSet<string>(NonTerminals.Where(productive.Contains));
            var newProds = Productions
                .Where(p => productive.Contains(p.Lhs) && p.Rhs.All(productive.Contains))
                .Select(p => (p.Lhs, new List<string>(p.Rhs)))
                .ToList();

            return new Grammar(newNT, Terminals, newProds, Start);
        }

        // ─────────────────────────────────────────────
        //  Step 5 – Convert to Chomsky Normal Form
        //           Every production must be A → BC  or  A → a
        // ─────────────────────────────────────────────

        public Grammar ToCNF()
        {
            var prods = Productions
                .Select(p => (p.Lhs, new List<string>(p.Rhs)))
                .ToList();

            // 5a. Replace terminals in long productions with fresh NTs
            //     e.g. a in  A → aBC  becomes  T_a → a,  A → T_a BC
            var terminalNT = new Dictionary<string, string>(); // terminal → fresh NT
            var freshNTs = new HashSet<string>(NonTerminals);

            var step5a = new List<(string, List<string>)>();
            foreach (var (lhs, rhs) in prods)
            {
                if (rhs.Count == 1)
                {
                    // A → a  or  A → B — keep as-is (unit productions already removed)
                    step5a.Add((lhs, rhs));
                    continue;
                }

                // Replace each terminal symbol with its representative NT
                var newRhs = rhs.Select(sym =>
                {
                    if (!Terminals.Contains(sym)) return sym;
                    if (!terminalNT.TryGetValue(sym, out var nt))
                    {
                        nt = $"T_{sym}";
                        while (freshNTs.Contains(nt)) nt += "_";
                        terminalNT[sym] = nt;
                        freshNTs.Add(nt);
                    }
                    return nt;
                }).ToList();

                step5a.Add((lhs, newRhs));
            }

            // Add  T_a → a  productions
            foreach (var (term, nt) in terminalNT)
                step5a.Add((nt, new List<string> { term }));

            // 5b. Break productions longer than 2 into binary chains
            //     A → B C D  becomes  A → B X1,  X1 → C D
            var final = new List<(string, List<string>)>();
            foreach (var (lhs, rhs) in step5a)
            {
                if (rhs.Count <= 2)
                {
                    final.Add((lhs, rhs));
                    continue;
                }

                string cur = lhs;
                var rem = new List<string>(rhs);
                while (rem.Count > 2)
                {
                    string fresh = $"X{++_freshCounter}";
                    while (freshNTs.Contains(fresh)) fresh = $"X{++_freshCounter}";
                    freshNTs.Add(fresh);

                    final.Add((cur, new List<string> { rem[0], fresh }));
                    cur = fresh;
                    rem.RemoveAt(0);
                }
                final.Add((cur, rem));
            }

            var allNT = new HashSet<string>(freshNTs.Where(s => NonTerminals.Contains(s) || s.StartsWith("T_") || s.StartsWith("X")));
            // collect all NTs that actually appear
            foreach (var (lhs, rhs) in final)
            {
                allNT.Add(lhs);
                foreach (var s in rhs) if (!Terminals.Contains(s)) allNT.Add(s);
            }

            return new Grammar(allNT, Terminals, final, Start);
        }

        // ─────────────────────────────────────────────
        //  Helpers
        // ─────────────────────────────────────────────

        private static void AddIfAbsent(
            List<(string, List<string>)> list, string lhs, List<string> rhs)
        {
            if (!list.Any(p => p.Item1 == lhs &&
                               p.Item2.Count == rhs.Count &&
                               p.Item2.SequenceEqual(rhs)))
                list.Add((lhs, new List<string>(rhs)));
        }
    }
}