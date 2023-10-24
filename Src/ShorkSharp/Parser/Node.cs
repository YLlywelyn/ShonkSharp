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
        
        public override string ToString()
        {
            return string.Format("({0})", token);
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
        
        public override string ToString()
        {
            return string.Format("({0})", token);
        }
    }

    internal class ListNode : Node
    {
        public List<Node> elements;

        public ListNode(List<Node> elements, Position startPosition, Position endPosition)
            : base(startPosition, endPosition)
        {
            this.elements = elements;
        }
        
        public override string ToString()
        {
            return string.Format("(LIST: {{{0}}})", elements.ToDelimitedString());
        }
    }

    internal class VarAccessNode : Node
    {
        public Token token { get; protected set; }

        public VarAccessNode(Token token)
            : base(token.startPosition, token.endPosition)
        {
            this.token = token;
        }
    }

    internal class VarAssignNode : Node
    {
        public Token varNameToken { get; protected set; }
        public Node varValueNode { get; protected set; }

        public VarAssignNode(Token varNameToken, Node varValueNode)
            : base(varNameToken.startPosition, varValueNode.endPosition)
        {
            this.varNameToken = varNameToken;
            this.varValueNode = varValueNode;
        }
    }

    internal class BinOpNode : Node
    {
        public Node leftNode { get; protected set; }
        public Token opToken { get; protected set; }
        public Node rightNode { get; protected set; }

        public BinOpNode(Node leftNode, Token opToken, Node rightNode)
            : base(leftNode.startPosition, rightNode.endPosition)
        {
            this.leftNode = leftNode;
            this.opToken = opToken;
            this.rightNode = rightNode;
        }
        
        public override string ToString()
        {
            return string.Format("({0} {1} {2})", leftNode, opToken, rightNode);
        }
    }

    internal class UnaryOpNode : Node
    {
        public Token opToken {get; protected set; }
        public Node node { get; protected set; }

        public UnaryOpNode(Token opToken, Node node)
            : base(opToken.startPosition, node.endPosition)
        {
            this.opToken = opToken;
            this.node = node;
        }
        
        public override string ToString()
        {
            return string.Format("({0}{1})", opToken, node);
        }
    }

    /*
    internal class IfNode : Node
    {

    }
    */

    /*
    internal class ForNode : Node
    {

    }
    */
}
