using System;
using System.Collections.Generic;
using System.Globalization;

// Re-use the Token / TokenType / Lexer from Lab3 (LexerScanner project).

using Lab3;

namespace Lab6
{
    /// <summary>
    /// Recursive-descent parser.
    ///
    /// Grammar (informal):
    ///   program     := statement* EOF
    ///   statement   := letStmt
    ///   letStmt     := 'let' IDENTIFIER '=' expression ';'
    ///   expression  := term ( ('+' | '-') term )*
    ///   term        := primary                          (no * / for simplicity, easy to extend)
    ///   primary     := NUMBER
    ///               | IDENTIFIER '(' argList ')'       // function call
    ///               | IDENTIFIER
    ///               | '(' expression ')'
    ///   argList     := expression ( ',' expression )*
    /// </summary>
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _pos;

        public List<string> Errors { get; } = new List<string>();

        public Parser(List<Token> tokens) => _tokens = tokens;

        // ── helpers ──────────────────────────────────────────────────────────

        private Token Current => _tokens[_pos];

        private Token Peek(int offset = 1)
        {
            int idx = _pos + offset;
            return idx < _tokens.Count ? _tokens[idx] : _tokens[^1];
        }

        private Token Consume()
        {
            var t = Current;
            _pos++;
            return t;
        }

        private Token Expect(TokenType type)
        {
            if (Current.Type != type)
            {
                Errors.Add($"Expected {type} but got {Current.Type} ('{Current.Literal}')");
                // return a dummy token to keep parsing
                return new Token(type, "");
            }
            return Consume();
        }

        // ── public entry point ────────────────────────────────────────────────

        public ProgramNode ParseProgram()
        {
            var program = new ProgramNode();

            while (Current.Type != TokenType.Eof)
            {
                var stmt = ParseStatement();
                if (stmt != null)
                    program.Statements.Add(stmt);
            }

            return program;
        }

        // ── statements ────────────────────────────────────────────────────────

        private AstNode ParseStatement()
        {
            if (Current.Type == TokenType.Let)
                return ParseLetStatement();

            // Unknown statement — skip token to avoid infinite loop
            Errors.Add($"Unexpected token '{Current.Literal}' ({Current.Type}) at start of statement");
            Consume();
            return null;
        }

        private LetStatementNode ParseLetStatement()
        {
            Expect(TokenType.Let);

            string name = Current.Literal;
            Expect(TokenType.Identifier);

            Expect(TokenType.Assign);

            var value = ParseExpression();

            Expect(TokenType.Semicolon);

            return new LetStatementNode(name, value);
        }

        // ── expressions ───────────────────────────────────────────────────────

        /// expression := term ( ('+' | '-') term )*
        private AstNode ParseExpression()
        {
            var left = ParseTerm();

            while (Current.Type == TokenType.Plus || Current.Type == TokenType.Minus)
            {
                string op = Current.Literal;
                Consume();
                var right = ParseTerm();
                left = new BinaryExpressionNode(op, left, right);
            }

            return left;
        }

        /// term := factor ( ('*' | '/') factor )*
        private AstNode ParseTerm()
        {
            var left = ParseFactor();

            while (Current.Type == TokenType.Asterisk || Current.Type == TokenType.Slash)
            {
                string op = Current.Literal;
                Consume();
                var right = ParseFactor();
                left = new BinaryExpressionNode(op, left, right);
            }

            return left;
        }

        /// factor := unary
        private AstNode ParseFactor() => ParseUnary();

        /// unary := '-' unary | primary
        private AstNode ParseUnary()
        {
            if (Current.Type == TokenType.Minus)
            {
                string op = Current.Literal;
                Consume();
                var operand = ParseUnary();
                // represent unary minus as (0 - operand)
                return new BinaryExpressionNode(op, new NumberLiteralNode(0), operand);
            }
            return ParsePrimary();
        }

        /// primary := NUMBER | IDENTIFIER '(' argList ')' | IDENTIFIER | '(' expression ')'
        private AstNode ParsePrimary()
        {
            // Number literal
            if (Current.Type == TokenType.Number)
            {
                double val = double.Parse(Current.Literal, CultureInfo.InvariantCulture);
                Consume();
                return new NumberLiteralNode(val);
            }

            // Identifier or function call
            if (Current.Type == TokenType.Identifier)
            {
                string name = Current.Literal;
                Consume();

                // function call: identifier followed by '('
                if (Current.Type == TokenType.LParen)
                {
                    Consume(); // consume '('
                    var args = ParseArgList();
                    Expect(TokenType.RParen);
                    return new FunctionCallNode(name, args);
                }

                return new IdentifierNode(name);
            }

            // Grouped expression
            if (Current.Type == TokenType.LParen)
            {
                Consume(); // consume '('
                var expr = ParseExpression();
                Expect(TokenType.RParen);
                return expr;
            }

            // Error recovery
            Errors.Add($"Unexpected token '{Current.Literal}' ({Current.Type}) in expression");
            Consume();
            return new NumberLiteralNode(0); // placeholder
        }

        /// argList := expression ( ',' expression )*
        private List<AstNode> ParseArgList()
        {
            var args = new List<AstNode>();

            if (Current.Type == TokenType.RParen)
                return args; // empty arg list

            args.Add(ParseExpression());

            while (Current.Type == TokenType.Comma)
            {
                Consume(); // consume ','
                args.Add(ParseExpression());
            }

            return args;
        }
    }
}