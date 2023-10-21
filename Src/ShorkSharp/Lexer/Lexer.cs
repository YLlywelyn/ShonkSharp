using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShorkSharp.Lexer
{
    internal class Lexer
    {
        public string text { get; protected set; }
        public Position position { get; protected set; }

        public const string WHITESPACE = " \t";
        public const string DIGITS = "0123456789";

        public Dictionary<char, string> ESCAPE_CHARS = new Dictionary<char, string>()
        {
            { '\\', "\\\\" },
            { 'n', "\n" },
            { 't', "\t" },
            { '"', "\"" },
        };

        public char? currentChar
        {
            get
            {
                if (position.index >= text.Length) return null;
                else if (position.index < 0) return null;
                else return text[position.index];
            }
        }

        public Lexer(string filetext, string filename = "<STDIN>")
        {
            text = filetext;
            position = new Position(-1, 0, -1, filename, filetext);
            Advance();
        }

        protected void Advance()
        {
            position.Advance(currentChar);
        }

        public List<Token> MakeTokens()
        {
            List<Token> tokens = new List<Token>();
            
            while (currentChar is not null)
            {
                char currentChar = (char)this.currentChar;

                // Skip Whitespace thast is not part of another token
                if (WHITESPACE.Contains(currentChar))
                {
                    Advance();
                }

                // Make number tokens
                else if (DIGITS.Contains(currentChar))
                {
                    tokens.Add(MakeNumber());
                }

                // Make string tokens
                else if (currentChar == '"')
                {
                    tokens.Add(MakeString());
                }

                #region Single character tokens
                else if (currentChar == '+')
                {
                    tokens.Add(new Token(TokenType.PLUS, position.Copy()));
                    Advance();
                }
                else if (currentChar == '-')
                {
                    tokens.Add(new Token(TokenType.MINUS, position.Copy()));
                    Advance();
                }
                else if (currentChar == '/')
                {
                    tokens.Add(new Token(TokenType.SLASH, position.Copy()));
                    Advance();
                }
                else if (currentChar == '*')
                {
                    tokens.Add(new Token(TokenType.ASTERIX, position.Copy()));
                    Advance();
                }
                else if (currentChar == '&')
                {
                    tokens.Add(new Token(TokenType.AMPERSAND, position.Copy()));
                    Advance();
                }
                else if (currentChar == '|')
                {
                    tokens.Add(new Token(TokenType.PIPE, position.Copy()));
                    Advance();
                }
                else if (currentChar == '^')
                {
                    tokens.Add(new Token(TokenType.CARET, position.Copy()));
                    Advance();
                }
                else if (currentChar == '?')
                {
                    tokens.Add(new Token(TokenType.QMARK, position.Copy()));
                    Advance();
                }
                else if (currentChar == '!')
                {
                    tokens.Add(new Token(TokenType.EXCLAMATION, position.Copy()));
                    Advance();
                }
                else if (currentChar == '%')
                {
                    tokens.Add(new Token(TokenType.PERCENTAGE, position.Copy()));
                    Advance();
                }
                else if (currentChar == '.')
                {
                    tokens.Add(new Token(TokenType.DOT, position.Copy()));
                    Advance();
                }
                else if (currentChar == ',')
                {
                    tokens.Add(new Token(TokenType.COMMA, position.Copy()));
                    Advance();
                }
                else if (currentChar == ';')
                {
                    tokens.Add(new Token(TokenType.SEMICOLON, position.Copy()));
                    Advance();
                }
                else if (currentChar == '(')
                {
                    tokens.Add(new Token(TokenType.LPAREN, position.Copy()));
                    Advance();
                }
                else if (currentChar == ')')
                {
                    tokens.Add(new Token(TokenType.RPAREN, position.Copy()));
                    Advance();
                }
                else if (currentChar == '{')
                {
                    tokens.Add(new Token(TokenType.LBRACE, position.Copy()));
                    Advance();
                }
                else if (currentChar == '}')
                {
                    tokens.Add(new Token(TokenType.RBRACE, position.Copy()));
                    Advance();
                }
                else if (currentChar == '[')
                {
                    tokens.Add(new Token(TokenType.LBRACK, position.Copy()));
                    Advance();
                }
                else if (currentChar == ']')
                {
                    tokens.Add(new Token(TokenType.RBRACK, position.Copy()));
                    Advance();
                }
                else if (currentChar == '<')
                {
                    tokens.Add(new Token(TokenType.LANGLE, position.Copy()));
                    Advance();
                }
                else if (currentChar == '>')
                {
                    tokens.Add(new Token(TokenType.RANGLE, position.Copy()));
                    Advance();
                }
                #endregion

                else
                {
                    // Illegal character found, throw error
                    Position startPosition = position.Copy();
                    char theChar = currentChar;
                    Advance();
                    ShorkException.Throw(new IllegalCharError(startPosition, position.Copy(), theChar));
                }
            }

            return tokens;
        }

        Token MakeNumber()
        {
            string numString = "";
            int dotCount = 0;
            Position startPosition = position.Copy();

            while (currentChar is not null && (DIGITS+".").Contains((char)currentChar))
            {
                char currentChar = (char)this.currentChar;
                if (currentChar == '.')
                {
                    if (dotCount > 0)
                    {
                        // THROW ERROR: NUMBER CANNOT HAVE MORE THAN ONE DECIMAL POINT
                        Advance();
                        ShorkException.Throw(new MultipleDecimalPointsError(startPosition, position.Copy()));
                    }
                    dotCount++;
                }
                numString += currentChar;

                Advance();
            }

            if (dotCount == 0) return new Token(TokenType.INT, startPosition, int.Parse(numString), position.Copy());
            else return new Token(TokenType.FLOAT, startPosition, double.Parse(numString), position.Copy());
        }

        Token MakeString()
        {
            string text = "";
            Position startPosition = position.Copy();
            Advance();

            while (currentChar is not null)
            {
                // Escape characters
                if (currentChar == '\\')
                {
                    Advance();
                    if (ESCAPE_CHARS.ContainsKey((char)currentChar))
                    {
                        text += ESCAPE_CHARS[(char)currentChar];
                    }
                    else
                        ShorkException.Throw(new InvalidEscapeSequenceError(startPosition, position.Copy(), (char)currentChar));
                }

                // Is the current character the same type of quote the string was opened with?
                else if (currentChar == '"')
                {
                    Advance();
                    break;
                }

                else
                {
                    text += currentChar;
                    Advance();
                }
            }

            return new Token(TokenType.STRING, startPosition, text, position.Copy());
        }
    }
}
