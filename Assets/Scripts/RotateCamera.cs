using UnityEngine;
using UnityEngine.InputSystem;

public class RotateCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    //[SerializeField] private Vector3 targetOffset = Vector3.zero; // Ajusta a altura no Inspector
    [SerializeField] private float rotSpeed = 120f;
    [SerializeField] private float zoomSpeed = 15f;
    [SerializeField] private float minDist = 5f;
    [SerializeField] private float maxDist = 100f;
    

    private float yaw;
    private float pitch = 30f;
    private float dist = 20f;

    void Start()
    {
        if (!target) return;
        Vector3 focusPoint = target.position;
        dist = Mathf.Clamp(Vector3.Distance(transform.position, focusPoint), minDist, maxDist);
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
    }

    void LateUpdate()
    {
        if (!target) return;

        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed) yaw -= rotSpeed * Time.deltaTime;
            if (kb.dKey.isPressed) yaw += rotSpeed * Time.deltaTime;
            if (kb.wKey.isPressed) pitch -= rotSpeed * Time.deltaTime;
            if (kb.sKey.isPressed) pitch += rotSpeed * Time.deltaTime;

            pitch = Mathf.Clamp(pitch, -10f, 80f);

            if (kb.qKey.isPressed) dist -= zoomSpeed * Time.deltaTime;
            if (kb.eKey.isPressed) dist += zoomSpeed * Time.deltaTime;
        }

        dist = Mathf.Clamp(dist, minDist, maxDist);

        // Ponto de foco central com o offset aplicado
        Vector3 focusPoint = target.position;

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.position = focusPoint - (transform.rotation * Vector3.forward * dist);
    }
}