using ShorkSharp.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShorkSharp.Parser
{
    internal abstract class Node
    {
        public Position startPosition { get; protected set; }
        public Position endPosition { get; protected set; }

        public Node(Position startPosition, Position endPosition = null)
        {
            this.startPosition = startPosition.Copy();
            this.endPosition = (endPosition is null) ? startPosition.Copy() : endPosition.Copy();
        }
    }

    internal class NumberNode : Node
    {
        public Token token { get; protected set; }

        public NumberNode(Token token)
            : base(token.startPosition, token.endPosition)
        {
            this.token = token;
        }
    }

    internal class StringNode : Node
    {
        public Token token { get; protected set; }

        public StringNode(Token token)
            : base(token.startPosition, token.endPosition)
        {
            this.token = token;
        }
    }
}
