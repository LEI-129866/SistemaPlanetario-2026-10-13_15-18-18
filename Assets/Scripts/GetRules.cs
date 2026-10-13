using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class GetRules
{
    public static (List<Symbol> axiom, List<ProductionRule> rules) GetRule(string fileName)
    {
        string filePath = $"{Application.dataPath}/Scripts/{fileName}";

        List<Symbol> axiom = new List<Symbol>();
        List<ProductionRule> rules = new List<ProductionRule>();

        if (!File.Exists(filePath))
        {
            Debug.LogError($"Ficheiro de regras não encontrado em: {filePath}");
            return (axiom, rules);
        }

        string[] lines = File.ReadAllLines(filePath);
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            if (trimmed.StartsWith("AXIOM:"))
            {
                string axiomStr = trimmed.Substring(6).Trim();
                axiom = ParseWord(axiomStr);
            }
            // Lê regras no formato "esquerda: direita"
            else if (trimmed.Contains(":"))
            {
                string[] parts = trimmed.Split(new[] { ':' }, 2);

                string left = parts[0].Trim();
                string right = parts[1].Trim();

                Symbol predSymbol = ParseSingleSymbol(left);
                List<Symbol> succSymbols = ParseWord(right);

                rules.Add(new ProductionRule(
                    predSymbol.Id, 
                    predSymbol.Parameter, 
                    succSymbols
                ));
            }
        }

        return (axiom, rules);
    }

    public static List<Symbol> ParseWord(string wordStr)
    {
        List<Symbol> word = new List<Symbol>();

        // Regex para capturar:
        // 1. Letra seguida de número (ex: d3, d2, d1)
        // 2. Ou símbolos individuais (ex: P, 1, [, ], n, p, e, D, m, N)
        string pattern = @"[a-zA-Z]\d+|[a-zA-Z0-9\[\]]";
        MatchCollection matches = Regex.Matches(wordStr, pattern);

        foreach (Match match in matches)
        {
            word.Add(ParseSingleSymbol(match.Value));
        }

        return word;
    }

    private static Symbol ParseSingleSymbol(string token)
    {
        // Se o token for do tipo "d3", separa o ID ('d') do parâmetro (3)
        if (token.Length > 1 && char.IsLetter(token[0]) && char.IsDigit(token[token.Length - 1]))
        {
            string id = token.Substring(0, token.Length - 1);
            int param = int.Parse(token[token.Length - 1].ToString());
            return new Symbol(id, param);
        }

        return new Symbol(token);
    }
}