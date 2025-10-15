using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float followSpeed = 240f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);
    [SerializeField] private PlayerController playerController;

    private void Awake()
    {
        playerController = FindObjectOfType<PlayerController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = Vector3.Lerp(transform.position, playerController.transform.position + offset, followSpeed);
        transform.position = playerController.transform.position + offset;
    }
}
