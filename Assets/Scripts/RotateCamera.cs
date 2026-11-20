using UnityEngine;
using UnityEngine.InputSystem;

public class RotateCamera : MonoBehaviour
{
    [SerializeField] private Transform target; // alvo para onde aponta a câmera
    [SerializeField] private float rotSpeed = 120f;
    [SerializeField] private float zoomSpeed = 15f;
    [SerializeField] private float minDist = 5f;
    [SerializeField] private float maxDist = 100f;
    
    private float yaw; // ângulo horizontal (em torno do eixo dos Y)
    private float pitch = 30f; // ângulo vertical (em torno do eixo dos X)
    private float dist = 20f; // distância atual da câmera ao alvo

    void Start()
    {   
        // inicializa a distância e os ângulos com base na posição inicial da câmara na scene
        if (!target) return;
        Vector3 focusPoint = target.position;
        dist = Mathf.Clamp(Vector3.Distance(transform.position, focusPoint), minDist, maxDist);
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    // garante que a câmara se move apenas depois de todos os corpos atualizarem a posição
    void LateUpdate()
    {
        if (!target) return;

        Keyboard kb = Keyboard.current;
        if (kb != null)
        {   
            // Rotação horizontal (A/D) e vertical (W/S)
            if (kb.aKey.isPressed) yaw += rotSpeed * Time.deltaTime;
            if (kb.dKey.isPressed) yaw -= rotSpeed * Time.deltaTime;
            if (kb.wKey.isPressed) pitch += rotSpeed * Time.deltaTime;
            if (kb.sKey.isPressed) pitch -= rotSpeed * Time.deltaTime;

            // limita o ângulo vertical para evitar que a câmara se vire ao contrário
            pitch = Mathf.Clamp(pitch, -10f, 80f);

            // zoom in (E) e zoom out (Q)
            if (kb.qKey.isPressed) dist += zoomSpeed * Time.deltaTime;
            if (kb.eKey.isPressed) dist -= zoomSpeed * Time.deltaTime;
        }

        // garante que a distância respeita os limites mínimo e máximo definidos
        dist = Mathf.Clamp(dist, minDist, maxDist);

        // calcula a rotação final e posiciona a câmara à distância 'dist' atrás do ponto de foco
        Vector3 focusPoint = target.position;
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.position = focusPoint - (transform.rotation * Vector3.forward * dist);
    }
}