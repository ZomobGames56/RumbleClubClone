using UnityEngine;
using System.Collections;

public enum m_EnemyState
{
    Idle,
    Run,
    Search,
    Chase,
    Attack,
    Stunned
}

[RequireComponent(typeof(Rigidbody))]
public class EnemyFSM : MonoBehaviour, IDamageable
{
    [Header("Refs")]
    public Animator anim;
    public Rigidbody rb;
    public Transform player;

    [Header("Detection")]
    public float visionRange = 10f;
    public float attackRange = 2f;
    public LayerMask playerLayer;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float roamRadius = 6f;

    [Header("Attack")]
    public float punchForce = 8f;

    m_EnemyState currentState;
    Vector3 roamTarget;

    bool isPunching;
    bool isStunned;

    float searchTimer;

    int baseLayer;
    int actionLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        baseLayer = anim.GetLayerIndex("Base Layer");
        actionLayer = anim.GetLayerIndex("Action"); // optional layer

        ChangeState(m_EnemyState.Run);
    }

    void Update()
    {
        if (isStunned) return;

        switch (currentState)
        {
            case m_EnemyState.Run:
                UpdateRoam();
                break;

            case m_EnemyState.Search:
                UpdateSearch();
                break;

            case m_EnemyState.Chase:
                UpdateChase();
                break;
        }
    }

    #region ROAM
    void UpdateRoam()
    {
        MoveTo(roamTarget);

        if (Vector3.Distance(transform.position, roamTarget) < 1f)
            PickNewRoamPoint();

        if (CanSeePlayer())
            ChangeState(m_EnemyState.Chase);
    }

    void PickNewRoamPoint()
    {
        Vector3 rand = Random.insideUnitSphere * roamRadius;
        rand.y = 0;
        roamTarget = transform.position + rand;
    }
    #endregion

    #region SEARCH
    void UpdateSearch()
    {
        searchTimer -= Time.deltaTime;

        if (CanSeePlayer())
        {
            ChangeState(m_EnemyState.Chase);
            return;
        }

        if (searchTimer <= 0)
            ChangeState(m_EnemyState.Run);
    }
    #endregion

    #region CHASE
    void UpdateChase()
    {
        if (!player) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > visionRange)
        {
            ChangeState(m_EnemyState.Search);
            searchTimer = 2f;
            return;
        }

        if (dist <= attackRange)
        {
            if (!isPunching)
            {
                ChangeState(m_EnemyState.Attack);
                Vector3.Dot(transform.forward, player.position);
                StartCoroutine(PunchRoutine());
            }
            return;
        }

        MoveTo(player.position);
    }
    #endregion

    #region MOVE
    void MoveTo(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        dir.y = 0;

        rb.linearVelocity = new Vector3(dir.x * moveSpeed, rb.linearVelocity.y, dir.z * moveSpeed);

        if (dir != Vector3.zero)
            transform.forward = dir;
    }
    #endregion

    #region DETECT
    bool CanSeePlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, visionRange, playerLayer);
        if (hits.Length > 0)
        {
            player = hits[0].transform;
            return true;
        }
        return false;
    }
    #endregion

    #region ATTACK
    IEnumerator PunchRoutine()
    {
        isPunching = true;
        rb.linearVelocity = Vector3.zero;

        //anim.Play("HandAttack", baseLayer);

        yield return new WaitForSeconds(0.2f);

        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * 1f, 1.3f);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var dmg))
            {
                Vector3 dir = (hit.transform.position - transform.position).normalized;
                dir.y += 0.3f;
                dmg.TakeDamage(dir, punchForce);
            }
        }

        yield return new WaitForSeconds(0.6f);

        isPunching = false;
        ChangeState(m_EnemyState.Chase);
    }
    #endregion

    #region DAMAGE
    public void TakeDamage(Vector3 dir, float force)
    {
        if (isPunching) return; // cannot stun while punching
        if (isStunned) return;

        StartCoroutine(StunRoutine(dir, force));
    }

    IEnumerator StunRoutine(Vector3 dir, float force)
    {
        isStunned = true;
        ChangeState(m_EnemyState.Stunned);

        rb.AddForce(dir * force, ForceMode.Impulse);
        anim.CrossFade("Hit", 0.1f);

        yield return new WaitForSeconds(0.6f);

        isStunned = false;
        ChangeState(m_EnemyState.Chase);
    }
    #endregion

    #region STATE
    void ChangeState(m_EnemyState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        switch (newState)
        {
            case m_EnemyState.Run:
                PickNewRoamPoint();
                anim.CrossFade("Run", 0.15f);
                break;

            case m_EnemyState.Search:
                rb.linearVelocity = Vector3.zero;
                anim.CrossFade("Idle", 0.15f);
                break;

            case m_EnemyState.Chase:
                anim.CrossFade("Run", 0.15f);
                break;

            case m_EnemyState.Attack:
                rb.linearVelocity = Vector3.zero;
                anim.CrossFade("HandAttack", 0.15f);
                break;

            case m_EnemyState.Stunned:
                rb.linearVelocity = Vector3.zero;

                break;
        }
    }
    #endregion

   

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}