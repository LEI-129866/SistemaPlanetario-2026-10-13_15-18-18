using UnityEngine;

public class SelfRotationScript : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 40.0f;

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
    }

    public void SetRotationSpeed(float degreesPerSecond)
    {
        rotationSpeed = degreesPerSecond;
    }


    //void Start()
    //{
    //    
    //}
    //
    //void Update()
    //{
    //    transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    //}
}
