using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float Health = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Health <= 0)
        {
            Destroy(gameObject);
        }
    }
    
    public void EnemyHit(float _damage)
    {
        Health -= _damage;
    }
}
