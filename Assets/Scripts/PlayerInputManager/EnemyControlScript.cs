using System.Collections;
using UnityEngine;

public enum EnemyState
{
    None=0,
    Idle,
    Run,
    Jump,
    HandAttack,
    Attacked,
    Attacked_2
}

[RequireComponent(typeof(Rigidbody))]
public class EnemyControlScript : MonoBehaviour, IDamageable
{
    Rigidbody rb;
    Animator animator;
    EnemyState currentState = EnemyState.Idle;


    [Header("Movement")]
    public float moveSpeed = 4f;
    public float directionChangeTime = 2f;
    public float idleTime = 0.5f;

    [Header("Rotation")]
    public float rotationSpeed = 10f;

    [Header("Jump")]
    public float jumpForce = 5f;
    public float jumpChance = 0.06f;

    [Header("Chase Player")]
    public Transform player;
    public float chaseChance = 0.12f;
    public float chaseDuration = 1.5f;
    public float chaseSpeedMultiplier = 1.3f;

    [Header("Ground Check")]
    public LayerMask groundMask;
    public float groundCheckDist = 0.2f;

    [Header("Tile Awareness")]
    public LayerMask tileMask;
    public float safeTileSearchRadius = 4f;
    public float panicJumpMultiplier = 1.2f;

    [Header("Animation")]
    [SerializeField] float runDampTime = 0.15f;


    Vector3 moveDir;
    bool canMove = true;
    bool isChasing;
    bool isGrounded;

    bool isStunned =false;
    Coroutine decisionRoutine;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }
    void Start()
    {
        decisionRoutine = StartCoroutine(DecisionRoutine());
    }
    public void TakeDamage(Vector3 dir, float force) // Take Damage 
    {
        if (isStunned) return;

        isStunned = true;
        canMove = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // stop AI loop
        if (decisionRoutine != null)
            StopCoroutine(decisionRoutine);

        // stop movement instantly
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // pushback
        rb.AddForce(dir * force, ForceMode.Impulse);

        EnemyAnimationState(EnemyState.Attacked_2);

        StartCoroutine(ResetEnemy());
    }

    void EnemyAnimationState(EnemyState state)

    {
        if (currentState == state) return;

        currentState = state;
        animator.CrossFade(state.ToString(), 0.25f);
    }

    IEnumerator ResetEnemy()
    {
        yield return new WaitForSeconds(2f);

        isStunned = false;
        canMove = true;

        EnemyAnimationState(EnemyState.Idle);

        // restart AI
        yield return new WaitForSeconds(0.5f);
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        decisionRoutine = StartCoroutine(DecisionRoutine());
    }



    private void Update()
    {
        UpdateAnimation();
    }
    void FixedUpdate()
    {
        if (isStunned) return;   // 🔴 HARD STOP EVERYTHING

        if (!canMove) return;

        isGrounded = IsGrounded();

        HandleMarkedTilePanic();

        Vector3 velocity = moveDir * moveSpeed;
        if (isChasing)
            velocity *= chaseSpeedMultiplier;

        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;

        // ROTATION
        if (moveDir.sqrMagnitude > 0.01f&& canMove)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }

    // ===================== ANIMATION =====================

    void UpdateAnimation()
    {
        if (isStunned) return;

        if (!isGrounded)
        {
            EnemyAnimationState(EnemyState.Jump);
            return;
        }

        float speed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;

        if (speed > 0.1f)
            EnemyAnimationState(EnemyState.Run);
        else
            EnemyAnimationState(EnemyState.Idle);
    }

    // ===================== AI LOGIC =====================

    IEnumerator DecisionRoutine()
    {
        while (true)
        {
            if (Random.value < jumpChance && isGrounded)
                Jump(jumpForce);

            if (player != null && Random.value < chaseChance)
            {
                yield return StartCoroutine(ChasePlayer());
            }
            else
            {
                PickRandomDirection();
                canMove = true;
                yield return new WaitForSeconds(directionChangeTime);
            }

            canMove = false;
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            yield return new WaitForSeconds(idleTime);
        }
    }

    IEnumerator ChasePlayer()
    {
        isChasing = true;

        float t = 0f;
        while (t < chaseDuration)
        {
            Vector3 dir = player.position - transform.position;
            dir.y = 0;
            moveDir = dir.normalized;

            t += Time.deltaTime;
            yield return null;
        }

        isChasing = false;
    }

    void PickRandomDirection()
    {
        moveDir = new Vector3(
            Random.Range(-1f, 1f),
            0,
            Random.Range(-1f, 1f)
        ).normalized;
    }

    void Jump(float force)
    {
        rb.AddForce(Vector3.up * force, ForceMode.Impulse);
    }

    // ===================== TILE PANIC =====================

    void HandleMarkedTilePanic()
    {
        RumbleTile tile = GetTileUnder();
        if (tile == null || !tile.IsMarked) return;

        // Panic jump
        if (isGrounded)
            Jump(jumpForce * panicJumpMultiplier);

        // Run toward nearest safe tile
        Vector3 safeDir = FindSafeTileDirection();
        if (safeDir != Vector3.zero)
        {
            moveDir = safeDir;
            canMove = true;
            isChasing = false;
        }
    }

    RumbleTile GetTileUnder()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.2f,
                             Vector3.down,
                             out RaycastHit hit,
                             2f,
                             tileMask))
        {
            return hit.collider.GetComponent<RumbleTile>();
        }
        return null;
    }

    Vector3 FindSafeTileDirection()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            safeTileSearchRadius,
            tileMask
        );

        float bestDist = float.MaxValue;
        Vector3 bestDir = Vector3.zero;

        foreach (Collider col in hits)
        {
            RumbleTile tile = col.GetComponent<RumbleTile>();
            if (tile == null || tile.IsMarked) continue;

            float d = Vector3.Distance(transform.position, tile.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                Vector3 dir = tile.transform.position - transform.position;
                dir.y = 0;
                bestDir = dir.normalized;
            }
        }

        return bestDir;
    }

    // ===================== GROUND =====================

    bool IsGrounded()
    {
        return Physics.Raycast(
            transform.position + Vector3.up * 0.1f,
            Vector3.down,
            groundCheckDist,
            groundMask
        );
    }
}
