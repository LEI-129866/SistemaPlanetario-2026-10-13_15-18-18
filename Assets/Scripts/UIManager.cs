using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UIManager : MonoBehaviour
{

    [SerializeField] private Button nextButton;

    [Header("Referências Principais")]
    [SerializeField] private LSystemTester lSystemTester;
    [SerializeField] private SceneBuilder sceneBuilder;

    [Header("Textos informativos")]
    [SerializeField] private TextMeshProUGUI stepText;
    [SerializeField] private TextMeshProUGUI wordText;
    [SerializeField] private TextMeshProUGUI playPauseButtonText;
    [SerializeField] private TextMeshProUGUI orbitRadiusText;

    [Header("Sliders")]
    [SerializeField] private Slider orbitRadiusSlider;
    [SerializeField] private Slider speedSlider;
    

    void Start()
    {
        // Configuração inicial dos Sliders
        if (orbitRadiusSlider != null)
        {
            orbitRadiusSlider.minValue = 3.5f; // Mínimo seguro para não entrar dentro da estrela
            orbitRadiusSlider.maxValue = 8.0f; // Máximo visível na câmara até ao 3º planeta (3 * 8 = 24)
            orbitRadiusSlider.value = 5.0f;
            orbitRadiusSlider.onValueChanged.AddListener(OnOrbitRadiusChanged);

            // atualiza o texto logo no início com o valor padrão (5.0)
            if (orbitRadiusText != null)
                orbitRadiusText.text = $"raio órbita (r): {orbitRadiusSlider.value:F1}";
        }

        if (speedSlider != null)
        {
            speedSlider.minValue = 0.5f; // Rápido (0.5s entre passos)
            speedSlider.maxValue = 8.0f; // Lento (4s entre passos)
            speedSlider.value = 4.0f;
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

        bool isFinished = lSystemTester.Engine.IsFinished || lSystemTester.Engine.CurrentStep >= 7;

        // atualiza o texto do passo e indica claramente quando terminou
        if (stepText != null)
        {
            if (isFinished)
                stepText.text = $"Passo: {lSystemTester.Engine.CurrentStep} (Fim)";
            else
                stepText.text = $"Passo: {lSystemTester.Engine.CurrentStep}";
        }

        // desativa o botão de avanço no fim da evolução
        if (nextButton != null)
        {
            nextButton.interactable = !isFinished;
        }

        // atualiza a palavra e o texto do botão Play/Pause
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
        // aplica o valor de r definido no slider antes de reiniciar
        if (sceneBuilder != null && orbitRadiusSlider != null)
        {
            sceneBuilder.SetOrbitRadiusStep(orbitRadiusSlider.value);
        }
        lSystemTester.ResetSystem();
        ClearButtonSelection();
    }

    // --- Métodos chamados pelos Sliders ---
    private void ClearButtonSelection()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private void OnOrbitRadiusChanged(float value)
    {
        // atualiza o texto com o novo valor de r (com 1 casa decimal)
        if (orbitRadiusText != null)
            orbitRadiusText.text = $"raio órbita (r): {value:F1}";

        if (sceneBuilder != null)
        {
            sceneBuilder.SetOrbitRadiusStep(value);
            // reconstrói a cena com o novo raio sem mudar de passo
            //if (lSystemTester.Engine != null)
            //{
                //sceneBuilder.BuildScene(lSystemTester.Engine.GetCurrentWord());
                
            //}
        }
    }

    private void OnSpeedChanged(float value)
    {
        // atualiza a velocidade das órbitas 3D em tempo real
        if (sceneBuilder != null)
        {
            sceneBuilder.SetOrbitSpeed(value);
        }

        // atualiza a velocidade do Auto-Play (quanto maior o valor, menor o tempo de espera)
        if (lSystemTester != null)
        {
            float interval = 4.0f / value; //4.0 dá 1s por passo; a 8.0 dá 0.5s; a 0.5 dá 8s
            lSystemTester.SetAutoPlaySpeed(interval);
        }
    }    
}