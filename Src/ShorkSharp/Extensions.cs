using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShorkSharp.Lexer;

namespace ShorkSharp
{
    internal static class Extensions
    {
        public static string ToDelimitedString<T>(this IEnumerable<T> list)
        {
            return string.Join(", ", list);
        }

        public static bool HasValue(this TokenType tokenType)
        {
            switch (tokenType)
            {
                default: return false;

                case TokenType.INT:
                case TokenType.FLOAT:
                case TokenType.STRING:
                    return true;
            }
        }
    }
}
