using System;
using System.Collections.Generic;

namespace Lab1
{
    class Program
    {
        static void Main(string[] args)
        {
            // 1. Initialize the Grammar with your specific variant rules
            Grammar myGrammar = new Grammar();

            // 2. Convert the Grammar to a Finite Automaton object
            FiniteAutomaton myFA = myGrammar.ToFiniteAutomaton();

            Console.WriteLine("--- Formal Languages & Finite Automata: Lab 1 ---");
            Console.WriteLine("Variant Rules: S->dA, A->aB|bA, B->bC|aB|d, C->cB\n");

            // 3. Generate 5 valid strings and verify them using the FA
            Console.WriteLine("Generating 5 valid unique strings from the grammar:");
            Console.WriteLine("--------------------------------------------");

            var uniqueStrings = new HashSet<string>();
            int printed = 0;
            int attempts = 0;
            const int maxAttempts = 100000
                ; // To prevent infinite loops in case of duplicates
                
            while (printed < 5 && attempts < maxAttempts)
            {
                string generated = myGrammar.GenerateString();
                if (!uniqueStrings.Contains(generated))
                {
                    uniqueStrings.Add(generated);
                    bool isValid = myFA.StringBelongToLanguage(generated);
                    Console.WriteLine($"String: {generated,-18} | Accepted by FA: {isValid}");
                    printed++;
                }
                attempts++;
            }

            // 4. Test an invalid string to prove the FA works
            Console.WriteLine("\nTesting an invalid string:");
            string invalidTest = "abc"; 
            bool invalidResult = myFA.StringBelongToLanguage(invalidTest);
            Console.WriteLine($"String: {invalidTest,-18} | Accepted by FA: {invalidResult}");

            // Keep the console window open
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}