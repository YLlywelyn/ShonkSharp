﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ShorkSharp.Lexer;

namespace ShorkSharp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("-> ");
                string input = Console.ReadLine();

                try
                {
                    Lexer.Lexer lexer = new Lexer.Lexer(input);
                    List<Token> tokens = lexer.MakeTokens();
                    Console.WriteLine(tokens.ToDelimitedString());
                }
                catch(ShorkException e)
                {
                    Console.WriteLine(e.error);
                }
                finally { Console.WriteLine(); }
            }
        }
    }
}