using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LSystemEngine
{
    private List<ProductionRule> rules;
    private List<List<Symbol>> history = new List<List<Symbol>>();
    
    public int CurrentStep { get; private set; } = 0;
    public bool IsFinished { get; private set; } = false;

    public LSystemEngine(List<Symbol> axiom, List<ProductionRule> rules)
    {
        this.rules = rules;
        history.Add(axiom); // Passo 0
    }

    public List<Symbol> GetCurrentWord() => history[CurrentStep];

    public void DeriveNext()
    {
        if (IsFinished) return;

        List<Symbol> currentWord = GetCurrentWord();
        List<Symbol> nextWord = new List<Symbol>();

        // Derivação em paralelo para cada símbolo
        foreach (Symbol symbol in currentWord)
        {
            // Encontrar a regra de produção adequada ao símbolo
            ProductionRule matchedRule = rules.FirstOrDefault(r => r.Matches(symbol));

            if (matchedRule != null)
            {
                // Aplica a regra de produção
                foreach (Symbol succ in matchedRule.Successor)
                {
                    nextWord.Add(new Symbol(succ.Id, succ.Parameter));
                }
            }
            else
            {
                // Produção Identidade: copia sem alterações
                nextWord.Add(new Symbol(symbol.Id, symbol.Parameter));
            }
        }

        // Verifica se a evolução terminou (palavra idêntica à anterior)
        if (WordsAreEqual(currentWord, nextWord))
        {
            IsFinished = true;
            Debug.Log("<color=green>Evolução Concluída! Nenhum símbolo foi alterado.</color>");
            return;
        }

        history.Add(nextWord);
        CurrentStep++;
    }

    public void DerivePrevious()
    {
        if (CurrentStep > 0)
        {
            CurrentStep--;
            IsFinished = false; // Se recuar, reativa a evolução
        }
    }

    private bool WordsAreEqual(List<Symbol> w1, List<Symbol> w2)
    {
        if (w1.Count != w2.Count) return false;
        for (int i = 0; i < w1.Count; i++)
        {
            if (w1[i].Id != w2[i].Id || w1[i].Parameter != w2[i].Parameter)
                return false;
        }
        return true;
    }

    public string GetFormattedWord(List<Symbol> word)
    {
        return string.Join(" ", word.Select(s => s.ToString()));
    }
}