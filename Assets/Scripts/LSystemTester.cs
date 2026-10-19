using UnityEngine;
using UnityEngine.InputSystem; // Importante adicionar este namespace

public class LSystemTester : MonoBehaviour
{
    [SerializeField] private SceneBuilder sceneBuilder;
    private LSystemEngine engine;

    void Start()
    {
        var (axiom, rules) = GetRules.GetRule("rules.txt");
        engine = new LSystemEngine(axiom, rules);

        UpdateVisuals();
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        // Pressiona Seta Direita ou D para avançar
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
        {
            AdvanceStep();
        }

        // Pressiona Seta Esquerda ou A para recuar
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame)
        {
            PreviousStep();
        }
    }

    public void AdvanceStep()
    {
        if (engine != null && !engine.IsFinished)
        {
            engine.DeriveNext();
            UpdateVisuals();
        }
    }

    public void PreviousStep()
    {
        if (engine != null && engine.CurrentStep > 0)
        {
            engine.DerivePrevious();
            UpdateVisuals();
        }
    }

    private void UpdateVisuals()
    {
        var word = engine.GetCurrentWord();
        Debug.Log($"<b>Passo {engine.CurrentStep}:</b> {engine.GetFormattedWord(word)}");

        if (sceneBuilder != null)
        {
            sceneBuilder.BuildScene(word);
        }
    }
}