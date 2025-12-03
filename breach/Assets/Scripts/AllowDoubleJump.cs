using UnityEngine;

public class AllowDoubleJump : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.allowDoubleJump = true;
        }

    }

}
