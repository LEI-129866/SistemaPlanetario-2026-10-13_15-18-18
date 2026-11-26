using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Botões Principais")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button cameraToggleButton;

    [Header("Referências Principais")]
    [SerializeField] private LSystemTester lSystemTester;
    [SerializeField] private SceneBuilder sceneBuilder;
    [SerializeField] private RotateCamera rotateCamera;
    [SerializeField] private SpaceshipController spaceshipController;

    [Header("Textos Informativos")]
    [SerializeField] private TextMeshProUGUI stepText;
    [SerializeField] private TextMeshProUGUI wordText;
    [SerializeField] private TextMeshProUGUI playPauseButtonText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI cameraButtonText;
    [SerializeField] private TextMeshProUGUI shipTelemetryText; // opcional: mostra velocidade e força gravítica da nave

    [Header("Controlos da Fase 2 (Seeds, Fase Inicial e Velocidade)")]
    [SerializeField] private TMP_InputField lSystemSeedInput;
    [SerializeField] private TMP_InputField orbitPhaseSeedInput;
    [SerializeField] private Toggle randomInitialPhaseToggle;
    [SerializeField] private Slider speedSlider; // Multiplicador temporal: x1 a x10^4

    [Header("Controlos Legados (Opcional)")]
    [SerializeField] private Slider orbitRadiusSlider;

    void Start()
    {
        // 1. Configurar Slider de Velocidade Temporal (x1 a x10^4)
        if (speedSlider != null)
        {
            speedSlider.minValue = 1f;
            speedSlider.maxValue = 50f;
            speedSlider.value = lSystemTester != null ? lSystemTester.TimeScaleMultiplier : 1f;
            speedSlider.onValueChanged.AddListener(OnSpeedChanged);
            OnSpeedChanged(speedSlider.value);
        }

        // 2. Configurar Inputs das Sementes (L-System e Fases Orbitais)
        if (lSystemSeedInput != null && lSystemTester != null)
        {
            lSystemSeedInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            lSystemSeedInput.text = lSystemTester.LSystemSeed.ToString();
            lSystemSeedInput.onEndEdit.AddListener(OnLSystemSeedChanged);
        }

        if (orbitPhaseSeedInput != null && lSystemTester != null)
        {
            orbitPhaseSeedInput.contentType = TMP_InputField.ContentType.IntegerNumber;
            orbitPhaseSeedInput.text = lSystemTester.OrbitPhaseSeed.ToString();
            orbitPhaseSeedInput.onEndEdit.AddListener(OnOrbitPhaseSeedChanged);
        }

        // 3. Configurar Toggle da Fase Inicial (Zero vs Aleatória)
        if (randomInitialPhaseToggle != null && lSystemTester != null)
        {
            randomInitialPhaseToggle.isOn = lSystemTester.UseRandomInitialPhase;
            randomInitialPhaseToggle.onValueChanged.AddListener(OnRandomPhaseToggleChanged);
        }

        // 4. Mantido caso o slider antigo ainda exista no Canvas
        if (orbitRadiusSlider != null)
        {
            orbitRadiusSlider.minValue = 4.0f;
            orbitRadiusSlider.maxValue = 8.0f;
            orbitRadiusSlider.value = 5.0f;
            orbitRadiusSlider.onValueChanged.AddListener(OnOrbitRadiusChanged);
        }

        UpdateUI();
    }

    void Update()
    {
        UpdateUI();
    }

    // Sincroniza os textos do ecrã e o estado dos botões com o estado atual do L-System
    private void UpdateUI()
    {
        if (lSystemTester == null || lSystemTester.Engine == null) return;

        // Na Fase 2, o fim ocorre quando nenhum símbolo tem produções associadas
        bool isFinished = lSystemTester.Engine.IsFinished;

        if (stepText != null)
        {
            stepText.text = isFinished
                ? $"Passo: {lSystemTester.Engine.CurrentStep} (Fim)"
                : $"Passo: {lSystemTester.Engine.CurrentStep}";
        }

        if (nextButton != null)
        {
            nextButton.interactable = !isFinished;
        }

        if (wordText != null)
        {
            var word = lSystemTester.Engine.GetCurrentWord();
            wordText.text = $"Palavra: {lSystemTester.Engine.GetFormattedWord(word)}";
        }

        if (playPauseButtonText != null)
        {
            playPauseButtonText.text = lSystemTester.IsPlaying ? "Pause" : "Play";
        }

        if (cameraButtonText != null && rotateCamera != null)
        {
            cameraButtonText.text = rotateCamera.IsFollowingSpaceship ? "Camara: Nave" : "Camara: Sistema";
        }

        if (shipTelemetryText != null && spaceshipController != null)
        {
            float speed = spaceshipController.Velocity.magnitude;
            float gravForce = spaceshipController.LastGravityForce.magnitude;
            shipTelemetryText.text = $"Nave | Vel: {speed:F1} m/s | F_grav: {gravForce:E2} N";
        }
    }

    // --- Métodos chamados pelos Botões --- //

    public void OnClickNext()
    {
        lSystemTester.Pause();
        lSystemTester.AdvanceStep();
        ClearButtonSelection();
    }

    public void OnClickPrevious()
    {
        lSystemTester.Pause();
        lSystemTester.PreviousStep();
        ClearButtonSelection();
    }

    public void OnClickPlayPause()
    {
        lSystemTester.TogglePlayPause();
        ClearButtonSelection();
    }

    public void OnClickReset()
    {
        lSystemTester.ResetSystem();
        if (spaceshipController != null)
        {
            spaceshipController.ResetShip();
        }
        ClearButtonSelection();
    }

    public void OnClickToggleCamera()
    {
        if (rotateCamera != null)
        {
            rotateCamera.ToggleTarget();
        }
        ClearButtonSelection();
    }

    private void ClearButtonSelection()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    // --- Callbacks dos Novos Controlos da Fase 2 --- //

    private void OnLSystemSeedChanged(string text)
    {
        if (lSystemTester != null && int.TryParse(text, out int newSeed))
        {
            lSystemTester.SetLSystemSeed(newSeed);
        }
    }

    private void OnOrbitPhaseSeedChanged(string text)
    {
        if (lSystemTester != null && int.TryParse(text, out int newSeed))
        {
            lSystemTester.SetOrbitPhaseSeed(newSeed);
        }
    }

    private void OnRandomPhaseToggleChanged(bool isRandom)
    {
        if (lSystemTester != null)
        {
            lSystemTester.SetUseRandomInitialPhase(isRandom);
        }
    }

    private void OnSpeedChanged(float value)
    {
        if (lSystemTester != null)
        {
            lSystemTester.SetTimeScaleMultiplier(value);
        }

        if (speedText != null)
        {
            speedText.text = $"Velocidade: x{value:F0}";
        }
    }

    private void OnOrbitRadiusChanged(float value)
    {
        if (sceneBuilder != null)
        {
            sceneBuilder.SetOrbitRadiusStep(value);
        }
    }
}