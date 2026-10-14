using System;
using UnityEngine;

public class OrbitParent : MonoBehaviour
{
    [SerializeField]
    private float radius = 5;

    [SerializeField]
    private float orbitSpeed = 4;

    private float PI = Mathf.PI;
    private GameObject parent;
    private float currentState = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        parent = this.transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        currentState += orbitSpeed * Time.deltaTime;
        currentState = currentState % (2 * PI);

        float x = parent.transform.position.x + radius * Mathf.Cos(currentState);
        float z = parent.transform.position.z + radius * Mathf.Sin(currentState);

        this.transform.position = new Vector3(x, transform.position.y, z);

        //Debug.Log(currentState);
    }
}
