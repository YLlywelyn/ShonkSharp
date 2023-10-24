using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShorkSharp.Lexer
{
    [Flags]
    internal enum TokenType
    {
        Unknown,

        INT,    // [0-9]+
        FLOAT,  // [0-9]+\.[0-9]+
        STRING, // "[a-zA-Z \t]*"

        IDENTIFIER,
        KEYWORD,

        NEWLINE, // \n

        PLUS,           // +
        MINUS,          // -
        SLASH,          // /
        ASTERIX,        // *
        CARET,          // ^
        PERCENTAGE,     // %
        COMMA,          // ,

        ARROW,          // ->

        EQUALS,                 // =
        DOUBLE_EQUALS,          // ==
        LESS_THAN_OR_EQUAL,     // <=
        GREATER_THAN_OR_EQUAL,  // >=
        LESS_THAN,              // <
        GREATER_THAN,           // >
        NOTEQUAL,               // !=

        LPAREN, // (
        RPAREN, // )
        LBRACE, // {
        RBRACE, // }
        LBRACK, // [
        RBRACK, // ]

        EOF,
    }
}
