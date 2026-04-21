using System;
using System.Collections.Generic;

namespace CNF
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("╔══════════════════════════════════════════════════════╗");
            Console.WriteLine("║   Chomsky Normal Form Converter — Variant 17         ║");
            Console.WriteLine("║   Course: Formal Languages & Finite Automata         ║");
            Console.WriteLine("║   Author: Moisei Daniel                              ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════╝");
            Console.WriteLine();

            // ── Build Variant 17 grammar ──────────────────────────────────────────
            //
            //  G = (V_N, V_T, P, S)
            //  V_N = {S, A, B, C, D, E}
            //  V_T = {a, b}
            //
            //  P = { 1. S→aA     5. A→BC    9.  C→ε
            //        2. S→AC     6. A→aD    10. C→BA
            //        3. A→a      7. B→b     11. E→aB
            //        4. A→ASC    8. B→bA    12. D→abC }

            var nonTerminals = new[] { "S", "A", "B", "C", "D", "E" };
            var terminals = new[] { "a", "b" };

            var productions = new (string, List<string>)[]
            {
                ("S", new List<string> { "a", "A" }),           //  1. S → aA
                ("S", new List<string> { "A", "C" }),           //  2. S → AC
                ("A", new List<string> { "a" }),                //  3. A → a
                ("A", new List<string> { "A", "S", "C" }),     //  4. A → ASC
                ("A", new List<string> { "B", "C" }),           //  5. A → BC
                ("A", new List<string> { "a", "D" }),           //  6. A → aD
                ("B", new List<string> { "b" }),                //  7. B → b
                ("B", new List<string> { "b", "A" }),           //  8. B → bA
                ("C", new List<string>()),                      //  9. C → ε
                ("C", new List<string> { "B", "A" }),           // 10. C → BA
                ("E", new List<string> { "a", "B" }),           // 11. E → aB
                ("D", new List<string> { "a", "b", "C" }),     // 12. D → abC
            };

            var grammar = new Grammar(nonTerminals, terminals, productions, "S");

            // ── Display original grammar ──────────────────────────────────────────
            Banner("Original Grammar (Variant 17)");
            Console.Write(grammar);

            // ── BONUS: accept any user-supplied grammar ───────────────────────────
            if (args.Length > 0 && args[0] == "--interactive")
            {
                grammar = ReadGrammarFromUser();
                Banner("User-supplied Grammar");
                Console.Write(grammar);
            }

            // ── Step 1: Eliminate ε-productions ──────────────────────────────────
            Banner("Step 1 — Eliminate ε-Productions");
            grammar = grammar.EliminateEpsilon();
            Console.Write(grammar);

            // ── Step 2: Eliminate renaming / unit productions ─────────────────────
            Banner("Step 2 — Eliminate Renaming (Unit) Productions");
            grammar = grammar.EliminateRenaming();
            Console.Write(grammar);

            // ── Step 3: Eliminate inaccessible symbols ────────────────────────────
            Banner("Step 3 — Eliminate Inaccessible Symbols");
            grammar = grammar.EliminateInaccessible();
            Console.Write(grammar);

            // ── Step 4: Eliminate non-productive symbols ──────────────────────────
            Banner("Step 4 — Eliminate Non-Productive Symbols");
            grammar = grammar.EliminateNonProductive();
            Console.Write(grammar);

            // ── Step 5: Convert to CNF ────────────────────────────────────────────
            Banner("Step 5 — Chomsky Normal Form");
            grammar = grammar.ToCNF();
            Console.Write(grammar);

            // ── Verify CNF ────────────────────────────────────────────────────────
            Banner("CNF Verification");
            VerifyCNF(grammar);

            Console.WriteLine("\nPress any key to exit…");
            Console.ReadKey();
        }

        // ─────────────────────────────────────────────
        //  BONUS – interactive grammar input
        // ─────────────────────────────────────────────

        static Grammar ReadGrammarFromUser()
        {
            Console.WriteLine("=== Interactive Grammar Input ===");
            Console.Write("Enter non-terminals (space-separated): ");
            var nts = Console.ReadLine()!.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            Console.Write("Enter terminals (space-separated): ");
            var ts = Console.ReadLine()!.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            Console.Write("Enter start symbol: ");
            var start = Console.ReadLine()!.Trim();

            Console.WriteLine("Enter productions one per line in the form  LHS -> RHS");
            Console.WriteLine("  Use 'eps' for ε.  Example:  S -> a B");
            Console.WriteLine("  Type 'done' when finished.");

            var prods = new List<(string, List<string>)>();
            while (true)
            {
                var line = Console.ReadLine()!.Trim();
                if (line.ToLower() == "done") break;
                var parts = line.Split(new[] { "->" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2) { Console.WriteLine("  !! Bad format, try again"); continue; }

                var lhs = parts[0].Trim();
                var rhsParts = parts[1].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var rhs = new List<string>();
                foreach (var p in rhsParts)
                    if (p != "eps") rhs.Add(p);

                prods.Add((lhs, rhs));
            }

            return new Grammar(nts, ts, prods, start);
        }

        // ─────────────────────────────────────────────
        //  CNF verifier
        // ─────────────────────────────────────────────

        static void VerifyCNF(Grammar g)
        {
            bool ok = true;
            foreach (var (lhs, rhs) in g.Productions)
            {
                bool validUnit = rhs.Count == 1 && g.Terminals.Contains(rhs[0]);
                bool validBinary = rhs.Count == 2
                                   && g.NonTerminals.Contains(rhs[0])
                                   && g.NonTerminals.Contains(rhs[1]);
                // Allow S → ε only if start symbol was nullable
                bool validEps = rhs.Count == 0 && lhs == g.Start;

                if (!validUnit && !validBinary && !validEps)
                {
                    Console.WriteLine($"  ✗ INVALID: {lhs} → {(rhs.Count == 0 ? "ε" : string.Join(" ", rhs))}");
                    ok = false;
                }
            }

            if (ok)
            {
                Console.WriteLine("  ✓ All productions conform to Chomsky Normal Form.");
                Console.WriteLine($"  ✓ Total productions: {g.Productions.Count}");
                Console.WriteLine($"  ✓ Non-terminals:     {string.Join(", ", g.NonTerminals)}");
            }
        }

        static void Banner(string title)
        {
            Console.WriteLine();
            Console.WriteLine(new string('─', 60));
            Console.WriteLine($"  {title}");
            Console.WriteLine(new string('─', 60));
        }
    }
}