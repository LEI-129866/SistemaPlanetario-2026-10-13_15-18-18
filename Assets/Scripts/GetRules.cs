using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class GetRules
{
    // lê o ficheiro em Assets/Scripts
    // devolve o axioma inicial e a lista de regras de produção
    public static (List<Symbol> axiom, List<ProductionRule> rules) GetRule(string fileName)
    {
        string filePath = $"{Application.dataPath}/Scripts/{fileName}";

        List<Symbol> axiom = new List<Symbol>();
        List<ProductionRule> rules = new List<ProductionRule>();

        // verifica se o ficheiro existe antes de tentar ler
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Ficheiro de regras não encontrado em: {filePath}");
            return (axiom, rules);
        }

        // processa o ficheiro linha a linha, ignorando linhas vazias
        string[] lines = File.ReadAllLines(filePath);
        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            // identifica e extrai o axioma inicial
            if (trimmed.StartsWith("AXIOM:"))
            {
                string axiomStr = trimmed.Substring(6).Trim();
                axiom = ParseWord(axiomStr);
            }

            // lê regras no formato "esquerda: direita (ex: "d3: d2 M")"
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

    // converte uma string de texto numa lista ordenada de objetos Symbol
    public static List<Symbol> ParseWord(string wordStr)
    {
        List<Symbol> word = new List<Symbol>();

        // regex para capturar:
        // 1. letra seguida de número (ex: d3, d2, d1)
        // 2. símbolos individuais (ex: P, 1, [, ], n, p, e, D, m, N)
        string pattern = @"[a-zA-Z]\d+|[a-zA-Z0-9\[\]]";
        MatchCollection matches = Regex.Matches(wordStr, pattern);

        foreach (Match match in matches)
        {
            word.Add(ParseSingleSymbol(match.Value));
        }

        return word;
    }

    // transforma um token individual num Symbol (com ou sem parâmetro numérico)
    private static Symbol ParseSingleSymbol(string token)
    {
        Match match = Regex.Match(token, @"^([a-zA-Z]+)(\d+)$");
        if (match.Success){
            string id = match.Groups[1].Value;
            int param = int.Parse(match.Groups[2].Value);
            return new Symbol(id, param);
        }
        return new Symbol(token);
    }
}