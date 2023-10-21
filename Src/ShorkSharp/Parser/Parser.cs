using ShorkSharp.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShorkSharp.Parser
{
    internal class Parser
    {
        public List<Token> tokens {  get; protected set; }
        public int tokenIndex { get; protected set; }
        public Token currentToken
        {
            get
            {
                if (tokenIndex < 0) return null;
                else if (tokenIndex >= tokens.Count) return null;
                else return tokens[tokenIndex];
            }
        }

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
            this.tokenIndex = -1;
            Advance();
        }

        public void Advance()
        {
            tokenIndex++;
        }

        public List<Node> MakeNodes()
        {
            List<Node> nodes = new List<Node>();

            // TODO: Create a node tree from the token list

            return nodes;
        }
    }
}
