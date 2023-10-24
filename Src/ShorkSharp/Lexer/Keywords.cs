using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShorkSharp.Lexer
{
    internal partial class Lexer
    {
        public readonly string[] Keywords =
        {
            "VAR",

            "AND",
            "OR",
            "NOT",

            "IF",
            "ELIF",
            "ELSE",
            
            "FUN",

            "FOR",
            "TO",
            "STEP",
            "WHILE",
            "THEN",
            "END",
            "RETURN",
            "CONTINUE",
            "BREAK"
        };
    }
}
