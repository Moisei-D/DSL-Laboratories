# Finite Automata. Regular Grammars. Determinization.

### Course: Formal Languages & Finite Automata
### Author: Moisei Daniel

----

## Overview

A finite automaton models processes as a state machine with a start state and one or more final states. Non-determinism appears when a transition on the same symbol can reach multiple states. Determinism describes predictability, while non-determinism indicates multiple possible next states. A non-deterministic automaton can be converted into a deterministic one using standard algorithms.

## Objectives:

* Understand what an automaton is and what it can be used for.
* Add grammar classification by Chomsky hierarchy to the grammar type/class.
* Use the previous variant to define the finite automaton.
* Convert a finite automaton to a regular grammar.
* Determine whether the automaton is deterministic or non-deterministic.
* Convert an NDFA to a DFA.
* Represent the finite automaton graphically (optional bonus).

## Implementation description

* **Grammar classification**: Implemented `ClassifyChomskyHierarchy` to detect Type-3 (regular) vs Type-2 (context-free) grammars based on production forms.

* **Finite automaton definition**: The main program defines the Variant 17 automaton transitions and prints whether it is deterministic.

* **FA → Regular grammar**: Each transition `A --a--> B` becomes a production `A -> a B`, and if `B` is final, also `A -> a`.

* **NDFA → DFA**: Subset construction is used; DFA states are sets of NFA states and transitions are computed with move and epsilon-closure.

* **Graph rendering (bonus)**: NFA and DFA graphs are rendered to DOT and PNG using Graphviz.

```csharp
var dfa = fa.ToDfa();
Console.WriteLine($"DFA states: {string.Join(", ", dfa.States.OrderBy(s => s, StringComparer.Ordinal))}");
Console.WriteLine($"DFA final states: {string.Join(", ", dfa.FinalStates.OrderBy(s => s, StringComparer.Ordinal))}");
```

## Conclusions / Screenshots / Results

The project implements grammar classification, NFA → grammar conversion, determinism checks, and NFA → DFA conversion. The generated DFA states and transitions confirm correct determinization, and the DOT/PNG output provides visual verification of the automata.

**Sample Output:**
```
Variant 17 FA
Deterministic: False
Grammar classification: Type-3 (Regular)
DFA states: q0, q1, q2, q3, {q0,q1}, {q0,q1,q2}, {q1,q3}
DFA final states: q3, {q1,q3}
DFA transitions:
δ(q0, a) = {q0,q1}
δ(q1, a) = q2
δ(q1, b) = q1
```

## References

* Course materials: Formal Languages & Finite Automata
