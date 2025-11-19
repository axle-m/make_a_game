using UnityEditor.Callbacks;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] protected float Health;
    [SerializeField] protected int Damage;
    [SerializeField] protected float MoveSpeed;
    [SerializeField] protected PlayerController player;
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilSpeed;
    [SerializeField] protected bool isRecoiling = false;
    protected float speedPreKnockback;

    protected float recoilTimer = 0f;
    protected Rigidbody2D rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = FindAnyObjectByType<PlayerController>();
    }

    protected virtual void Update()
    {
        if (Health <= 0)
        {
            Destroy(gameObject);
        }

        if (isRecoiling)
        {
            recoilTimer += Time.deltaTime;
            if (recoilTimer >= recoilLength)
            {
                isRecoiling = false;
                recoilTimer = 0f;
                rb.linearVelocityX = speedPreKnockback;
            }
        }
    }

    public virtual void EnemyHit(float _damage, Vector2 _hitDirection, float _hitForce)
    {
        Health -= _damage;
        if (isRecoiling) return;
        isRecoiling = true;
        rb.AddForce(_hitForce * recoilSpeed * -_hitDirection);
    }
}
