using UnityEngine;

public class RotateCamera : MonoBehaviour
{

    [SerializeField] private GameObject systemOrigin;
    void Update()
    {
        transform.RotateAround(systemOrigin.transform.position, Vector3.up, 7f * Time.deltaTime);
        transform.LookAt(systemOrigin.transform.position);

    }
}
