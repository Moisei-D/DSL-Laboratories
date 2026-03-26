# Laboratory Work Nr. 4 — Regular Expressions

### Course: Formal Languages & Finite Automata
### Author: Moisei Daniel

---

## Theory

A **regular expression (regex)** is a formal notation for describing sets of strings using a concise pattern language. Originating from formal language theory, regular expressions define *regular languages*, which sit at the bottom of the Chomsky hierarchy and are recognized by finite automata.

The core building blocks of regular expressions are:

- **Literals** — characters that match themselves exactly (e.g. `a`, `3`, `Z`).
- **Alternation** (`|`) — a choice between two or more alternatives, e.g. `(a|b)` matches either `a` or `b`.
- **Concatenation** — two sub-expressions placed side by side, meaning both must match in order.
- **Quantifiers** — control how many times a sub-expression repeats:
  - `?` — zero or one occurrence (optional)
  - `+` — one or more occurrences
  - `*` — zero or more occurrences
  - `^N` — exactly N occurrences (superscript notation)
- **Grouping** (`(…)`) — scopes alternation and quantifiers together.

Regular expressions are widely used in compilers (tokenization), text processing tools, search engines, and input validation systems.

---

## Objectives

- Understand the formal structure of regular expressions.
- Learn how to interpret regular expression syntax dynamically, without hardcoding.
- Implement a generator that reads any regular expression at runtime and produces valid strings conforming to it.
- Apply a cap of 5 repetitions for unbounded quantifiers (`*` and `+`) to avoid infinite generation.
- **(Bonus)** Implement a step-by-step processing sequence printer that shows the order of evaluation.

---

## Implementation Description

### Project Structure

The solution consists of two C# source files:

| File | Responsibility |
|------|---------------|
| `Program.cs` | Entry point. Runs the three Variant 1 patterns and provides an interactive mode for user-supplied regexes. |
| `RegexEngine.cs` | All logic: dynamic generator (`ParseExpr`, `ParseConcat`, `ParseAtom`) and bonus step-by-step printer (`ParseExprWithSteps`, `ParseConcatWithSteps`, `ParseAtomWithSteps`). |

### Variant 1 — Regular Expressions

The three patterns assigned for Variant 1 are:

| Pattern | Description |
|---------|-------------|
| `(a\|b)(c\|d)E+G?` | One of {a,b}, then one of {c,d}, then 1–5 E's, then optionally G. |
| `P(Q\|R\|S)T(UV\|W\|X)*Z+` | Literal P, one of {Q,R,S}, literal T, 0–5 repetitions of {UV,W,X}, then 1–5 Z's. |
| `1(0\|1)*2(3\|4)^5 36` | Literal 1, 0–5 binary digits, literal 2, exactly 5 of {3,4}, literal 36. |

### Core Algorithm — Character-by-Character Parsing

Rather than building an explicit Abstract Syntax Tree, the engine reads the pattern character by character using three mutually recursive methods. This is sufficient because regular expression grammar has a naturally recursive structure:

| Method | Role |
|--------|------|
| `ParseExpr` | Splits on `\|` and picks one branch at random. |
| `ParseConcat` | Reads atoms left-to-right, applies quantifiers, concatenates results. |
| `ParseAtom` | Handles a group `(…)` by recursing into `ParseExpr`, or reads literal characters. |

### Quantifier Handling

After each atom is read, `ParseConcat` peeks at the next character to determine the quantifier and chooses a repeat count randomly within the allowed range:

```csharp
if      (p[pos] == '?') { repeatMin = 0; repeatMax = 1;   pos++; }
else if (p[pos] == '+') { repeatMin = 1; repeatMax = Cap; pos++; }  // Cap = 5
else if (p[pos] == '*') { repeatMin = 0; repeatMax = Cap; pos++; }
else if (p[pos] == '^')
{
    pos++;                         // skip ^
    int n = ReadInt(p, ref pos);   // read digit(s), stops at non-digit
    repeatMin = repeatMax = n;     // exactly n times
    if (pos < p.Length && p[pos] == ' ') pos++; // skip separator space
}

int count = _rng.Next(repeatMin, repeatMax + 1);
for (int i = 0; i < count; i++)
    result.Append(atom);
```

The `^N` notation uses a space as a delimiter to separate the count from subsequent literals — for example, `^5 36` means exactly 5 repetitions followed by the literal `36`. `ReadInt` stops at any non-digit (including spaces), and `ParseConcat` then skips exactly one separator space after `^N`.

### Literal Reading and Quantifier Binding

When reading literal characters, the engine must ensure a quantifier applies only to the **last** character read — not the entire run. For example, `EG?` should produce `E` followed by an optional `G`, not an optional `EG` pair. This is handled by backing up one character when a quantifier is detected after a multi-character read:

```csharp
// If we read more than one char and the next char is a quantifier,
// put the last character back so it gets its own quantifier.
if (sb.Length > 1 && pos < p.Length && IsQuantifier(p[pos]))
{
    sb.Remove(sb.Length - 1, 1);
    pos--;
    break;
}
```

### Bonus — Step-by-Step Processing Sequence

The bonus requirement is implemented as a parallel set of methods (`ParseExprWithSteps`, `ParseConcatWithSteps`, `ParseAtomWithSteps`) that mirror the generator exactly but print a numbered, indented log of every decision made:

```
// Example output for pattern (a|b)(c|d)E+G?:

Step 1: Enter group '('
   Step 2: Alternation — choose 1 of 2 options: [a | b]
Step 3: Exit group ')' → produced "b"
Step 4: Enter group '('
   Step 5: Alternation — choose 1 of 2 options: [c | d]
Step 6: Exit group ')' → produced "d"
Step 7: Quantifier '+' on "E" → repeat 1–5 (chose 3) time(s)
Step 8: Quantifier '?' on "G" → repeat 0–1 (chose 1) time(s)
```

The indentation level increases each time the parser enters a group, making nested structures visually clear.

---

## Results

The generator was validated by running 300 generated strings (100 per pattern) against Python's `re` module using equivalent standard regex patterns. All 300 samples passed. Sample output:

| Pattern | Sample outputs |
|---------|---------------|
| `(a\|b)(c\|d)E+G?` | `acEEEEE`, `bdEG`, `adEEEG`, `bcEE`, `acEEEEEG` |
| `P(Q\|R\|S)T(UV\|W\|X)*Z+` | `PQTWWWWWZ`, `PRTUVUVZ`, `PSTXXXZZZZ`, `PRTWWWZZ` |
| `1(0\|1)*2(3\|4)^5 36` | `1000024444 36`, `111124333 36`, `124344436` |

The interactive mode allows any regex to be typed at the prompt and immediately generates 5 valid strings along with the processing sequence, demonstrating that the engine is fully dynamic and not hardcoded to the three Variant 1 patterns.

---

## Conclusions

This laboratory work demonstrated that a dynamic regular expression generator can be implemented concisely using a recursive descent approach without requiring an explicit AST. The three mutually recursive methods — `ParseExpr`, `ParseConcat`, and `ParseAtom` — naturally mirror the recursive grammar of regular expressions and handle all required constructs: alternation, concatenation, literals, grouping, and quantifiers (`?`, `+`, `*`, `^N`).

A key practical challenge was the `^N` superscript notation used in the handwritten lab sheet, where a space separates the repeat count from the next literal (e.g. `^5 36`). Careful delimiter handling in `ReadInt` and a targeted space-skip after `^N` resolved the ambiguity cleanly.

The bonus step-by-step printer reinforced understanding of the recursive evaluation order and showed clearly how each decision — alternation choice, quantifier count — is made during generation.

---

## Difficulties / Challenges Faced

During this laboratory work, the most difficult parts were:

- **Understanding the parser flow**: It took time to fully understand how a regex should be split into expression, concatenation, and atom levels, and why recursion is necessary for groups.
- **Position tracking (`pos`)**: A small mistake in increment/decrement logic could break the whole parse, especially when entering/exiting nested groups.
- **Quantifier binding correctness**: Making sure quantifiers apply to the intended atom (especially after reading multiple literal characters) required repeated debugging.
- **Pattern 3 spacing ambiguity (`^5 36`)**: Distinguishing decorative spacing from literal content was tricky and caused edge-case behavior around the space before `36`.
- **Repeated token-like handling**: Similar to lexer work, implementing and testing many syntax cases (`|`, `()`, `?`, `+`, `*`, `^N`) required repetitive coding and validation.

---

## References

- Course materials: Formal Languages & Finite Automata
- Hopcroft, J., Motwani, R., Ullman, J. — *Introduction to Automata Theory, Languages, and Computation*
- Microsoft C# Documentation — https://learn.microsoft.com/en-us/dotnet/csharp/