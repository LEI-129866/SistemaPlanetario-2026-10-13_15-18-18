using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class RotateCamera : MonoBehaviour
{
    [Header("Alvos da Câmara")]
    [SerializeField] private Transform systemTarget;    // centro do sistema planetário
    [SerializeField] private Transform spaceshipTarget; // nave espacial (Spaceship)
    [SerializeField] private bool followSpaceship = false;

    [Header("Velocidades e Limites")]
    [SerializeField] private float rotSpeed = 120f;
    [SerializeField] private float mouseSensitivity = 0.25f;
    [SerializeField] private float zoomSpeed = 25f;
    [SerializeField] private float minDist = 3f;
    [SerializeField] private float maxDist = 500f;

    private float yaw;          // ângulo horizontal (em torno do eixo Y)
    private float pitch = 30f;  // ângulo vertical (em torno do eixo X)
    private float dist = 30f;   // distância atual da câmara ao alvo

    public bool IsFollowingSpaceship => followSpaceship;

    void Start()
    {
        Transform activeTarget = GetActiveTarget();
        if (!activeTarget) return;

        dist = Mathf.Clamp(Vector3.Distance(transform.position, activeTarget.position), minDist, maxDist);
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    void LateUpdate()
    {
        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;

        // Ignora atalhos de teclado se o utilizador estiver a escrever numa caixa de texto da UI
        bool isTypingInUI = EventSystem.current != null &&
                            EventSystem.current.currentSelectedGameObject != null &&
                            EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null;

        // Tecla C alterna entre vista do Sistema Planetário e vista da Nave Espacial
        if (kb != null && !isTypingInUI && kb.cKey.wasPressedThisFrame)
        {
            ToggleTarget();
        }

        Transform activeTarget = GetActiveTarget();
        if (!activeTarget) return;

        // 1. Controlo com Rato (funciona em ambos os modos: botão direito para rodar, scroll para zoom)
        if (mouse != null)
        {
            if (mouse.rightButton.isPressed)
            {
                Vector2 delta = mouse.delta.ReadValue();
                yaw += delta.x * mouseSensitivity;
                pitch -= delta.y * mouseSensitivity;
            }

            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                dist -= scroll * 0.05f * zoomSpeed;
            }
        }

        // 2. Controlo com Teclado (apenas no modo Sistema para não conflituar com o voo da nave)
        if (kb != null && !followSpaceship && !isTypingInUI)
        {
            if (kb.aKey.isPressed) yaw += rotSpeed * Time.deltaTime;
            if (kb.dKey.isPressed) yaw -= rotSpeed * Time.deltaTime;
            if (kb.wKey.isPressed) pitch += rotSpeed * Time.deltaTime;
            if (kb.sKey.isPressed) pitch -= rotSpeed * Time.deltaTime;

            if (kb.qKey.isPressed) dist += zoomSpeed * Time.deltaTime;
            if (kb.eKey.isPressed) dist -= zoomSpeed * Time.deltaTime;
        }

        pitch = Mathf.Clamp(pitch, -80f, 85f);
        dist = Mathf.Clamp(dist, minDist, maxDist);

        // Posiciona e orienta a câmara em torno do alvo ativo
        Vector3 focusPoint = activeTarget.position;
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.position = focusPoint - (transform.rotation * Vector3.forward * dist);
    }

    public void ToggleTarget()
    {
        if (spaceshipTarget == null) return;
        followSpaceship = !followSpaceship;
    }

    public void SetFollowSpaceship(bool follow)
    {
        followSpaceship = follow && spaceshipTarget != null;
    }

    private Transform GetActiveTarget()
    {
        if (followSpaceship && spaceshipTarget != null)
            return spaceshipTarget;

        return systemTarget;
    }
}