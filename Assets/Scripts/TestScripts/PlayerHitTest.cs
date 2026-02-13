using UnityEngine;

public class PlayerHitTest : MonoBehaviour
{
    [SerializeField]
    float force = 8;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            if (collision.gameObject.TryGetComponent<IDamageable>(out IDamageable dmg))
            {
                Vector3 dir = (collision.transform.position - transform.position).normalized;
                dir.y = 0.5f;
                dmg.TakeDamage(dir, force);
                
            }
        }
    }
}
