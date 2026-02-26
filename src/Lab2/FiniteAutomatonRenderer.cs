using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Lab2
{
    public static class FiniteAutomatonRenderer
    {
        public static string ToDot(FiniteAutomaton automaton, string graphName = "FA")
        {
            var emptyStates = new HashSet<string>(StringComparer.Ordinal) { "∅", "φ" };
            var sb = new StringBuilder();
            sb.AppendLine($"digraph {graphName} {{");
            sb.AppendLine("    rankdir=LR;");
            sb.AppendLine("    node [shape=circle];");
            sb.AppendLine("    __start__ [shape=point];");
            sb.AppendLine($"    __start__ -> \"{Escape(automaton.InitialState)}\";");

            foreach (var finalState in automaton.FinalStates.OrderBy(s => s, StringComparer.Ordinal))
            {
                sb.AppendLine($"    \"{Escape(finalState)}\" [shape=doublecircle];");
            }

            foreach (var transition in automaton.Transitions.OrderBy(t => t.Key.State, StringComparer.Ordinal)
                         .ThenBy(t => t.Key.Symbol, StringComparer.Ordinal))
            {
                var from = Escape(transition.Key.State);
                if (emptyStates.Contains(transition.Key.State))
                {
                    continue;
                }
                var symbol = Escape(transition.Key.Symbol);

                foreach (var to in transition.Value.OrderBy(s => s, StringComparer.Ordinal))
                {
                    if (emptyStates.Contains(to))
                    {
                        continue;
                    }
                    sb.AppendLine($"    \"{from}\" -> \"{Escape(to)}\" [label=\"{symbol}\"]; ");
                }
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

        public static string SaveDotFile(FiniteAutomaton automaton, string graphName, string filePath)
        {
            var dot = ToDot(automaton, graphName);
            File.WriteAllText(filePath, dot);
            return filePath;
        }

        public static string RenderToPng(FiniteAutomaton automaton, string graphName, string dotFilePath, string pngFilePath)
        {
            SaveDotFile(automaton, graphName, dotFilePath);

            var startInfo = new ProcessStartInfo
            {
                FileName = "dot",
                Arguments = $"-Tpng \"{dotFilePath}\" -o \"{pngFilePath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(startInfo))
            {
                process?.WaitForExit();
            }

            return pngFilePath;
        }

        private static string Escape(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
