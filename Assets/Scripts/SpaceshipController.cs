using UnityEngine;
using UnityEngine.InputSystem;

// Controla o voo da nave espacial e calcula a atração gravitacional newtoniana
// exercida por todos os corpos celestes (CelestialBody) da cena
public class SpaceshipController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private LSystemTester lSystemTester;

    [Header("Configuração de Voo e Posição Inicial")]
    [SerializeField] private Vector3 initialPosition = new Vector3(0f, 10f, -40f);
    [SerializeField] private float thrustForce = 50f;       // força dos propulsores (N)
    [SerializeField] private float rotationSpeed = 90f;     // velocidade de rotação (graus/s)
    [SerializeField] private float minGravityDistance = 2f; // evita divisões por zero perto do centro de um astro

    private Vector3 velocity = Vector3.zero;
    private Vector3 lastGravityForce = Vector3.zero;

    public Vector3 Velocity => velocity;
    public Vector3 LastGravityForce => lastGravityForce;

    void Start()
    {
        if (lSystemTester == null)
            lSystemTester = FindFirstObjectByType<LSystemTester>();

        ResetShip();
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        // tecla R repõe a nave na posição inicial com velocidade nula
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            ResetShip();
        }

        HandleRotation();
    }

    void FixedUpdate()
    {
        float G = 6.6743e-11f;
        float shipMass = 1000f;

        if (lSystemTester != null && lSystemTester.Config != null)
        {
            G = lSystemTester.Config.gravitationalConstant;
            shipMass = Mathf.Max(0.001f, lSystemTester.Config.shipMass);
        }

        // força gravitacional resultante de todos os astros com massa na cena
        Vector3 totalGravityForce = ComputeGravitationalForce(G, shipMass);
        lastGravityForce = totalGravityForce;

        // força dos propulsores controlada pelo utilizador
        Vector3 thrust = ComputeThrustForce();

        // segunda Lei de Newton (F = m * a) e integração de Euler
        Vector3 acceleration = (totalGravityForce + thrust) / shipMass;
        velocity += acceleration * Time.fixedDeltaTime;
        transform.position += velocity * Time.fixedDeltaTime;
    }

    private Vector3 ComputeGravitationalForce(float G, float shipMass)
    {
        Vector3 netForce = Vector3.zero;
        CelestialBody[] bodies = FindObjectsByType<CelestialBody>(FindObjectsSortMode.None);

        foreach (CelestialBody body in bodies)
        {
            if (body == null || body.Mass <= 0f) continue;

            Vector3 direction = body.transform.position - transform.position;
            float distance = direction.magnitude;

            if (distance < 0.0001f) continue;

            // limita a distância mínima para evitar aceleração infinita ao atravessar um astro
            float clampedDist = Mathf.Max(distance, minGravityDistance);
            float forceMagnitude = G * (body.Mass * shipMass) / (clampedDist * clampedDist);

            netForce += direction.normalized * forceMagnitude;
        }

        return netForce;
    }

    private Vector3 ComputeThrustForce()
    {
        if (Keyboard.current == null) return Vector3.zero;

        float forwardInput = 0f;
        if (Keyboard.current.wKey.isPressed) forwardInput += 1f;
        if (Keyboard.current.sKey.isPressed) forwardInput -= 1f;

        float boost = Keyboard.current.leftShiftKey.isPressed ? 3f : 1f;

        return transform.forward * (forwardInput * thrustForce * boost);
    }

    private void HandleRotation()
    {
        float yaw = 0f;   // A / D (esquerda / direita)
        float pitch = 0f; // Setas Cima / Baixo (nariz cima / baixo)
        float roll = 0f;  // Q / E (zoom)

        if (Keyboard.current.dKey.isPressed) yaw += 1f;
        if (Keyboard.current.aKey.isPressed) yaw -= 1f;

        if (Keyboard.current.upArrowKey.isPressed) pitch -= 1f;
        if (Keyboard.current.downArrowKey.isPressed) pitch += 1f;

        if (Keyboard.current.qKey.isPressed) roll += 1f;
        if (Keyboard.current.eKey.isPressed) roll -= 1f;

        Vector3 eulerDelta = new Vector3(pitch, yaw, roll) * (rotationSpeed * Time.deltaTime);
        transform.Rotate(eulerDelta, Space.Self);
    }

    public void ResetShip()
    {
        transform.position = initialPosition;
        transform.rotation = Quaternion.identity;
        velocity = Vector3.zero;
        lastGravityForce = Vector3.zero;
    }
}