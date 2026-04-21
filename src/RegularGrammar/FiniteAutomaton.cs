using System;
using System.Collections.Generic;

namespace Lab1
{
    public class FiniteAutomaton
    {
        public string InitialState { get; set; }
        public HashSet<string> FinalStates { get; set; } = new HashSet<string>();
        public Dictionary<string, string> Transitions { get; set; } 
            = new Dictionary<string, string>();

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
    }
}
