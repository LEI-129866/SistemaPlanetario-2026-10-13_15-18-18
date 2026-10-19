using UnityEngine;


public class OrbitParent : MonoBehaviour
{
    [SerializeField]
    private float radius = 5;

    [SerializeField]
    private float orbitSpeed = 4;

    [SerializeField]
    private bool randomizeBegginingValue = false;

    [SerializeField]
    private bool useYValue = false;

    [SerializeField]
    private float yValueMultiplier = 3f;

    private float PI = Mathf.PI;
    private GameObject parent;
    private float currentState = 0;
    private bool switchYCos = false;
    private float radiusOffset = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        parent = this.transform.parent.gameObject;
        radius = Vector3.Distance(transform.position, parent.transform.position);

        if (randomizeBegginingValue) {
            currentState = Random.Range(0f, 2 * PI);
            float yValueOffset = Random.Range(0, radius / 4) - radius / 2;
            yValueMultiplier += yValueOffset;
            switchYCos = Random.Range(0, 2) == 0;
            radiusOffset = Random.Range(0, radius / 3);
        }

    }

    // Update is called once per frame
    void Update()
    {
        currentState += orbitSpeed * Time.deltaTime;
        currentState = currentState % (2 * PI);

        float x = parent.transform.position.x + (radius + radiusOffset) * Mathf.Cos(currentState);
        float y = transform.position.y;
        float z = parent.transform.position.z + (radius + radiusOffset) * Mathf.Sin(currentState);

        if (useYValue) {
            if (switchYCos)
                y = parent.transform.position.y + radius/2 * Mathf.Sin(currentState * yValueMultiplier);
            else
                y = parent.transform.position.y + radius/2 * Mathf.Cos(currentState * yValueMultiplier);
        }

        this.transform.position = new Vector3(x, y, z);

        //Debug.Log(currentState);
    }
}
