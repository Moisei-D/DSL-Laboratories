using System;
using System.Collections.Generic;
using System.IO;
using Lab3;

namespace Lab6
{
    class Program
    {
        static void Main(string[] args)
        {
            // ── locate demo source file ─────────────────────────────────────
            string filePath = Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..", "LexerScanner", "samples", "demo.txt");

            // Fallback: look next to the executable
            if (!File.Exists(filePath))
                filePath = Path.Combine(AppContext.BaseDirectory, "demo.txt");

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: demo.txt not found. Expected at:\n  {filePath}");
                Console.WriteLine("Place demo.txt next to the executable or adjust the path.");
                Console.ReadLine();
                return;
            }

            string source = File.ReadAllText(filePath);

            PrintBanner();

            // ── 1. Lexical analysis ─────────────────────────────────────────
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("  Phase 1 — Lexical Analysis");
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine($"Source:\n{source}\n");

            var lexer = new Lexer(source);
            var tokens = new List<Token>();
            Token tok;
            do
            {
                tok = lexer.NextToken();
                tokens.Add(tok);
            } while (tok.Type != TokenType.Eof);

            Console.WriteLine("Tokens:");
            Console.WriteLine("─────────────────────────────────────────────────────");
            foreach (var t in tokens)
                Console.WriteLine($"  {t.Type,-14} '{t.Literal}'");

            // ── 2. Parsing ──────────────────────────────────────────────────
            Console.WriteLine();
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("  Phase 2 — Parsing (building AST)");
            Console.WriteLine("═══════════════════════════════════════════════════════");

            var parser = new Parser(tokens);
            ProgramNode ast = parser.ParseProgram();

            if (parser.Errors.Count > 0)
            {
                Console.WriteLine("Parser errors:");
                foreach (var err in parser.Errors)
                    Console.WriteLine($"  ✗ {err}");
            }
            else
            {
                Console.WriteLine("  ✓ No parse errors.\n");
            }

            // ── 3. Print AST ────────────────────────────────────────────────
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("  Phase 3 — Abstract Syntax Tree");
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine(ast.ToTreeString());

            Console.WriteLine();
            Console.WriteLine("─────────────────────────────────────────────────────");
            Console.WriteLine($"  ✓ Statements parsed : {ast.Statements.Count}");
            Console.WriteLine("  ✓ Parsing complete.");
            Console.WriteLine("─────────────────────────────────────────────────────");

            Console.ReadLine();
        }

        static void PrintBanner()
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
            Console.WriteLine("║   Lab 5 — Parser & Abstract Syntax Tree               ║");
            Console.WriteLine("║   Formal Languages & Finite Automata                  ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════╝");
            Console.WriteLine();
        }
    }
}