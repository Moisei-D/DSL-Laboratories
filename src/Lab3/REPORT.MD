# Lexer & Scanner

### Course: Formal Languages & Finite Automata
### Author: Moisei Daniel

----

## Theory

A lexer (or scanner/tokenizer) performs lexical analysis, which is the process of converting a raw sequence of characters into a stream of tokens. Tokens are categorized units such as identifiers, numbers, keywords, operators, and delimiters. The lexeme is the raw substring from the input, while the token assigns a type to that lexeme (and may include metadata like position or literal value). Lexers are typically the first stage in language processing pipelines such as compilers and interpreters.

## Objectives

* Understand what lexical analysis is.
* Get familiar with the internal workflow of a lexer/scanner.
* Implement a lexer that recognizes identifiers, numbers (integers and floats), operators, delimiters, and keywords.

## Implementation description

* **Token Definition**: Token categories are defined in `TokenType` (identifiers, numbers, operators, delimiters, and keywords). A `Token` stores the token type and the literal string.
* **Character Stream Tracking**: The lexer maintains `_position`, `_readPosition`, and `_ch` to read the input sequentially.
* **Whitespace Handling**: Whitespace is skipped using `SkipWhiteSpace()` before processing tokens.
* **Identifiers & Keywords**: Identifiers are read as sequences of letters or `_`. A keyword lookup table differentiates reserved words (`fn`, `let`, `if`, `return`, etc.).
* **Numbers (int/float)**: `ReadNumber()` supports integers and decimals (one dot allowed).
* **Operators & Delimiters**: Single-character tokens are recognized with a switch; `==` is handled as a two-character token.

```csharp
public Token NextToken()
{
    Token token;
    SkipWhiteSpace();

    switch (_ch)
    {
        case '=':
            if (PeekChar() == '=')
            {
                char prevCh = _ch;
                ReadChar();
                token = new Token(TokenType.Eq, $"{prevCh}{_ch}");
            }
            else
            {
                token = new Token(TokenType.Assign, _ch.ToString());
            }
            break;
        default:
            if (char.IsLetter(_ch))
            {
                string literal = ReadIdentifier();
                return new Token(LookupIdentifier(literal), literal);
            }
            else if (char.IsDigit(_ch))
            {
                return new Token(TokenType.Number, ReadNumber());
            }
            else
            {
                token = new Token(TokenType.Illegal, _ch.ToString());
            }
            break;
    }
    ReadChar();
    return token;
}
```

```csharp
private string ReadNumber()
{
    int start = _position;
    bool hasDecimal = false;

    while (char.IsDigit(_ch) || (_ch == '.' && !hasDecimal))
    {
        if (_ch == '.') hasDecimal = true;
        ReadChar();
    }
    return _input.Substring(start, _position - start);
}
```

## Conclusions / Screenshots / Results

The lexer correctly splits the input into tokens and categorizes them using `TokenType`. Identifiers are distinguished from keywords, numbers allow floating point literals, and operators/delimiters are recognized as separate tokens. The output depends on the content of the input file (`samples\demo.txt`). If the file is empty, only the `Eof` token is produced.

## Difficulties / Challenges faced

One of the main challenges was fully understanding what a lexer does and how lexemes turn into categorized tokens. Implementing the token set also required repeated coding and careful handling of many token types (operators, delimiters, keywords, and literals) to ensure each was recognized correctly.

## References

* Course materials: Formal Languages & Finite Automata
