using System;
using System.Collections.Generic;
using System.Text;

namespace Lab3
{
    /// <summary>
    /// Defines all the possible categories of tokens the lexer can recognize
    /// </summary>
    public enum TokenType
    {
        // Special Marker tokens
        Illegal,  //Represent a character the lexer can't recognize
        EOF, //Represent the end of file/input

        //Identifiers and Literals
        Identifier,   //Names like x, cos, sin
        Number,  //Numerical values 3, 3.14


        //Operators
        Assign,     // "="
        Plus,       // "+"
        Minus,      // "-"
        Bang,       // "!"
        Asterick,   // "*"
        Slash,      // "/"
        Lt,         // "<"
        Gt,         // ">"
        Eq,         // "=="
        NotEq,      // "!="

        //Delimiters
        Comma,      // ","
        Semicolon,  // ";"
        LParen,     // "("
        RParen,     // ")"
        LBrace,     // "{"
        RBrace,     // "}"

        //Language Keywords
        Function,   // "fn"
        Let,        // "let"
        If,         // "if"
        Else,       // "else"
        Return,     // "return"
        True,       // "true"
        False      // "false"
    }
    /// <summary>
    /// Represents a single unit of meaning (a token) extracted from the source code
    /// </summary>
    public class Token
    {
        public TokenType Type { get; }

        public string Literal { get; }

        public Token(TokenType type, string literal)
        {
            Type = type;
            Literal = literal;
        }

        /// <summary>
        /// For debugging purposes
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Token {{ Type: {Type}, Literal: \"{Literal}\" }}";
        }
    }


}
