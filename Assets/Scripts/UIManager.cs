using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Referências Principais")]
    [SerializeField] private LSystemTester lSystemTester;
    [SerializeField] private SceneBuilder sceneBuilder;

    [Header("Textos informativos")]
    [SerializeField] private TextMeshProUGUI stepText;
    [SerializeField] private TextMeshProUGUI wordText;
    [SerializeField] private TextMeshProUGUI playPauseButtonText;

    [Header("Sliders")]
    [SerializeField] private Slider orbitRadiusSlider;
    [SerializeField] private Slider speedSlider;

    void Start()
    {
        // Configuração inicial dos Sliders
        if (orbitRadiusSlider != null)
        {
            orbitRadiusSlider.minValue = 2f;
            orbitRadiusSlider.maxValue = 15f;
            orbitRadiusSlider.value = 5f;
            orbitRadiusSlider.onValueChanged.AddListener(OnOrbitRadiusChanged);
        }

        if (speedSlider != null)
        {
            speedSlider.minValue = 0.5f; // Rápido (0.5s entre passos)
            speedSlider.maxValue = 4.0f; // Lento (4s entre passos)
            speedSlider.value = 2.0f;
            speedSlider.onValueChanged.AddListener(OnSpeedChanged);
        }

        UpdateUI();
    }

    void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (lSystemTester == null || lSystemTester.Engine == null) return;

        // Atualizar Textos
        if (stepText != null)
            stepText.text = $"Passo: {lSystemTester.Engine.CurrentStep}";

        if (wordText != null)
        {
            var word = lSystemTester.Engine.GetCurrentWord();
            wordText.text = $"Palavra: {lSystemTester.Engine.GetFormattedWord(word)}";
        }

        if (playPauseButtonText != null)
            playPauseButtonText.text = lSystemTester.IsPlaying ? "Pause" : "Play";
    }

    // --- Métodos chamados pelos Botões ---

    public void OnClickNext()
    {
        lSystemTester.Pause();
        lSystemTester.AdvanceStep();
    }

    public void OnClickPrevious()
    {
        lSystemTester.Pause();
        lSystemTester.PreviousStep();
    }

    public void OnClickPlayPause()
    {
        lSystemTester.TogglePlayPause();
    }

    public void OnClickReset()
    {
        lSystemTester.ResetSystem();
    }

    // --- Métodos chamados pelos Sliders ---

    private void OnOrbitRadiusChanged(float value)
    {
        if (sceneBuilder != null)
        {
            sceneBuilder.SetOrbitRadiusStep(value);
            // Reconstrói a cena com o novo raio sem mudar de passo
            if (lSystemTester.Engine != null)
            {
                sceneBuilder.BuildScene(lSystemTester.Engine.GetCurrentWord());
            }
        }
    }

    private void OnSpeedChanged(float value)
    {
        if (lSystemTester != null)
        {
            lSystemTester.SetAutoPlaySpeed(value);
        }
    }
}