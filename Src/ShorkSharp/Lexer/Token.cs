using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShorkSharp.Lexer
{
    internal enum TokenType
    {
        Unknown,

        INT,    // [0-9]+
        FLOAT,  // [0-9]+\.[0-9]+
        STRING, // "[a-zA-Z \t]*"

        PLUS,           // +
        MINUS,          // -
        SLASH,          // /
        ASTERIX,        // *
        AMPERSAND,      // &
        PIPE,           // |
        CARET,          // ^
        QMARK,          // ?
        EXCLAMATION,    // !
        PERCENTAGE,     // %
        DOT,            // .
        COMMA,          // ,
        SEMICOLON,      // ;

        EQUALS,                 // =
        DOUBLE_EQUALS,          // ==
        LESS_THAN_OR_EQUAL,     // <=
        GREATER_THAN_OR_EQUAL,  // >=
        EQUALS_ARROW,           // =>

        LPAREN, // (
        RPAREN, // )
        LBRACE, // {
        RBRACE, // }
        LBRACK, // [
        RBRACK, // ]
        LANGLE, // <
        RANGLE, // >
    }

    internal class Token
    {
        public TokenType tokenType { get; protected set; }
        public Position startPosition { get; protected set; }
        public Position endPosition { get; protected set; }

        private dynamic? __value;
        public dynamic value
        {
            get
            {
                if (__value is null) return '\0';
                else return __value;
            }
            protected set { __value = value; }
        }

        public Token(TokenType tokenType, Position startPosition, dynamic? value = null, Position endPosition = null)
        {
            this.tokenType = tokenType;
            this.value = value;

            this.startPosition = startPosition.Copy();
            if (endPosition is not null)
                this.endPosition = endPosition.Copy();
            else
                this.endPosition = startPosition.Copy();
        }

        public override string ToString()
        {
            if (tokenType.HasValue())
                return string.Format("[{0}, {1}]", tokenType, value);
            else
                return string.Format("[{0}]", tokenType);
        }
    }
}
