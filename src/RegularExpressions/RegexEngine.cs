namespace RegexGenerator;

public static class RegexEngine
{
    private static readonly Random _rng = new();

    // Unlimited quantifiers (* and +) are capped at 5 
    private const int Cap = 5;


    /// <summary>Generate one valid string from the given pattern.</summary>
    public static string Generate(string pattern)
    {
        int pos = 0;
        return ParseExpr(pattern, ref pos);
    }

    /// <summary>Bonus: print a numbered sequence of processing steps.</summary>
    public static void ShowSteps(string pattern)
    {
        int pos = 0;
        int step = 0;
        ParseExprWithSteps(pattern, ref pos, ref step, depth: 0);
    }

    // ── Core generator (reads pattern left-to-right) ──────────────────────────
    private static string ParseExpr(string p, ref int pos)
    {
        var branches = new List<string>();
        branches.Add(ParseConcat(p, ref pos));

        while (pos < p.Length && p[pos] == '|')
        {
            pos++; // skip '|'
            branches.Add(ParseConcat(p, ref pos));
        }

        return branches[_rng.Next(branches.Count)];
    }

    // A "concat" is a sequence of (atom + optional quantifier)
    // e.g.  AB+C?  →  A, then 1-5 B's, then maybe C
    // Stops when it hits ) or | or end of string
    private static string ParseConcat(string p, ref int pos)
    {
        var result = new System.Text.StringBuilder();

        while (pos < p.Length && p[pos] != ')' && p[pos] != '|')
        {
            // Skip decorative spaces between tokens
            if (p[pos] == ' ') { pos++; continue; }

            string atom = ParseAtom(p, ref pos);

            int repeatMin = 1, repeatMax = 1;

            if (pos < p.Length)
            {
                if (p[pos] == '?') { repeatMin = 0; repeatMax = 1; pos++; }
                else if (p[pos] == '+') { repeatMin = 1; repeatMax = Cap; pos++; }
                else if (p[pos] == '*') { repeatMin = 0; repeatMax = Cap; pos++; }
                else if (p[pos] == '^')
                {
                    // ^N — read digits only; stop at space or non-digit
                    pos++; // skip ^
                    int n = ReadInt(p, ref pos);
                    repeatMin = repeatMax = n;
                    // skip the separating space if present (e.g. "^5 36")
                    if (pos < p.Length && p[pos] == ' ') pos++;
                }
            }

            int count = _rng.Next(repeatMin, repeatMax + 1);
            for (int i = 0; i < count; i++)
                result.Append(atom);
        }

        return result.ToString();
    }

    // An "atom" is either a group (...) or one or more literal characters
    private static string ParseAtom(string p, ref int pos)
    {
        if (p[pos] == '(')
        {
            pos++; // skip '('
            string inner = ParseExpr(p, ref pos);
            pos++; // skip ')'
            return inner;
        }

        // Read literal characters until we hit a metacharacter or space
        var sb = new System.Text.StringBuilder();
        while (pos < p.Length && !IsMeta(p[pos]) && p[pos] != ' ')
        {
            sb.Append(p[pos++]);

            // Stop before a quantifier so it applies only to the last char
            if (sb.Length > 0 && pos < p.Length && IsQuantifier(p[pos]))
            {
                if (sb.Length > 1) { sb.Remove(sb.Length - 1, 1); pos--; }
                break;
            }
        }

        return sb.ToString();
    }

    // ── Bonus: step-by-step printer (mirrors the generator exactly) ───────────

    private static string ParseExprWithSteps(string p, ref int pos, ref int step, int depth)
    {
        string indent = new(' ', depth * 3);
        var branches = new List<string>();

        branches.Add(ParseConcatWithSteps(p, ref pos, ref step, depth + 1));

        while (pos < p.Length && p[pos] == '|')
        {
            pos++;
            branches.Add(ParseConcatWithSteps(p, ref pos, ref step, depth + 1));
        }

        if (branches.Count > 1)
        {
            step++;
            Console.WriteLine($"{indent}Step {step}: Alternation — choose 1 of {branches.Count} options: " +
                              $"[{string.Join(" | ", branches)}]");
        }

        return branches[_rng.Next(branches.Count)];
    }

    private static string ParseConcatWithSteps(string p, ref int pos, ref int step, int depth)
    {
        string indent = new(' ', depth * 3);
        var result = new System.Text.StringBuilder();

        while (pos < p.Length && p[pos] != ')' && p[pos] != '|')
        {
            if (p[pos] == ' ') { pos++; continue; }

            string atom = ParseAtomWithSteps(p, ref pos, ref step, depth);

            int repeatMin = 1, repeatMax = 1;
            string quantSymbol = "";

            if (pos < p.Length)
            {
                if (p[pos] == '?') { repeatMin = 0; repeatMax = 1; quantSymbol = "?"; pos++; }
                else if (p[pos] == '+') { repeatMin = 1; repeatMax = Cap; quantSymbol = "+"; pos++; }
                else if (p[pos] == '*') { repeatMin = 0; repeatMax = Cap; quantSymbol = "*"; pos++; }
                else if (p[pos] == '^')
                {
                    pos++;
                    int n = ReadInt(p, ref pos);
                    repeatMin = repeatMax = n;
                    quantSymbol = $"^{n}";
                    if (pos < p.Length && p[pos] == ' ') pos++;
                }
            }

            int count = _rng.Next(repeatMin, repeatMax + 1);

            step++;
            if (quantSymbol != "")
            {
                string rangeDesc = repeatMin == repeatMax
                    ? $"exactly {repeatMin}"
                    : $"{repeatMin}–{repeatMax} (chose {count})";
                Console.WriteLine($"{indent}Step {step}: Quantifier '{quantSymbol}' on \"{atom}\" → repeat {rangeDesc} time(s)");
            }
            else
            {
                Console.WriteLine($"{indent}Step {step}: Literal \"{atom}\"");
            }

            for (int i = 0; i < count; i++)
                result.Append(atom);
        }

        return result.ToString();
    }

    private static string ParseAtomWithSteps(string p, ref int pos, ref int step, int depth)
    {
        string indent = new(' ', depth * 3);

        if (p[pos] == '(')
        {
            step++;
            Console.WriteLine($"{indent}Step {step}: Enter group '('");
            pos++;
            string inner = ParseExprWithSteps(p, ref pos, ref step, depth + 1);
            step++;
            Console.WriteLine($"{indent}Step {step}: Exit group ')' → produced \"{inner}\"");
            pos++;
            return inner;
        }

        var sb = new System.Text.StringBuilder();
        while (pos < p.Length && !IsMeta(p[pos]) && p[pos] != ' ')
        {
            sb.Append(p[pos++]);
            if (sb.Length > 0 && pos < p.Length && IsQuantifier(p[pos]))
            {
                if (sb.Length > 1) { sb.Remove(sb.Length - 1, 1); pos--; }
                break;
            }
        }

        return sb.ToString();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static bool IsMeta(char c) =>
        c == '(' || c == ')' || c == '|' || c == '?' || c == '+' || c == '*' || c == '^';

    private static bool IsQuantifier(char c) =>
        c == '?' || c == '+' || c == '*' || c == '^';

    // Read digits only; stops at non-digit (including space)
    private static int ReadInt(string p, ref int pos)
    {
        int start = pos;
        while (pos < p.Length && char.IsDigit(p[pos])) pos++;
        return int.Parse(p[start..pos]);
    }
}
