using System;
using System.IO;

namespace Lab3
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = "C:\\Users\\mrdan\\source\\repos\\Moisei-D\\DSL-Laboratories\\Lab3\\samples\\demo.txt";
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Error: Could not find {filePath}");
                return;
            }

            string sourceCode = File.ReadAllText(filePath);
            Console.WriteLine("=== Starting Lexical Analysis ===");
            Console.WriteLine($"Source Code:\n{sourceCode}\n");
            Console.WriteLine("Generated Tokens:");
            Console.WriteLine("--------------------------------------------------");

            Lexer lexer = new Lexer(sourceCode);
            Token token;

            do
            {
                token = lexer.NextToken();
                Console.WriteLine($"Type: {token.Type,-12} | Literal: '{token.Literal}'");
            }
            while (token.Type != TokenType.Eof);


            Console.WriteLine("--------------------------------------------------");
            Console.WriteLine("=== Lexical Analysis Complete ===");
            Console.ReadLine();
        }

    }
}