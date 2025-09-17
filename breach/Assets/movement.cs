using UnityEngine;

public class movement : MonoBehaviour
{
    //globals
    public float MaxSpeed = 5.0f;
    public float Acceleration = 10.0f; 
    public float AirFriction = 0.1f;
    public float GroundFriction = 0.5f;


    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Space key was pressed.");
        }

        // float horizontalInput = Input.GetAxis("Horizontal");
        // float verticalInput = Input.GetAxis("Vertical");
    }
}
