using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShorkSharp
{
    internal class ShorkException : Exception
    {
        public Error error { get; private set; }

        public ShorkException(Error error) { this.error = error; }
        public ShorkException(string message, Error error) : base(message) { this.error = error; }
        public ShorkException(string message, Exception innerException, Error error) : base(message, innerException) { this.error = error; }

        public static void Throw(Error error)
        {
            throw new ShorkException(error);
        }
    }

    internal abstract class Error
    {
        public Position startPosition { get; private set; }
        public Position endPosition { get; private set; }
        public string errorName { get; private set; }
        public string errorMessage { get; private set; }

        protected Error(Position startPosition, Position endPosition, string errorName, string errorMessage)
        {
            this.startPosition = startPosition;
            this.endPosition = endPosition;
            this.errorName = errorName;
            this.errorMessage = errorMessage;
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}\nFile: {2}, line {3}",
                                    errorName,
                                    errorMessage,
                                    startPosition.filename,
                                    startPosition.line + 1);
        }
    }

    internal class IllegalCharError : Error
    {
        public IllegalCharError(Position startPosition, Position endPosition, char theChar)
            : base(startPosition, endPosition, "Illegal Character", string.Format("Invalid character '{0}'.", theChar))
        { }
    }

    internal class MultipleDecimalPointsError : Error
    {
        public MultipleDecimalPointsError(Position startPosition, Position endPosition)
            : base(startPosition, endPosition, "Multiple Decimal Points", "Number should have either 0 or 1 decimal points.")
        { }
    }

    internal class InvalidEscapeSequenceError : Error
    {
        public InvalidEscapeSequenceError(Position startPosition, Position endPosition, char theChar)
            : base(startPosition, endPosition, "Invalid Escape Sequence", string.Format("'{0}' is not a valid escape sequence.", theChar))
        { }
    }

    internal class ExpectedCharacterError : Error
    {
        public ExpectedCharacterError(Position startPosition, Position endPosition, char expected, char found)
            : base(startPosition, endPosition, "Expected Character", string.Format("Expected '{0}', found '{1}'.", expected, found))
        { }
    }

    internal class SyntaxError : Error
    {
        public SyntaxError(Position startPosition, Position endPosition, string details)
            : base(startPosition, endPosition, "Syntax Error", details)
        { }
    }
}
