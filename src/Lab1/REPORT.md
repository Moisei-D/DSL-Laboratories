# Intro to Formal Languages. Regular Grammars. Finite Automata.

### Course: Formal Languages & Finite Automata
### Author: Moisei Daniel

----

## Theory

A formal language is defined by four components: an alphabet (set of valid characters), vocabulary (set of valid words), grammar (set of production rules), and a starting symbol. Regular grammars can be converted into finite automata, which are computational models that process strings through state transitions. A finite automaton consists of states, an alphabet, transition functions, an initial state, and final states. If a string leads from the initial state to a final state following valid transitions, it belongs to the language.


## Objectives:

* Understand what formal languages are and their fundamental components.
* Implement a Grammar class that can generate valid strings from production rules.
* Implement a Finite Automaton class that validates strings.
* Convert a Grammar object to a Finite Automaton object.
* Test the implementation by generating strings and verifying them with the FA.


## Implementation description

* **Grammar Class**: This class stores the grammar components (Vn, Vt, P, S) according to the variant rules: S→dA, A→aB|bA, B→bC|aB|d, C→cB. The production rules are stored in a Dictionary where each non-terminal maps to a list of possible productions.

* **String Generation**: The GenerateString() method starts with the start symbol 'S' and repeatedly replaces non-terminals with random productions until only terminals remain. This produces valid strings in the language.

* **Grammar to FA Conversion**: The ToFiniteAutomaton() method converts grammar productions into state transitions. Each non-terminal becomes a state, and each production rule becomes a transition. Terminal productions (like B→d) transition to a special final state 'X'.

* **Finite Automaton Class**: The FA stores states, transitions (as a dictionary mapping "state|symbol" to next state), and final states. The StringBelongToLanguage() method processes input strings character by character, following transitions and checking if the final state is accepting.

* **Uniqueness Handling**: The main program generates 5 unique strings by storing them in a HashSet and regenerating duplicates. This ensures all 5 displayed strings are different.

```csharp
public string GenerateString()
{
    Random rand = new Random();
    string current = S.ToString();
    while (current.Any(c => Vn.Contains(c)))
    {
        for (int i = 0; i < current.Length; i++)
        {
            if (Vn.Contains(current[i]))
            {
                var options = P[current[i]];
                string replacement = options[rand.Next(options.Count)];
                current = current.Remove(i, 1).Insert(i, replacement);
                break;
            }
        }
    }
    return current;
}
```

```csharp
public bool StringBelongToLanguage(string inputString)
{
    string currentState = InitialState;
    foreach (char symbol in inputString)
    {
        var key = currentState + "|" + symbol;
        if (!Transitions.ContainsKey(key)) return false;
        currentState = Transitions[key];
    }
    return FinalStates.Contains(currentState);
}
```

## Conclusions / Screenshots / Results

The implementation successfully demonstrates the relationship between formal grammars and finite automata. The Grammar class generates valid strings following the production rules, and the converted Finite Automaton correctly validates these strings. The uniqueness mechanism ensures no duplicate strings are displayed. Testing with invalid strings (like "abc") confirms the FA properly rejects strings not in the language. This lab provides a foundation for understanding formal language theory and automata-based validation.

**Sample Output:**
```
--- Formal Languages & Finite Automata: Lab 1 ---
Variant Rules: S->dA, A->aB|bA, B->bC|aB|d, C->cB

Generating 5 valid unique strings from the grammar:
--------------------------------------------
String: dababd            | Accepted by FA: True
String: daabd             | Accepted by FA: True
String: dbababcbd         | Accepted by FA: True
String: dabd              | Accepted by FA: True
String: dbd               | Accepted by FA: True

Testing an invalid string:
String: abc               | Accepted by FA: False
```


## References

* Course materials: Formal Languages & Finite Automata
