using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShorkSharp.Lexer
{
    internal partial class Lexer
    {
        public string text { get; protected set; }
        public Position position { get; protected set; }

        public const string WHITESPACE = " \t";
        public const string DIGITS = "0123456789";
        public const string LETTERS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

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

                // Skip Whitespace that is not part of another token
                if (WHITESPACE.Contains(currentChar))
                {
                    Advance();
                }

                // Skip comment lines
                else if (currentChar == '#')
                {
                    SkipComment();
                }

                else if (";\n".Contains(currentChar))
                {
                    tokens.Add(new Token(TokenType.NEWLINE, position));
                    Advance();
                }

                // Make number tokens
                else if (DIGITS.Contains(currentChar))
                {
                    tokens.Add(MakeNumber());
                }

                else if (LETTERS.Contains(currentChar))
                {
                    tokens.Add(MakeIdentifier());
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
                    tokens.Add(MakeMinusOrArrow());
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
                else if (currentChar == '^')
                {
                    tokens.Add(new Token(TokenType.CARET, position.Copy()));
                    Advance();
                }
                else if (currentChar == '!')
                {
                    tokens.Add(MakeNotEquals());
                }
                else if (currentChar == '%')
                {
                    tokens.Add(new Token(TokenType.PERCENTAGE, position.Copy()));
                    Advance();
                }
                else if (currentChar == ',')
                {
                    tokens.Add(new Token(TokenType.COMMA, position.Copy()));
                    Advance();
                }
                else if (currentChar == '=')
                {
                    tokens.Add(MakeEquals());
                }
                else if (currentChar == '<')
                {
                    tokens.Add(MakeLessThan());
                }
                else if (currentChar == '>')
                {
                    tokens.Add(MakeGreaterThan());
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

            tokens.Add(new Token(TokenType.EOF, position));
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

        Token MakeIdentifier()
        {
            string id = "";
            Position startPosition = position.Copy();

            while (currentChar is not null && (LETTERS + DIGITS + "_").Contains((char)currentChar))
            {
                id += currentChar;
                Advance();
            }

            TokenType ttype = (Keywords.Contains(id)) ? TokenType.KEYWORD : TokenType.IDENTIFIER;
            return new Token(ttype, startPosition, id, position);
        }

        Token MakeMinusOrArrow()
        {
            TokenType ttype = TokenType.MINUS;
            Position startPosition = position.Copy();
            Advance();

            if (currentChar == '>')
            {
                Advance();
                ttype = TokenType.ARROW;
            }

            return new Token(ttype, startPosition, position);
        }

        Token MakeNotEquals()
        {
            Position startPosition = position.Copy();
            Advance();

            if (currentChar == '=')
            {
                Advance();
                return new Token(TokenType.NOTEQUAL, startPosition, position);
            }
            else
            {
                Advance();
                throw new ShorkException(new ExpectedCharacterError(startPosition, position, '=', (char)currentChar));
            }
        }

        Token MakeEquals()
        {
            TokenType ttype = TokenType.EQUALS;
            Position startPosition = position.Copy();
            Advance();

            if (currentChar == '=')
            {
                Advance();
                ttype = TokenType.DOUBLE_EQUALS;
            }

            return new Token(ttype, startPosition, position);
        }

        Token MakeLessThan()
        {
            TokenType ttype = TokenType.LESS_THAN;
            Position startPosition = position.Copy();
            Advance();

            if (currentChar == '=')
            {
                Advance();
                ttype = TokenType.LESS_THAN_OR_EQUAL;
            }

            return new Token(ttype, startPosition, position);
        }

        Token MakeGreaterThan()
        {
            TokenType ttype = TokenType.GREATER_THAN;
            Position startPosition = position.Copy();
            Advance();

            if (currentChar == '=')
            {
                Advance();
                ttype = TokenType.GREATER_THAN_OR_EQUAL;
            }

            return new Token(ttype, startPosition, position);
        }

        void SkipComment()
        {
            Advance();
            while (currentChar != '\n')
                Advance();
            Advance();
        }
    }
}
