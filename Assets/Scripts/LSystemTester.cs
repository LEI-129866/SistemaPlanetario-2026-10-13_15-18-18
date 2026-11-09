using UnityEngine;
using UnityEngine.InputSystem;

public class LSystemTester : MonoBehaviour
{
    [SerializeField] private SceneBuilder sceneBuilder;
    [SerializeField] private int maxSteps = 7;
    [SerializeField] private float autoPlayInterval = 2.0f; // Tempo em segundos entre passos no Play

    private LSystemEngine engine;
    private bool isPlaying = false;
    private float timer = 0f;

    public LSystemEngine Engine => engine;
    public bool IsPlaying => isPlaying;

    void Start()
    {
        ResetSystem();
    }

    void Update()
    {
        // Gestão do Auto-Play
        if (isPlaying)
        {
            timer += Time.deltaTime;
            if (timer >= autoPlayInterval)
            {
                timer = 0f;
                if (engine != null && engine.CurrentStep < maxSteps && !engine.IsFinished)
                {
                    AdvanceStep();
                }
                else
                {
                    Pause(); // Para automaticamente ao chegar ao fim
                }
            }
        }

        // Atalhos de teclado (opcional)
        if (Keyboard.current == null) return;
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame) AdvanceStep();
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.aKey.wasPressedThisFrame) PreviousStep();
    }

    public void AdvanceStep()
    {
        if (engine != null && !engine.IsFinished && engine.CurrentStep < maxSteps)
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

    public void ResetSystem()
    {
        Pause();
        var (axiom, rules) = GetRules.GetRule("rules.txt");
        engine = new LSystemEngine(axiom, rules);
        UpdateVisuals();
    }

    public void Play() => isPlaying = true;
    public void Pause() => isPlaying = false;
    public void TogglePlayPause() => isPlaying = !isPlaying;

    public void SetAutoPlaySpeed(float interval)
    {
        autoPlayInterval = Mathf.Max(0.2f, interval);
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