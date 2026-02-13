using System.Collections;
using UnityEngine;

public class AttackLearn : MonoBehaviour
{
    [Header("Attack position ")]
    [SerializeField]
    Transform attackPosition;
    [Header("Attack Radius ")]
    [SerializeField]
    float attackRadius = 2;
    [SerializeField]
    LayerMask layerMask;
    [SerializeField]
    QueryTriggerInteraction triggerInteraction;
    bool canAttack = true;
    [Header("Attack Cool Down Time")]
    [SerializeField]
    float attackCoolDownTime = 2f;
    [Header("Attack Force")]
    float attackForce = 12f;

    Animator animator;
    int reactionLayer;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryAttack();
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPosition.position, attackRadius);
    }

    #region Attack
    void TryAttack()
    {
        Debug.Log("Key Get");
        if (!canAttack) return;

         reactionLayer = animator.GetLayerIndex("Reaction Layer");
        animator.SetLayerWeight(reactionLayer, 1);
        animator.Play("HandAttack", reactionLayer);
        
        Debug.Log("Key Get and entered");
        StartCoroutine(AttackCoolDown());
        Collider[] hits = Physics.OverlapSphere(attackPosition.position, attackRadius);
        foreach (Collider hit in hits)
        {
            Debug.Log($"Hit: {hit.name}");
            if (hit.transform.root == transform.root) continue;

            if (hit.attachedRigidbody != null)
            {
                Vector3 dir = (hit.attachedRigidbody.position - transform.position).normalized;
                dir.y += 0.5f;
                hit.attachedRigidbody.AddForce(dir * attackForce, ForceMode.Impulse);
            }
        }
    }
    #endregion

    #region Attack CoolDown
    IEnumerator AttackCoolDown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCoolDownTime);
        animator.SetLayerWeight(reactionLayer, 0f);
        canAttack = true;
    }
    #endregion
}

