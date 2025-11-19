using UnityEngine;
using UnityEngine.AI;

public class GroundEnemy : Enemy
{

    protected override void Awake()
    {
        base.Awake();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb.gravityScale = 12f;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (isRecoiling) return;

        MoveTowardsPlayer();
    }

    public void MoveTowardsPlayer()
    {
        Vector2 dir = new Vector2(player.transform.position.x - transform.position.x, 0).normalized;
        if (!player.GetComponent<PlayerStateList>().Respawning && Physics2D.Raycast(transform.position, dir, 10f, LayerMask.GetMask("Player")))
        {
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, MoveSpeed * Time.deltaTime);
        }
    }

    public override void EnemyHit(float _damage, Vector2 _hitDirection, float _hitForce)
    {
        base.EnemyHit(_damage, _hitDirection, _hitForce);
    }
}
