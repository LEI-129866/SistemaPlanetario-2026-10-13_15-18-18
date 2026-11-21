using System;
using System.Collections.Generic;
using System.Linq;

// motor de derivação probabilística (fase 2)
public class LSystemEngine
{
    // símbolos com movimento orbital e velocidades angulares (alpha, beta)
    private static readonly HashSet<string> OrbitingSymbols = new HashSet<string> { "Pc", "Pb", "Pa", "La" };

    private List<StochasticRule> rules;
    private List<List<Symbol>> history = new List<List<Symbol>>(); // histórico de palavras geradas por passo
    private System.Random rng; // gerador pseudo-aleatório com seed

    public int Seed { get; private set; }
    public int CurrentStep { get; private set; } = 0;
    public bool IsFinished { get; private set; } = false;

    // inicializa o motor com a configuração lida do ficheiro e uma seed para o gerador aleatório
    public LSystemEngine(SimulationConfig config, int seed = 0)
    {
        this.rules = config.rules;
        this.Seed = seed;
        this.rng = new System.Random(seed);

        // clone do axioma inicial para o passo 0
        List<Symbol> initialWord = config.axiom.Select(s => s.Clone()).ToList();
        history.Add(initialWord);

        // verifica se o axioma inicial já não tem regras aplicáveis
        CheckIfFinished(initialWord);
    }

    // lista de símbolos correspondente ao passo atual
    public List<Symbol> GetCurrentWord() => history[CurrentStep];

    // deriva a palavra seguinte através de regras estocásticas (em paralelo)
    public void DeriveNext()
    {
        // se o passo seguinte já existe no histórico (após ter recuado), recupera-o diretamente
        if (CurrentStep < history.Count - 1)
        {
            CurrentStep++;
            CheckIfFinished(history[CurrentStep]);
            return;
        }

        // se nenhum símbolo tem produções associadas, não deriva mais 
        if (IsFinished) return;

        List<Symbol> currentWord = GetCurrentWord();
        List<Symbol> nextWord = new List<Symbol>();

        // percorre cada símbolo para o substituir em paralelo
        foreach (Symbol symbol in currentWord)
        {
            // filtra todas as produções possíveis para este símbolo
            List<StochasticRule> matchingRules = rules.Where(r => r.predecessor == symbol.Id).ToList();

            if (matchingRules.Count > 0)
            {
                // escolhe uma das produções disponíveis de acordo com o seu peso (probabilidade)
                StochasticRule chosenRule = SelectWeightedRule(matchingRules);
                int orbitParamIndex = 0;

                // conta quantos corpos orbitais existem no sucessor para saber se há herança direta (ex: Pc -> Pb)
                int orbitingInSuccessor = chosenRule.successor.Count(s => OrbitingSymbols.Contains(s.Id));
                bool inheritBodyId = OrbitingSymbols.Contains(symbol.Id) && orbitingInSuccessor == 1;

                // construir os símbolos sucessores consoante a regra escolhida
                foreach (Symbol succ in chosenRule.successor)
                {
                    float alpha = 0f;
                    float beta = 0f;

                    // se o símbolo gerado tem órbita, extrai o par (alpha, beta) correspondente da regra
                    if (OrbitingSymbols.Contains(succ.Id) && orbitParamIndex < chosenRule.orbitParameters.Count)
                    {
                        OrbitParams op = chosenRule.orbitParameters[orbitParamIndex++];
                        alpha = op.rotationSpeed;
                        beta = op.orbitSpeed;
                    }
                    
                    // mantém o BodyId anterior se houver evolução direta, ou cria um novo (-1)
                    int bodyIdToUse = (inheritBodyId && OrbitingSymbols.Contains(succ.Id)) ? symbol.BodyId : -1;
                    nextWord.Add(new Symbol(succ.Id, succ.Parameter, alpha, beta, bodyIdToUse));
                }
            }
            else
            {
                // sem produção associada: mantém o símbolo exato (incluindo BodyId e velocidades)
                nextWord.Add(symbol.Clone());
            }
        }

        // guarda a nova palavra derivada no histórico e avança o contador de passos
        history.Add(nextWord);
        CurrentStep++;
        CheckIfFinished(nextWord);
    }

    // recua um passo na derivação, mantendo o histórico intacto
    public void DerivePrevious()
    {
        if (CurrentStep > 0)
        {
            CurrentStep--;
            IsFinished = false;
        }
    }

    // sorteia uma regra da lista - ponderada pelos pesos inteiros (p) de cada produção
    private StochasticRule SelectWeightedRule(List<StochasticRule> matchingRules)
    {
        if (matchingRules.Count == 1)
            return matchingRules[0];

        // soma os pesos de todas as regras candidatas
        int totalWeight = 0;
        foreach (var rule in matchingRules)
            totalWeight += rule.weight;

        // gera um número aleatório entre 0 e (totalWeight - 1) e encontra o intervalo correspondente
        int roll = rng.Next(0, totalWeight);
        int cumulative = 0;

        foreach (var rule in matchingRules)
        {
            cumulative += rule.weight;
            if (roll < cumulative)
                return rule;
        }

        return matchingRules[matchingRules.Count - 1];
    }

    // evolução termina quando nenhum símbolo da palavra atual tem produções no ficheiro de configuração
    private void CheckIfFinished(List<Symbol> word)
    {
        bool hasAnyRule = word.Any(sym => rules.Any(r => r.predecessor == sym.Id));
        IsFinished = !hasAnyRule;
    }

    // formata a palavra atual numa string legível (ex: "Eb ff ff ff G1")
    public string GetFormattedWord(List<Symbol> word)
    {
        return string.Join(" ", word.Select(s => s.ToString()));
    }
}


// -- Código Fase 1 -- //

// using System;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;

// public class LSystemEngine
// {
//     private List<ProductionRule> rules;
//     private List<List<Symbol>> history = new List<List<Symbol>>();
    
//     public int CurrentStep { get; private set; } = 0;
//     public bool IsFinished { get; private set; } = false;

//     public LSystemEngine(List<Symbol> axiom, List<ProductionRule> rules)
//     {
//         this.rules = rules;
//         history.Add(axiom); // passo 0
//     }

//     public List<Symbol> GetCurrentWord() => history[CurrentStep];

//     public void DeriveNext()
//     {
//         // se o passo seguinte já está no histórico,
//         // apenas avançamos o índice do passo atual sem recriar ou duplicar palavras
//         if (CurrentStep < history.Count - 1){
//             CurrentStep++;
//             return;
//         }

//         if (IsFinished) return;

//         List<Symbol> currentWord = GetCurrentWord();
//         List<Symbol> nextWord = new List<Symbol>();

//         // derivação em paralelo para cada símbolo
//         foreach (Symbol symbol in currentWord)
//         {
//             // encontrar a regra de produção adequada ao símbolo
//             ProductionRule matchedRule = rules.FirstOrDefault(r => r.Matches(symbol));

//             if (matchedRule != null)
//             {
//                 // aplica a regra de produção
//                 foreach (Symbol succ in matchedRule.Successor)
//                 {
//                     nextWord.Add(new Symbol(succ.Id, succ.Parameter));
//                 }
//             }
//             else
//             {
//                 // produção identidade: copia sem alterações
//                 nextWord.Add(new Symbol(symbol.Id, symbol.Parameter));
//             }
//         }

//         // verifica se a evolução terminou (palavra idêntica à anterior)
//         if (WordsAreEqual(currentWord, nextWord))
//         {
//             IsFinished = true;
//             // Debug.Log("<color=green>Evolução Concluída! Nenhum símbolo foi alterado.</color>");
//             return;
//         }

//         history.Add(nextWord);
//         CurrentStep++;
//     }

//     public void DerivePrevious()
//     {
//         if (CurrentStep > 0)
//         {
//             CurrentStep--;
//             IsFinished = false; // se recuar, reativa a evolução
//         }
//     }

//     private bool WordsAreEqual(List<Symbol> w1, List<Symbol> w2)
//     {
//         if (w1.Count != w2.Count) return false;
//         for (int i = 0; i < w1.Count; i++)
//         {
//             if (w1[i].Id != w2[i].Id || w1[i].Parameter != w2[i].Parameter)
//                 return false;
//         }
//         return true;
//     }

//     public string GetFormattedWord(List<Symbol> word)
//     {
//         return string.Join(" ", word.Select(s => s.ToString()));
//     }
// }