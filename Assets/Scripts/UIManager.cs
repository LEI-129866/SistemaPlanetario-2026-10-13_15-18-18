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
        // Configuração inicial dos limites, valores por defeito e listeners dos Sliders
        if (orbitRadiusSlider != null)
        {
            orbitRadiusSlider.minValue = 4.0f; // Mínimo seguro para não entrar dentro da estrela
            orbitRadiusSlider.maxValue = 8.0f; 
            orbitRadiusSlider.value = 5.0f;
            orbitRadiusSlider.onValueChanged.AddListener(OnOrbitRadiusChanged);
        }

        if (speedSlider != null)
        {
            speedSlider.minValue = 0.5f; 
            speedSlider.maxValue = 8.0f; 
            speedSlider.value = 4.0f;
            speedSlider.onValueChanged.AddListener(OnSpeedChanged);
        }

        UpdateUI();
    }

    void Update()
    {
        UpdateUI();
    }

    // sincroniza os textos do ecrã e o estado dos botões com o estado atual do L-System
    private void UpdateUI()
    {
        if (lSystemTester == null || lSystemTester.Engine == null) return;

        bool isFinished = lSystemTester.Engine.IsFinished || lSystemTester.Engine.CurrentStep >= 7;

        // atualiza o número do passo atual e indica quando a evolução terminou
        if (stepText != null)
        {
            if (isFinished)
                stepText.text = $"Passo: {lSystemTester.Engine.CurrentStep} (Fim)";
            else
                stepText.text = $"Passo: {lSystemTester.Engine.CurrentStep}";
        }

        // desativa o botão de avanço ao atingir o fim da evolução
        if (nextButton != null)
        {
            nextButton.interactable = !isFinished;
        }

        // atualiza a palavra derivada do L-System
        if (wordText != null)
        {
            var word = lSystemTester.Engine.GetCurrentWord();
            wordText.text = $"Palavra: {lSystemTester.Engine.GetFormattedWord(word)}";
        }

        if (playPauseButtonText != null)
            playPauseButtonText.text = lSystemTester.IsPlaying ? "Pause" : "Play";
    }

    // --- Métodos chamados pelos Botões --- //

    // pausa o Auto-Play e avança manualmente um passo
    public void OnClickNext()
    {
        lSystemTester.Pause();
        lSystemTester.AdvanceStep();
        ClearButtonSelection();
    }

    // pausa o Auto-Play e recua manualmente um passo
    public void OnClickPrevious()
    {
        lSystemTester.Pause();
        lSystemTester.PreviousStep();
        ClearButtonSelection();
    }

    // alterna entre execução automática e pausa
    public void OnClickPlayPause()
    {
        lSystemTester.TogglePlayPause();
        ClearButtonSelection();
    }

    // reinicia o L-System para o passo 0 mantendo o raio orbital definido no slider
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

    // remove o foco do botão clicado
    private void ClearButtonSelection()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    // --- Métodos chamados pelos Sliders --- //

    // atualiza o espaçamento radial (r) das órbitas na cena 3D
    private void OnOrbitRadiusChanged(float value)
    {
        if (sceneBuilder != null)
        {
            sceneBuilder.SetOrbitRadiusStep(value);
        }
    }

    // ajusta em simultâneo a velocidade orbital 3D e o ritmo de avanço do Auto-Play
    private void OnSpeedChanged(float value)
    {
        // ajusta velocidade das órbitas 3D em tempo real
        if (sceneBuilder != null)
        {
            sceneBuilder.SetOrbitSpeed(value);
        }

        // quanto maior o valor da velocidade, menor o tempo de espera
        if (lSystemTester != null)
        {
            float interval = 4.0f / value; // 4.0 -> 1s/passo; 8.0 -> 0.5s/passo; 0.5 -> 8s/passo
            lSystemTester.SetAutoPlaySpeed(interval);
        }
    }    
}