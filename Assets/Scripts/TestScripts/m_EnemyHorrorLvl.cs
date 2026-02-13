using System.Collections;
using UnityEngine;

public enum BasicState
{
    None = 0,
    Roam,
    Chase,
    Search,
    Attack,
    Stunned
}

[RequireComponent(typeof(Rigidbody))]
public class m_EnemyHorrorLvl : MonoBehaviour,IDamageable
{
    Rigidbody rb;
    Animator anim;
    BasicState currentBasicState = BasicState.Roam;
    EnemyState currentAnimationState = EnemyState.Idle;
    Vector3 roamTarget;
    public float roamRadius = 6f;

    [Header("Detection")]
    public float visionRange = 10f;
    public float attackRange = 2f;
    public LayerMask targetLayer;
    public Transform target;
    public float moveSpeed = 3f;

    bool isPunching;
    bool isStunned;

    float searchTimer;

    [SerializeField]
    float punchForce = 8f, attackRadius=0.5f;
    
    [SerializeField]
    Transform attackPosition;
    [SerializeField]
    GameObject effect;

    const int Max_Result = 5;
    readonly Collider[]hits = new Collider[Max_Result];

   
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }
    private void Update()
    {
        if (isStunned) return;

        switch (currentBasicState)
        {
            case BasicState.Roam:
                UpdateRoam();
                EnemyAnimationState(EnemyState.Run);
                break;

            case BasicState.Search:
                UpdateSearch();
                EnemyAnimationState(EnemyState.Idle);
                break;

            case BasicState.Chase:
                UpdateChase();
                EnemyAnimationState(EnemyState.Run);
                break;
            case BasicState.Attack:
                EnemyAnimationState(EnemyState.HandAttack);
                break;
        }
    }

    void EnemyAnimationState(EnemyState state)
    {
        if (currentAnimationState == state) return;

        currentAnimationState = state;
        anim.CrossFade(state.ToString(), 0.25f);
    }
    void UpdateRoam()
    {
        MoveTo(roamTarget);

        Debug.Log(Vector3.Distance(transform.position, roamTarget));

        if (Vector3.Distance(transform.position, roamTarget) < 1f)
            PickNewRoamPoint();

        if (CanSeePlayer())
        {
            ChangeState(BasicState.Chase);
            Debug.Log("Start Chase");
        }
    }

    void PickNewRoamPoint()
    {
        Vector3 rand = Random.insideUnitSphere * roamRadius;
        rand.y = 0;
        roamTarget = transform.position + rand;
    }
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
        Collider[] hits = Physics.OverlapSphere(transform.position, visionRange, targetLayer);
        if (hits.Length > 0)
        {
            target = hits[0].transform;    
            return true;
        }
        return false;
    }

    bool FindTargert()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position,visionRange,hits,targetLayer);
        float closest = Mathf.Infinity;
        Transform best = null;
        for (int i = 0; i < count; i++)
        {
            Collider h = hits[i];
            if (h.transform.root == transform.root) continue;
            if(!h.TryGetComponent<IDamageable>(out IDamageable dmg)) continue;

            float distance = (h.transform.position - transform.position).sqrMagnitude;
            if (distance < closest)
            {
                closest = distance;
                best = h.transform;
            }
        }
        target = best;
        return target!=null;
        
    }


    #endregion
    #region CHASE
    void UpdateChase()
    {
        if (!target) return;

        float dist = Vector3.Distance(transform.position, target.position);

        if (dist > visionRange)
        {
            ChangeState(BasicState.Search);
            searchTimer = 2f;
            return;
        }

        if (dist <= attackRange)
        {
            if (!isPunching)
            {
                ChangeState(BasicState.Attack);
                StartCoroutine(PunchRoutine());
            }
            return;
        }

        MoveTo(target.position);
    }
    #endregion

    #region SEARCH
    void UpdateSearch()
    {
        searchTimer -= Time.deltaTime;

        if (CanSeePlayer())
        {
            ChangeState(BasicState.Chase);
            return;
        }

        if (searchTimer <= 0)
            ChangeState(BasicState.Roam);
    }
    #endregion

    #region ATTACK
    IEnumerator PunchRoutine()
    {
        isPunching = true;
        rb.linearVelocity = Vector3.zero;
        Debug.Log("Punch Rotuine");
        

        yield return new WaitForSeconds(0.5f);

        Collider[] hits = Physics.OverlapSphere(attackPosition.position,attackRadius);

        foreach (var hit in hits)
        {
            if(hit.transform==transform.root) continue;

            Debug.Log(hit.gameObject.name);
            Vector3 effectPos = hit.ClosestPoint(attackPosition.position);

            Instantiate(effect, effectPos, Quaternion.identity);

            if (hit.TryGetComponent<IDamageable>(out var dmg))
            {
                Vector3 dir = (hit.transform.position - transform.position).normalized;
                dir.y += 0.3f;
                dmg.TakeDamage(dir, punchForce);
            }
        }

        yield return new WaitForSeconds(0.6f);
      
        isPunching = false;
        ChangeState(BasicState.Chase);
    }
    #endregion

    #region STATE
    void ChangeState(BasicState newState)
    {
        if(isStunned) return;
        if (currentBasicState == newState) return;

        currentBasicState = newState;

        switch (newState)
        {
            case BasicState.Roam:
                PickNewRoamPoint();
              //  EnemyAnimationState(EnemyState.Run);
                Debug.Log("Running");
                break;

            case BasicState.Search:
                rb.linearVelocity = Vector3.zero;
              //  EnemyAnimationState(EnemyState.Idle);
                break;

            case BasicState.Chase:
               // EnemyAnimationState(EnemyState.Run);
                Debug.Log("Chase");
                break;

            case BasicState.Attack:
                rb.linearVelocity = Vector3.zero;
               // EnemyAnimationState(EnemyState.HandAttack);
                break;

            case BasicState.Stunned:
                rb.linearVelocity = Vector3.zero;

                break;
        }
    }
    #endregion

    #region DAMAGE
    public void TakeDamage(Vector3 dir, float force)
    {
       // if (isPunching) return; // cannot stun while punching
        if (isStunned) return;

        StartCoroutine(StunRoutine(dir, force));
    }

    IEnumerator StunRoutine(Vector3 dir, float force)
    {
        isStunned = true;
        ChangeState(BasicState.Stunned);

        rb.AddForce(dir * force, ForceMode.Impulse);
        EnemyAnimationState(EnemyState.Attacked_2);

        yield return new WaitForSeconds(1f);
        EnemyAnimationState(EnemyState.Idle);
        yield return new WaitForSeconds(0.5f);
        isStunned = false;
        ChangeState(BasicState.Search);
    }
    #endregion

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(attackPosition.position, attackRadius);

    }
}
