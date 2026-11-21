using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class GetRules
{
    // símbolos que possuem órbita - exigem uma linha extra (alpha, beta) no ficheiro de configuração
    private static readonly HashSet<string> OrbitingSymbols = new HashSet<string> { "Pc", "Pb", "Pa", "La" };

    // lê o ficheiro de configuração e devolve um objeto SimulationConfig completo
    public static SimulationConfig LoadConfig(string fileName)
    {
        SimulationConfig config = new SimulationConfig();
        string filePath = $"{Application.dataPath}/Scripts/{fileName}";

        // verifica se o ficheiro existe antes de tentar ler
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Ficheiro de configuração não encontrado em: {filePath}");
            return config;
        }

        // lê todas as linhas não vazias do ficheiro
        List<string> lines = new List<string>();
        foreach (string rawLine in File.ReadAllLines(filePath))
        {
            string trimmed = rawLine.Trim();
            if (!string.IsNullOrEmpty(trimmed))
            {
                lines.Add(trimmed);
            }
        }

        if (lines.Count < 8)
        {
            Debug.LogError($"Ficheiro de configuração inválido (menos de 8 linhas): {filePath}");
            return config;
        }

        // Linha 1: deslocamento (c) e ângulo (a) da tartaruga
        string[] l1 = SplitTokens(lines[0]);
        config.stepDistance = ParseFloat(l1[0]);
        config.rotationAngle = ParseFloat(l1[1]);

        // Linha 2: massas me, mp, ml (inteiros)
        string[] l2 = SplitTokens(lines[1]);
        config.starMass = int.Parse(l2[0], CultureInfo.InvariantCulture);
        config.planetMass = int.Parse(l2[1], CultureInfo.InvariantCulture);
        config.moonMass = int.Parse(l2[2], CultureInfo.InvariantCulture);

        // Linha 3: massa da nave espacial
        config.spaceshipMass = ParseFloat(SplitTokens(lines[2])[0]);

        // Linha 4: raios re, rp, rl
        string[] l4 = SplitTokens(lines[3]);
        config.starRadius = ParseFloat(l4[0]);
        config.planetRadius = ParseFloat(l4[1]);
        config.moonRadius = ParseFloat(l4[2]);

        // Linha 5: larguras radiais dg (G1) e dr (R1)
        string[] l5 = SplitTokens(lines[4]);
        config.gasRingWidth = ParseFloat(l5[0]);
        config.rockRingWidth = ParseFloat(l5[1]);

        // Linha 6: constante da gravitação universal G
        config.gravitationalConstant = ParseFloat(SplitTokens(lines[5])[0]);

        // Linha 7: axioma do L-System (ex: Nv)
        config.axiom = ParseWordPhase2(lines[6]);

        // Linha 8: número total de produções
        int numProductions = int.Parse(SplitTokens(lines[7])[0], CultureInfo.InvariantCulture);

        // Linhas seguintes: leitura das produções e respetivos pares (alpha, beta)
        int currentLine = 8;
        for (int p = 0; p < numProductions && currentLine < lines.Count; p++)
        {
            string prodLine = lines[currentLine++];
            string[] tokens = SplitTokens(prodLine);

            // formato esperado: <peso> <Cabeça> -> <Símbolo1> <Símbolo2> ...
            if (tokens.Length < 4 || tokens[2] != "->")
            {
                Debug.LogWarning($"Formato inesperado na produção (linha {currentLine}): {prodLine}");
                continue;
            }

            int weight = int.Parse(tokens[0], CultureInfo.InvariantCulture);
            string head = tokens[1];

            List<Symbol> successor = new List<Symbol>();
            int orbitingCount = 0;

            for (int i = 3; i < tokens.Length; i++)
            {
                string symId = tokens[i];
                successor.Add(new Symbol(symId));

                if (OrbitingSymbols.Contains(symId))
                {
                    orbitingCount++;
                }
            }

            StochasticRule rule = new StochasticRule(weight, head, successor);

            // uma linha com (alpha, beta) por cada símbolo com órbita no corpo da produção
            for (int k = 0; k < orbitingCount && currentLine < lines.Count; k++)
            {
                string[] orbitTokens = SplitTokens(lines[currentLine++]);
                if (orbitTokens.Length >= 2)
                {
                    float alpha = ParseFloat(orbitTokens[0]);
                    float beta = ParseFloat(orbitTokens[1]);
                    rule.orbitParameters.Add(new OrbitParams(alpha, beta));
                }
            }

            config.rules.Add(rule);
        }

        return config;
    }

    // converte uma sequência de símbolos de 2 carateres separados por espaço numa lista de Symbol
    public static List<Symbol> ParseWordPhase2(string wordStr)
    {
        List<Symbol> word = new List<Symbol>();
        foreach (string token in SplitTokens(wordStr))
        {
            word.Add(new Symbol(token));
        }
        return word;
    }

    private static string[] SplitTokens(string line)
    {
        return line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
    }

    private static float ParseFloat(string value)
    {
        return float.Parse(value, CultureInfo.InvariantCulture);
    }
}

// -- Código Fase 1 -- //

// public class GetRules
// {
//     // lê o ficheiro em Assets/Scripts
//     // devolve o axioma inicial e a lista de regras de produção
//     public static (List<Symbol> axiom, List<ProductionRule> rules) GetRule(string fileName)
//     {
//         string filePath = $"{Application.dataPath}/Scripts/{fileName}";

//         List<Symbol> axiom = new List<Symbol>();
//         List<ProductionRule> rules = new List<ProductionRule>();

//         // verifica se o ficheiro existe antes de tentar ler
//         if (!File.Exists(filePath))
//         {
//             Debug.LogError($"Ficheiro de regras não encontrado em: {filePath}");
//             return (axiom, rules);
//         }

//         // processa o ficheiro linha a linha, ignorando linhas vazias
//         string[] lines = File.ReadAllLines(filePath);
//         foreach (string line in lines)
//         {
//             string trimmed = line.Trim();
//             if (string.IsNullOrEmpty(trimmed)) continue;

//             // identifica e extrai o axioma inicial
//             if (trimmed.StartsWith("AXIOM:"))
//             {
//                 string axiomStr = trimmed.Substring(6).Trim();
//                 axiom = ParseWord(axiomStr);
//             }

//             // lê regras no formato "esquerda: direita (ex: "d3: d2 M")"
//             else if (trimmed.Contains(":"))
//             {
//                 string[] parts = trimmed.Split(new[] { ':' }, 2);

//                 string left = parts[0].Trim();
//                 string right = parts[1].Trim();

//                 Symbol predSymbol = ParseSingleSymbol(left);
//                 List<Symbol> succSymbols = ParseWord(right);

//                 rules.Add(new ProductionRule(
//                     predSymbol.Id, 
//                     predSymbol.Parameter, 
//                     succSymbols
//                 ));
//             }
//         }

//         return (axiom, rules);
//     }

//     // converte uma string de texto numa lista ordenada de objetos Symbol
//     public static List<Symbol> ParseWord(string wordStr)
//     {
//         List<Symbol> word = new List<Symbol>();

//         // regex para capturar:
//         // 1. letra seguida de número (ex: d3, d2, d1)
//         // 2. símbolos individuais (ex: P, 1, [, ], n, p, e, D, m, N)
//         string pattern = @"[a-zA-Z]\d+|[a-zA-Z0-9\[\]]";
//         MatchCollection matches = Regex.Matches(wordStr, pattern);

//         foreach (Match match in matches)
//         {
//             word.Add(ParseSingleSymbol(match.Value));
//         }

//         return word;
//     }

//     // transforma um token individual num Symbol (com ou sem parâmetro numérico)
//     private static Symbol ParseSingleSymbol(string token)
//     {
//         Match match = Regex.Match(token, @"^([a-zA-Z]+)(\d+)$");
//         if (match.Success){
//             string id = match.Groups[1].Value;
//             int param = int.Parse(match.Groups[2].Value);
//             return new Symbol(id, param);
//         }
//         return new Symbol(token);
//     }
// }