using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShorkSharp.Parser
{
    internal class ParseResult
    {
        Node node { get; set; }
        SyntaxError? error { get; set; }

        int lastRegisteredAdvanceCount { get; set; }
        int advanceCount { get; set; }
        int toReverseCount { get; set; }

        public void RegisterAdvancement()
        {
            lastRegisteredAdvanceCount = 1;
            advanceCount++;
        }

        public Node Register(ParseResult res)
        {
            lastRegisteredAdvanceCount = res.lastRegisteredAdvanceCount;
            advanceCount += res.advanceCount;
            if (res.error is not null) this.error = res.error;
            return res.node;
        }

        public Node TryRegister(ParseResult res)
        {
            if (res.error is not null)
            {
                toReverseCount = res.advanceCount;
                return null;
            }
            return Register(res);
        }

        public ParseResult RegisterSuccess(Node node)
        {
            this.node = node;
            return this;
        }

        public ParseResult RegisterFailure(SyntaxError error)
        {
            if (error is null || lastRegisteredAdvanceCount == 0)
                this.error = error;
            return this;
        }
    }
}
