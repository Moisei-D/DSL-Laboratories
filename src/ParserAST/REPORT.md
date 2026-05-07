# Laboratory Work Nr. 6 — Parser & Abstract Syntax Tree

### Course: Formal Languages & Finite Automata
### Author: Moisei Daniel

---

## Theory

**Parsing** is the process of analysing a sequence of tokens (produced by a lexer) to determine its grammatical structure with respect to a given formal grammar. The result of parsing is typically a **parse tree** or, more commonly used in compilers, an **Abstract Syntax Tree (AST)**.

An **Abstract Syntax Tree** is a hierarchical data structure in which each node represents a syntactic construct of the source program. Unlike a concrete parse tree, an AST omits redundant intermediate nodes (such as parentheses or punctuation tokens) that carry no semantic meaning — it retains only the structural essence of the program.

The parser implemented in this lab work uses the **recursive-descent** strategy. Recursive-descent is a top-down parsing technique in which a dedicated procedure is written for each grammar rule. The procedures call one another recursively, naturally mirroring the nested structure of the grammar.

The grammar recognised by this parser (written informally) is:

```
program     ::= statement* EOF
statement   ::= letStmt
letStmt     ::= 'let' IDENTIFIER '=' expression ';'
expression  ::= term ( ('+' | '-') term )*
term        ::= factor ( ('*' | '/') factor )*
factor      ::= unary
unary       ::= '-' unary | primary
primary     ::= NUMBER
             | IDENTIFIER '(' argList ')'
             | IDENTIFIER
             | '(' expression ')'
argList     ::= expression ( ',' expression )*
```

This grammar encodes **operator precedence** through the hierarchy of rules: addition/subtraction bind more loosely than multiplication/division, which bind more loosely than unary operators and primaries.

---

## Objectives

1. Get familiar with parsing and how it can be programmed.
2. Get familiar with the concept of AST.
3. In addition to the 3rd lab work (Lexer Scanner):
   - Confirm that `TokenType` enum and regex-based categorisation are in place.
   - Implement the necessary data structures for an AST suitable for the calculator language processed in Lab 3.
   - Implement a simple recursive-descent parser that extracts syntactic information from the token stream and builds the AST.

---

## Implementation Description

### Project Structure

The solution adds a new C# console project `ParserAST` alongside the existing `LexerScanner`. It reuses Lab 3's `Token`, `TokenType`, and `Lexer` classes without modification.

| File | Responsibility |
|------|----------------|
| `src/ParserAST/Ast.cs` | All AST node types (`AstNode` hierarchy). |
| `src/ParserAST/Parser.cs` | Recursive-descent parser that consumes a `List<Token>` and produces a `ProgramNode`. |
| `src/ParserAST/Program.cs` | Entry point: runs the lexer, feeds tokens to the parser, and prints the resulting AST. |
| `src/LexerScanner/Token.cs` | (Lab 3) `TokenType` enum and `Token` record. |
| `src/LexerScanner/Lexer.cs` | (Lab 3) Lexer — unchanged. |

### AST Node Hierarchy

All nodes inherit from the abstract base class `AstNode`, which exposes a `ToTreeString(int indent)` method for pretty-printing.

```
AstNode  (abstract)
├── ProgramNode          — root of the tree; holds a list of statements
├── LetStatementNode     — let <name> = <expr> ;
├── NumberLiteralNode    — a numeric constant (double)
├── IdentifierNode       — a variable or function name reference
├── BinaryExpressionNode — left <op> right  (op ∈ {+, -, *, /})
└── FunctionCallNode     — <name> ( arg0, arg1, … )
```

Each node stores only the information relevant to its kind. For example, `BinaryExpressionNode` holds the operator string and two child `AstNode` references; `FunctionCallNode` holds the function name and a `List<AstNode>` of arguments.

### Parser

The `Parser` class receives a pre-built `List<Token>` (the full token stream including the EOF sentinel). Internally it maintains an index `_pos` into that list and two helpers:

- `Current` — the token at `_pos`.
- `Consume()` — returns `Current` and advances `_pos`.
- `Expect(type)` — asserts the current token has the given type, consumes it, or records a parse error and returns a dummy token to allow recovery.

The main parsing methods mirror the grammar productions one-to-one:

```
ParseProgram()     → calls ParseStatement() in a loop until EOF
ParseStatement()   → dispatches on 'let' keyword
ParseLetStatement()→ consumes 'let', name, '=', expression, ';'
ParseExpression()  → handles + and − (left-associative)
ParseTerm()        → handles * and / (left-associative)
ParseFactor()      → delegates to ParseUnary()
ParseUnary()       → handles unary minus
ParsePrimary()     → handles literals, identifiers, calls, groups
ParseArgList()     → comma-separated list of expressions
```

Operator precedence is enforced naturally: `ParseExpression` calls `ParseTerm`, which calls `ParseFactor`, which calls `ParseUnary`, which calls `ParsePrimary`. A lower-precedence operator can only appear above a higher-precedence one in the call stack, so the resulting tree correctly reflects precedence.

### Reuse of Lab 3 — Lexer

The Lab 3 `Lexer` and `Token`/`TokenType` types are referenced from the `LexerScanner` project without any changes. The only addition needed (already present in Lab 3) is the `Comma` token type, used to separate function arguments. The `Parser` project simply adds a project reference to `LexerScanner`.

---

## Results

### Input

```
let pi = 3.1415;
let x = 10;
let y = sin(pi) + cos(x);
```

### Phase 1 — Token Stream

```
Let            'let'
Identifier     'pi'
Assign         '='
Number         '3.1415'
Semicolon      ';'
Let            'let'
Identifier     'x'
Assign         '='
Number         '10'
Semicolon      ';'
Let            'let'
Identifier     'y'
Assign         '='
Identifier     'sin'
LParen         '('
Identifier     'pi'
RParen         ')'
Plus           '+'
Identifier     'cos'
LParen         '('
Identifier     'x'
RParen         ')'
Semicolon      ';'
Eof            ''
```

### Phase 2 — Parser Output (AST)

```
Program
  statement[0]:
    LetStatement
      name: pi
      value:
        NumberLiteral(3.1415)
  statement[1]:
    LetStatement
      name: x
      value:
        NumberLiteral(10)
  statement[2]:
    LetStatement
      name: y
      value:
        BinaryExpression(+)
          left:
            FunctionCall(sin)
              arg[0]:
                Identifier(pi)
          right:
            FunctionCall(cos)
              arg[0]:
                Identifier(x)
```

### Console Output Trace

```
╔═══════════════════════════════════════════════════════╗
║   Lab 5 — Parser & Abstract Syntax Tree               ║
║   Formal Languages & Finite Automata                  ║
╚═══════════════════════════════════════════════════════╝

═══════════════════════════════════════════════════════
  Phase 1 — Lexical Analysis
═══════════════════════════════════════════════════════
Source:
let pi = 3.1415;
let x = 10;
let y = sin(pi) + cos(x);

Tokens:
─────────────────────────────────────────────────────
  Let            'let'
  Identifier     'pi'
  ...

═══════════════════════════════════════════════════════
  Phase 2 — Parsing (building AST)
═══════════════════════════════════════════════════════
  ✓ No parse errors.

═══════════════════════════════════════════════════════
  Phase 3 — Abstract Syntax Tree
═══════════════════════════════════════════════════════
Program
  statement[0]:
    ...

─────────────────────────────────────────────────────
  ✓ Statements parsed : 3
  ✓ Parsing complete.
─────────────────────────────────────────────────────
```

---

## Conclusions

This laboratory work extended the Lexer from Lab 3 into a full front-end consisting of a recursive-descent parser and a typed Abstract Syntax Tree.

The AST hierarchy cleanly separates concerns: each node kind carries only the data relevant to its construct, and the `ToTreeString` method makes the tree human-readable for debugging without coupling presentation logic to the parser itself.

Recursive-descent proved to be a natural fit for this grammar because every rule maps directly to a method. Operator precedence — which is often a source of complexity — was handled implicitly through the call chain (`ParseExpression → ParseTerm → ParseFactor → ParseUnary → ParsePrimary`), requiring no explicit precedence tables.

The clean separation between the `LexerScanner` project (Lab 3) and the new `ParserAST` project (Lab 6) keeps the codebase organised: Lab 3 remains untouched, and Lab 5 simply adds a project reference, reusing the token infrastructure without duplication.

---

## Difficulties / Challenges Faced

- **Left-associativity** — ensuring that `a + b + c` is parsed as `(a + b) + c` (left-associative) rather than right-associative required building the binary node inside the `while` loop so that each iteration wraps the accumulated left subtree.
- **Function-call disambiguation** — distinguishing a bare identifier (`x`) from a function call (`sin(...)`) required a one-token look-ahead after consuming the identifier name.
- **Error recovery** — when an unexpected token is encountered, the parser records the error and either skips the token or inserts a dummy placeholder node so that parsing can continue and report multiple errors in a single run rather than aborting on the first one.
- **Unary minus** — since the `Lexer` emits a single `Minus` token regardless of context, unary minus had to be handled explicitly in a dedicated `ParseUnary` method rather than in `ParseTerm`.

---

## References

- [1] [Parsing — Wikipedia](https://en.wikipedia.org/wiki/Parsing)
- [2] [Abstract Syntax Tree — Wikipedia](https://en.wikipedia.org/wiki/Abstract_syntax_tree)
- [3] Course materials: Formal Languages & Finite Automata, Technical University of Moldova
- [4] Microsoft C# Documentation — https://learn.microsoft.com/en-us/dotnet/csharp/