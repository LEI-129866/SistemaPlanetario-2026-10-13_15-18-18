using UnityEngine;
using UnityEngine.InputSystem;

public class LSystemTester : MonoBehaviour
{
    [SerializeField] private SceneBuilder sceneBuilder;
    [SerializeField] private int maxSteps = 7;
    [SerializeField] private float autoPlayInterval = 2.0f; // tempo em segundos entre passos no Play

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
        // gestão do Auto-Play
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
                    Pause(); // para automaticamente ao chegar ao fim
                }
            }
        }

        // atalhos de teclado
        if (Keyboard.current == null) return;
        if (Keyboard.current.rightArrowKey.wasPressedThisFrame/* || Keyboard.current.dKey.wasPressedThisFrame*/) AdvanceStep();
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame/* || Keyboard.current.aKey.wasPressedThisFrame*/) PreviousStep();
    }

    // avança uma geração no L-System e atualiza a cena 3D
    public void AdvanceStep()
    {
        if (engine != null && !engine.IsFinished && engine.CurrentStep < maxSteps)
        {
            engine.DeriveNext();
            UpdateVisuals();
        }
    }

    // recua para o passo anterior guardado no histórico do L-System
    public void PreviousStep()
    {
        if (engine != null && engine.CurrentStep > 0)
        {
            engine.DerivePrevious();
            UpdateVisuals();
        }
    }

    // reinicia a simulação para o passo 0, recarregando o axioma e as regras do ficheiro
    public void ResetSystem()
    {
        Pause();
        var (axiom, rules) = GetRules.GetRule("rules.txt");
        engine = new LSystemEngine(axiom, rules);
        UpdateVisuals();
    }

    // controlos de estado do Auto-Play (chamados pelos botões da UI)
    public void Play() => isPlaying = true;
    public void Pause() => isPlaying = false;
    public void TogglePlayPause() => isPlaying = !isPlaying;

    // ajusta o intervalo entre passos (limite mínimo de 0.2s, evita saltos repentinos)
    public void SetAutoPlaySpeed(float interval)
    {
        autoPlayInterval = Mathf.Max(0.2f, interval);
    }

    // envia a palavra atual do L-System ao SceneBuilder para reconstruir os objetos 3D
    private void UpdateVisuals()
    {
        var word = engine.GetCurrentWord();
        // Debug.Log($"<b>Passo {engine.CurrentStep}:</b> {engine.GetFormattedWord(word)}");

        if (sceneBuilder != null)
        {
            sceneBuilder.BuildScene(word);
        }
    }
}