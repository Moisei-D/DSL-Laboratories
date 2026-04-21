namespace RegexGenerator;

class Program
{
    static void Main()
    {
        // The three regexes from Variant 1
        string[] patterns =
        {
            "(a|b)(c|d)E+G?",
            "P(Q|R|S)T(UV|W|X)*Z+",
            "1(0|1)*2(3|4)^5 36"
        };

        Console.WriteLine("=== Regex Generator — Variant 1 ===\n");

        foreach (var pattern in patterns)
        {
            Console.WriteLine($"Pattern: {pattern}");
            Console.WriteLine(new string('-', 45));

            //Generate 5 valid strings
            Console.WriteLine("Generated strings:");
            for (int i = 0; i < 5; i++)
                Console.WriteLine($"  {i + 1}. {RegexEngine.Generate(pattern)}");

            //Bonus: show processing steps for one generation
            Console.WriteLine("\nBonus — processing steps:");
            RegexEngine.ShowSteps(pattern);

            Console.WriteLine();
        }

        // Interactive mode
        Console.WriteLine("=== Try your own regex (Enter to quit) ===");
        while (true)
        {
            Console.Write("> ");
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) break;

            try
            {
                for (int i = 0; i < 5; i++)
                    Console.WriteLine($"  {i + 1}. {RegexEngine.Generate(input)}");

                Console.WriteLine("\nSteps:");
                RegexEngine.ShowSteps(input);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error: {ex.Message}");
            }
            Console.WriteLine();
        }
    }
}