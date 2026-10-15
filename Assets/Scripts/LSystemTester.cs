using UnityEngine;

public class LSystemTester : MonoBehaviour
{
    private LSystemEngine engine;

    void Start()
    {
        var (axiom, rules) = GetRules.GetRule("rules.txt");
        engine = new LSystemEngine(axiom, rules);
        

        Debug.Log($"<b>Passo 0 (Axioma):</b> {engine.GetFormattedWord(engine.GetCurrentWord())}");

        // Simula os 8 passos de evolução
        for (int i = 1; i <= 10; i++)
        {
            engine.DeriveNext();
            Debug.Log($"<b>Passo {engine.CurrentStep}:</b> {engine.GetFormattedWord(engine.GetCurrentWord())}");
            
            if (engine.IsFinished) break;
        }
    }
}