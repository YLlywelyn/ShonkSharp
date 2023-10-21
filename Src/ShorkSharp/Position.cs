using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShorkSharp
{
    internal class Position
    {
        public int index { get; protected set; }
        public int line { get; protected set; }
        public int column { get; protected set; }

        public string filename { get; protected set; }
        public string filetext { get; protected set; }

        public Position(int index, int line, int column, string filename = "<STDIN>", string filetext = "")
        {
            this.index = index;
            this.line = line;
            this.column = column;
            this.filename = filename;
            this.filetext = filetext;
        }

        public void Advance(char? currentChar)
        {
            index++;
            column++;

            if (currentChar == '\n')
            {
                column = 0;
                line++;
            }
        }

        public Position Copy()
        {
            return new Position(index, line, column, filename, filetext);
        }
    }
}
