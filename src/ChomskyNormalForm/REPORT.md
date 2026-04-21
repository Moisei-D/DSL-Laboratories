# Laboratory Work Nr. 5 — Chomsky Normal Form

### Course: Formal Languages & Finite Automata
### Author: Moisei Daniel

---

## Theory

A **Context-Free Grammar (CFG)** G = (V_N, V_T, P, S) is in **Chomsky Normal Form (CNF)** if every production rule takes one of exactly two forms:

- **A → BC** — a non-terminal derives exactly two non-terminals, or
- **A → a** — a non-terminal derives exactly one terminal symbol.

The only allowed exception is **S → ε** when the empty string belongs to the language and S never appears on the right-hand side of any production.

CNF is important because it enables efficient parsing algorithms such as the **CYK (Cocke–Younger–Kasami)** algorithm, which decides in O(n³) time whether a string of length n belongs to the language defined by the grammar.

Any CFG can be converted to CNF in five systematic steps:

1. **Eliminate ε-productions** — find all nullable non-terminals and add new productions that cover all combinations of having those nullable symbols present or absent, then remove the ε rules.
2. **Eliminate renaming (unit) productions** — productions of the form A → B, where B is a single non-terminal. Replace them by computing the transitive closure of all reachable non-unit rules.
3. **Eliminate inaccessible symbols** — remove any non-terminal (and its associated productions) that cannot be reached from the start symbol.
4. **Eliminate non-productive symbols** — remove any non-terminal that cannot derive any string of terminals.
5. **Obtain CNF** — replace every terminal inside a production of length ≥ 2 with a fresh non-terminal (T\_a → a), then break every production of length > 2 into a chain of binary productions using auxiliary non-terminals (X1, X2, …).

---

## Objectives

1. Learn about Chomsky Normal Form (CNF).
2. Get familiar with the approaches of normalizing a grammar.
3. Implement a method for normalizing an input grammar by the rules of CNF, encapsulated in an appropriate class with a clear signature.
4. Execute and test the implementation.
5. **(Bonus)** Make the function accept any grammar, not only the one from the student's variant (interactive mode via the `--interactive` flag).

---

## Implementation Description

### Project Structure

The solution is a self-contained C# console application consisting of two source files:

| File | Responsibility |
|------|---------------|
| `src/Grammar.cs` | Core `Grammar` class: holds the grammar data and exposes the five CNF conversion steps as chainable methods. |
| `src/Program.cs` | Entry point: constructs Variant 17's grammar, runs each step in sequence, prints intermediate grammars, verifies the final CNF, and supports the bonus interactive mode. |

### Grammar Representation

The `Grammar` class stores:
- `HashSet<string> NonTerminals` — set of non-terminal symbol names.
- `HashSet<string> Terminals` — set of terminal symbol names.
- `List<(string Lhs, List<string> Rhs)>` — list of productions represented as (left-hand side, right-hand side list of symbols).
- `string Start` — the start symbol.

Each transformation method returns a **new** `Grammar` object, leaving the original unchanged and making the pipeline easy to trace.

### Variant 17 — Input Grammar

```
G = (V_N, V_T, P, S)
V_N = {S, A, B, C, D, E}
V_T = {a, b}

P = { 1.  S → aA        5.  A → BC      9.  C → ε
       2.  S → AC        6.  A → aD     10.  C → BA
       3.  A → a         7.  B → b      11.  E → aB
       4.  A → ASC       8.  B → bA     12.  D → abC }
```

### Step 1 — Eliminate ε-Productions

The algorithm first computes all **nullable** non-terminals — those that can derive the empty string. It starts with non-terminals that have an explicit `X → ε` rule and propagates transitively: if every symbol on a right-hand side is nullable, the left-hand side is nullable too.

For Variant 17, **C is the only nullable non-terminal** (rule 9: C → ε).

For every production containing C, new productions are generated that cover all subsets of omitting C. The original ε rule is then dropped. Productions added include, for example:
- `S → AC` generates `S → A` (omit C)
- `A → ASC` generates `A → AS` (omit C)
- `A → BC` generates `A → B` (omit C)
- `D → abC` generates `D → ab` (omit C)

### Step 2 — Eliminate Unit Productions

Unit (renaming) productions have the form `A → B` where B is a single non-terminal. The algorithm computes, for each non-terminal A, the set of non-terminals reachable from A through chains of unit productions. It then copies all non-unit productions from those reachable non-terminals directly under A, and removes all unit rules.

For example, `S → A` (produced in Step 1) causes S to inherit all of A's non-unit productions: `S → a`, `S → ASC`, `S → AS`, `S → BC`, `S → aD`, `S → b`, `S → bA`. Similarly, `A → B` causes A to inherit `A → b` and `A → bA`.

### Step 3 — Eliminate Inaccessible Symbols

Starting from the start symbol S, a reachability closure is computed over all non-terminals that appear in the right-hand side of any reachable production. Any non-terminal not in this closure, along with its productions, is removed.

After Step 2, the non-terminal **E** is no longer reachable from S (the only production mentioning E on the left is `E → aB`, and no production has E on the right). E is therefore removed.

### Step 4 — Eliminate Non-Productive Symbols

A symbol is productive if it can derive some string of terminal symbols. The algorithm initialises the productive set with all terminals, then repeatedly adds non-terminals whose entire right-hand side consists of productive symbols. Non-productive symbols and their productions are removed.

After Step 3, all remaining non-terminals (S, A, B, C, D) are productive — each can eventually derive a string over {a, b}.

### Step 5 — Obtain Chomsky Normal Form

Two sub-steps are applied:

**5a — Terminal isolation.** For every production of length ≥ 2 that contains a terminal symbol, that terminal is replaced by a fresh non-terminal T\_x with the rule T\_x → x. This ensures no production mixes terminals and non-terminals.

- `T_a → a` and `T_b → b` are introduced.

**5b — Binary splitting.** Any production with three or more symbols on the right-hand side is split into a chain of binary productions using fresh auxiliary non-terminals X1, X2, …

For example:
- `S → ASC` becomes `S → AX1`, `X1 → SC`
- `A → ASC` becomes `A → AX2`, `X2 → SC`
- `D → abC` becomes `D → T_aX3`, `X3 → T_bC`

### Bonus — Accept Any Grammar

Running the program with the `--interactive` flag activates an input mode where the user can type any context-free grammar. Non-terminals, terminals, start symbol, and productions are entered at the prompt, and the same five-step pipeline is executed on the supplied grammar. Productions use the format `LHS -> sym1 sym2 ...` with `eps` representing ε.

---

## Results

The final grammar in CNF after all five steps:

```
Non-terminals : S, A, B, C, D, T_a, T_b, X1, X2, X3
Terminals     : a, b
Start         : S

Productions:
  S → T_a A       A → T_b A       B → T_b A
  S → AC          A → a           C → BA
  S → a           A → AX2         D → T_a X3
  S → AX1         X2 → SC         X3 → T_b C
  X1 → SC         A → AS          D → T_a T_b
  S → AS          A → BC          T_a → a
  S → BC          A → T_a D       T_b → b
  S → T_a D       A → b
  S → b
```

Total: **26 productions**, verified automatically — every rule is either A → a or A → BC.

### Console Output Trace

```
╔══════════════════════════════════════════════════════╗
║   Chomsky Normal Form Converter — Variant 17         ║
╚══════════════════════════════════════════════════════╝

──────────────────────────────────────────────────────
  Step 1 — Eliminate ε-Productions
──────────────────────────────────────────────────────
  Nullable non-terminals: { C }
  ... (intermediate grammar printed) ...

──────────────────────────────────────────────────────
  Step 5 — Chomsky Normal Form
──────────────────────────────────────────────────────
  ✓ All productions conform to Chomsky Normal Form.
  ✓ Total productions: 26
  ✓ Non-terminals: S, A, B, C, D, T_a, T_b, X1, X2, X3
```

---

## Conclusions

This laboratory work demonstrated the complete, step-by-step transformation of a context-free grammar into Chomsky Normal Form. Each of the five steps was implemented as a self-contained method in the `Grammar` class, making the pipeline transparent and easy to verify.

Several interesting observations arose from Variant 17's grammar. The non-terminal C was the sole source of ε-productions, and its elimination propagated into multiple rules, creating several new unit productions in Step 1. The transitive closure in Step 2 then resolved those unit rules efficiently. The non-terminal E turned out to be inaccessible — it appeared only as a left-hand side and was never referenced from S — so it was cleanly removed in Step 3.

The bonus interactive mode makes the converter general-purpose: any user-specified CFG can be normalised using the same pipeline, satisfying the requirement of accepting any grammar rather than only the assigned variant.

---

## Difficulties / Challenges Faced

- **Subset enumeration for ε-elimination** — generating all 2^k subsets of nullable positions for each production required careful bit-mask indexing to avoid both duplicates and the empty production.
- **Unit-production closure** — building the transitive reachability set for each non-terminal had to be done carefully to avoid including the starting non-terminal as reachable from itself in a way that would copy its own productions back.
- **Duplicate avoidance** — across all five steps, an `AddIfAbsent` helper was used to prevent the same production from appearing multiple times when multiple transformation paths lead to the same result.
- **Fresh non-terminal naming** — ensuring that generated names like `T_a`, `T_b`, and `X1` did not clash with existing non-terminal names required maintaining a running set of used names throughout Step 5.

---

## References

- [1] [Chomsky Normal Form — Wikipedia](https://en.wikipedia.org/wiki/Chomsky_normal_form)
- [2] Hopcroft, J., Motwani, R., Ullman, J. — *Introduction to Automata Theory, Languages, and Computation*, 3rd ed.
- [3] Course materials: Formal Languages & Finite Automata, Technical University of Moldova
- [4] Microsoft C# Documentation — https://learn.microsoft.com/en-us/dotnet/csharp/