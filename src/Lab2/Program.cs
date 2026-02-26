using System;
using System.Collections.Generic;
using System.Linq;
using Lab2;

var fa = new Lab2.FiniteAutomaton
{
    InitialState = "q0",
    FinalStates = new HashSet<string> { "q3" }
};

fa.AddTransition("q0", "a", "q0");
fa.AddTransition("q0", "a", "q1");
fa.AddTransition("q1", "b", "q1");
fa.AddTransition("q2", "b", "q3");
fa.AddTransition("q1", "a", "q2");
fa.AddTransition("q2", "a", "q0");

Console.WriteLine("Variant 17 FA");
Console.WriteLine($"Deterministic: {fa.IsDeterministic()}");

var grammar = fa.ToRegularGrammar();
Console.WriteLine($"Grammar classification: {grammar.ClassifyChomskyHierarchy()}");
Console.WriteLine("Regular grammar productions:");
foreach (var production in grammar.Productions)
{
    var rhsList = production.Value.Select(rhs => rhs.Count == 0 ? "ε" : string.Join(" ", rhs));
    Console.WriteLine($"{production.Key} -> {string.Join(" | ", rhsList)}");
}

var dfa = fa.ToDfa();
var emptyStates = new HashSet<string>(StringComparer.Ordinal) { "∅", "φ" };
Console.WriteLine($"DFA states: {string.Join(", ", dfa.States.OrderBy(s => s, StringComparer.Ordinal))}");
Console.WriteLine($"DFA final states: {string.Join(", ", dfa.FinalStates.OrderBy(s => s, StringComparer.Ordinal))}");
Console.WriteLine("DFA transitions:");
foreach (var transition in dfa.Transitions.OrderBy(t => t.Key.State, StringComparer.Ordinal)
             .ThenBy(t => t.Key.Symbol, StringComparer.Ordinal))
{
    if (emptyStates.Contains(transition.Key.State))
    {
        continue;
    }

    var filteredTargets = transition.Value.Where(target => !emptyStates.Contains(target)).ToList();
    if (filteredTargets.Count == 0)
    {
        continue;
    }

    var targets = string.Join(", ", filteredTargets.OrderBy(s => s, StringComparer.Ordinal));
    Console.WriteLine($"δ({transition.Key.State}, {transition.Key.Symbol}) = {targets}");
}

var outputDir = AppContext.BaseDirectory;
var dotPath = System.IO.Path.Combine(outputDir, "variant17nfa.dot");
var pngPath = System.IO.Path.Combine(outputDir, "variant17nfa.png");
var dfaDotPath = System.IO.Path.Combine(outputDir, "variant17dfa.dot");
var dfaPngPath = System.IO.Path.Combine(outputDir, "variant17dfa.png");

try
{
    FiniteAutomatonRenderer.RenderToPng(fa, "Variant17NFA", dotPath, pngPath);
    Console.WriteLine($"\nGraph image generated at: {pngPath}");
    FiniteAutomatonRenderer.RenderToPng(dfa, "Variant17DFA", dfaDotPath, dfaPngPath);
    Console.WriteLine($"Graph image generated at: {dfaPngPath}");
    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
    {
        FileName = pngPath,
        UseShellExecute = true
    });
    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
    {
        FileName = dfaPngPath,
        UseShellExecute = true
    });
}
catch (Exception ex)
{
    Console.WriteLine($"\nFailed to render graph. Ensure Graphviz is installed and 'dot' is on PATH. Details: {ex.Message}");
    Console.WriteLine("DOT file saved at: " + dotPath);
    Console.WriteLine(FiniteAutomatonRenderer.ToDot(fa, "Variant17NFA"));
    Console.WriteLine("DFA DOT file saved at: " + dfaDotPath);
    Console.WriteLine(FiniteAutomatonRenderer.ToDot(dfa, "Variant17DFA"));
}

// Keep the console window open
Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
