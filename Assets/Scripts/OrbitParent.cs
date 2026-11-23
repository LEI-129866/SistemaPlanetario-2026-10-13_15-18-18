using UnityEngine;


public class OrbitParent : MonoBehaviour
{
    //[SerializeField] private float radius = 5;
    //[SerializeField] private float orbitSpeed = 4;
    //[SerializeField] private bool randomizeBegginingValue = false;
    //[SerializeField] private bool randomizeBegginingState = false;
    //[SerializeField] private bool randomizeSpeedMultiplier = false;
    //[SerializeField] private bool useYValue = false;
    //[SerializeField] private float yValueMultiplier = 3f;

    //private float PI = Mathf.PI;
    //private float currentState = 0; // angulo atual da órbita em [0, 2π]
    //private bool switchYCos = false;
    //private float radiusOffset = 0f;
    //private float speedRandomMultiplier = 1f;
    //private GameObject parent;

    //// função chamada uma vez antes da Update() depois de ser criado o MonoBehaviour
    //void Start()
    //{
        //parent = this.transform.parent.gameObject;
        
        //// aplica variações aleatórias à posição inicial, inclinação e raio da órbita
        //if (randomizeBegginingValue) {
            //currentState = Random.Range(0f, 2 * PI);
            //float yValueOffset = Random.Range(0, radius / 4) - radius / 2;
            //yValueMultiplier += yValueOffset;
            //switchYCos = Random.Range(0, 2) == 0;
            //radiusOffset = Random.Range(0, radius / 3);
        //}

        //if (randomizeBegginingState)
            //currentState = Random.Range(0f, 2 * PI);

        //if (randomizeSpeedMultiplier)
            //speedRandomMultiplier = Random.Range(0.7f, 1.3f);

    //}

    //// função chamada uma vez por frame
    //void Update()
    //{
        //// incrementa o ângulo orbital de forma independente da taxa de frames (FPS)
        //currentState += orbitSpeed * speedRandomMultiplier * Time.deltaTime;
        //currentState = currentState % (2 * PI);

        // calcula a nova posição circular no plano XZ em torno da posição global do pai
        //float x = parent.transform.position.x + (radius + radiusOffset) * Mathf.Cos(currentState);
        //float y = transform.position.y;
        //float z = parent.transform.position.z + (radius + radiusOffset) * Mathf.Sin(currentState);

        //// adiciona oscilação vertical (eixo Y) para órbitas inclinadas
        //if (useYValue) {
            //if (switchYCos)
                //y = parent.transform.position.y + radius/2 * Mathf.Sin(currentState * yValueMultiplier);
            //else
                //y = parent.transform.position.y + radius/2 * Mathf.Cos(currentState * yValueMultiplier);
        //}

        //this.transform.position = new Vector3(x, y, z);

        ////Debug.Log(currentState);
    //}

    //// atualiza o raio da órbita em tempo real
    //public void SetRadius(float newRadius)
    //{
        //this.radius = newRadius;
    //}

    //// atualiza a velocidade angular da órbita em tempo real
    //public void SetSpeed(float newSpeed)
    //{
        //this.orbitSpeed = newSpeed;
    //}


    [SerializeField] private float radius = 5f;
    [SerializeField] private float orbitSpeed = 4f; // beta em rad/s

    private float currentAngle = 0f; // ângulo atual em radianos [0, 2*PI]
    private float initialY = 0f;

    // Inicializa a órbita a partir da posição local (t) onde a tartaruga criou o objeto
    public void InitOrbit(Vector3 localPos, float betaRadPerSec, float initialPhase = float.NaN)
    {
        this.radius = new Vector2(localPos.x, localPos.z).magnitude;
        this.initialY = localPos.y;
        this.orbitSpeed = betaRadPerSec;

        if (!float.IsNaN(initialPhase))
        {
            this.currentAngle = initialPhase;
        }
        else if (this.radius > 0.0001f)
        {
            this.currentAngle = Mathf.Atan2(localPos.z, localPos.x);
        }
        else
        {
            this.currentAngle = 0f;
        }

        UpdateLocalPosition();
    }

    void Update()
    {
        if (radius <= 0.0001f || Mathf.Approximately(orbitSpeed, 0f)) return;

        currentAngle = (currentAngle + orbitSpeed * Time.deltaTime) % (2f * Mathf.PI);
        UpdateLocalPosition();
    }

    private void UpdateLocalPosition()
    {
        float x = radius * Mathf.Cos(currentAngle);
        float z = radius * Mathf.Sin(currentAngle);
        transform.localPosition = new Vector3(x, initialY, z);
    }

    public float GetCurrentPhase() => currentAngle;
    public void SetRadius(float newRadius) => this.radius = newRadius;
    public void SetSpeed(float newSpeed) => this.orbitSpeed = newSpeed;
}