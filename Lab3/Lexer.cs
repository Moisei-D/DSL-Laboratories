using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Lab3
{
    public class Lexer
    {
        private readonly string _input;
        private int _position;              // Points to the current character in input
        private int _readPosition;          // Points to the next character to be read
        private char _ch;                   // The current character being examined

        //Lookup table for the keywords to distinguish for general identifiers
        private static readonly Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>
        {
            { "fn", TokenType.Function },
            { "let", TokenType.Let },
            { "true", TokenType.True },
            { "false", TokenType.False },
            { "if", TokenType.If },
            { "else", TokenType.Else },
            { "return", TokenType.Return },

            //Special for trigonometry keywords
            { "sin", TokenType.Identifier },
            { "cos", TokenType.Identifier }
        };

        public Lexer(string input)
        {
            _input = input;
            ReadChar(); //Initialze the first character pointer
        }

        /// <summary>
        /// Reads the next character and advances the position pointers
        /// </summary>
        private void ReadChar()
        {
            if(_readPosition >= _input.Length)
            {
                _ch = '\0'; //Null character signals the EOF
            }
            else
            {
                _ch = _input[_readPosition];
            }
            _position = _readPosition;
            _readPosition++;
        }

        private char PeekChar()
        {
            if (_readPosition >= _input.Length)
            { 
                return '\0';
            }
            else
            {
                return _input[_readPosition];
            }
        }

        public Token NextToken()
        {
            Token token;
            SkipWhiteSpace();

            switch (_ch)
            {
                case '=':
                    if (PeekChar() == '=')
                    { // Check for equality "=="
                        char prevCh = _ch;
                        ReadChar();
                        token = new Token(TokenType.Eq, $"{prevCh}{_ch}");
                    }
                    else
                    {
                        token = new Token(TokenType.Assign, _ch.ToString());
                    }
                    break;
                case '+':
                    token = new Token(TokenType.Plus, _ch.ToString());
                    break;
                case '-':
                    token = new Token(TokenType.Minus, _ch.ToString());
                    break;
                case '*':
                    token = new Token(TokenType.Asterisk, _ch.ToString());
                    break;
                case '/':
                    token = new Token(TokenType.Slash, _ch.ToString());
                    break;
                case '(':
                    token = new Token(TokenType.LParen, _ch.ToString());
                    break;
                case ')':
                    token = new Token(TokenType.RParen, _ch.ToString());
                    break;
                case ';':
                    token = new Token(TokenType.Semicolon, _ch.ToString());
                    break;
                case '\0':
                    token = new Token(TokenType.Eof, "");
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

        //Logic for reading a multi-character identifier
        private string ReadIdentifier()
        {
            int start = _position;
            while (char.IsLetter(_ch) || _ch == '_')
            {
                ReadChar();
            }
            return _input.Substring(start, _position - start);
        }

        //Logic for reading numbers
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


        private void SkipWhiteSpace()
        {
            while (char.IsWhiteSpace(_ch))
            {
                ReadChar();
            }
        }

        private TokenType LookupIdentifier(string literal)
        {
            return Keywords.TryGetValue(literal, out var type) ? type : TokenType.Identifier;
        }



    }   

}
