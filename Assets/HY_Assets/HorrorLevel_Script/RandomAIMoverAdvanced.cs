#region
//using UnityEngine;
//using System.Collections;

//[RequireComponent(typeof(Rigidbody))]
//public class RandomAIMoverAdvanced : MonoBehaviour
//{
//    [Header("Movement")]
//    public float moveSpeed = 4f;
//    public float directionChangeTime = 2f;
//    public float idleTime = 0.5f;

//    [Header("Rotation")]
//    public float rotationSpeed = 10f;

//    [Header("Jump")]
//    public float jumpForce = 5f;
//    public float jumpChance = 0.06f; // 6%

//    [Header("Chase Player")]
//    public Transform player;
//    public float chaseChance = 0.12f; // 12%
//    public float chaseDuration = 1.5f;
//    public float chaseSpeedMultiplier = 1.3f;

//    [Header("Ground Check")]
//    public LayerMask groundMask;
//    public float groundCheckDist = 0.2f;
//    [SerializeField]
//    bool isGrounded;

//    Rigidbody rb;
//    Vector3 moveDir;
//    bool canMove = true;
//    bool isChasing = false;
//    [SerializeField] float runDampTime = 0.15f;
//    Animator anim;
//    void Awake()
//    {
//        anim = GetComponent<Animator>();
//        rb = GetComponent<Rigidbody>();
//        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
//    }

//    void Start()
//    {
//        StartCoroutine(DecisionRoutine());
//       // Debug.Log(Random.value);
//    }
//    private void Update()
//    {
//        UpdateAnimation();
//    }
//    void FixedUpdate()
//    {
//        if (!canMove) return;

//        Vector3 velocity = moveDir * moveSpeed;
//        if (isChasing)
//            velocity *= chaseSpeedMultiplier;

//        velocity.y = rb.linearVelocity.y;
//        rb.linearVelocity = velocity;

//        if (moveDir != Vector3.zero)
//        {
//            Quaternion targetRot = Quaternion.LookRotation(moveDir);
//            transform.rotation = Quaternion.Slerp(
//                transform.rotation,
//                targetRot,
//                rotationSpeed * Time.fixedDeltaTime
//            );
//        }

//        isGrounded = IsGrounded();
//    }
//    void UpdateAnimation()
//    {
//        bool grounded = IsGrounded();
//        // anim.SetBool("Jump", grounded);
//        anim.SetBool("Jump", !isGrounded);
//        anim.SetBool("Hanging", !isGrounded);

//        float speed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;

//       // bool canMoveTowardTarget = canMove; // you can extend this later

//        if (grounded  && speed > 0.1f)
//        {
//            anim.SetFloat("Run", 1f, runDampTime, Time.deltaTime);
//        }
//        else
//        {
//            anim.SetFloat("Run", 0f, runDampTime, Time.deltaTime);
//        }
//    }
//    IEnumerator DecisionRoutine()
//    {
//        while (true)
//        {
//            // 🔹 Rare jump
//            if (Random.value < jumpChance && IsGrounded())
//            {
//                Jump();
//            }

//            // 🔹 Rare chase
//            if (player != null && Random.value < chaseChance)
//            {
//                yield return StartCoroutine(ChasePlayer());
//            }
//            else
//            {
//                PickRandomDirection();
//                canMove = true;
//                yield return new WaitForSeconds(directionChangeTime);
//            }

//            // Idle pause
//            canMove = false;
//            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
//            yield return new WaitForSeconds(idleTime);
//        }
//    }

//    IEnumerator ChasePlayer()
//    {
//        isChasing = true;

//        float timer = 0f;
//        while (timer < chaseDuration)
//        {
//            Vector3 dir = player.position - transform.position;
//            dir.y = 0;
//            moveDir = dir.normalized;

//            timer += Time.deltaTime;
//            yield return null;
//        }

//        isChasing = false;
//    }

//    void PickRandomDirection()
//    {
//        moveDir = new Vector3(
//            Random.Range(-1f, 1f),
//            0,
//            Random.Range(-1f, 1f)
//        ).normalized;
//    }

//    void Jump()
//    {
//        if (!isGrounded) return;
//        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
//    }

//    bool IsGrounded()
//    {
//        return Physics.Raycast(
//            transform.position + Vector3.up * 0.1f,
//            Vector3.down,
//            groundCheckDist,
//            groundMask
//        );
//    }
//}
#endregion old UnUsed Code

using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class RandomAIMoverAdvanced : MonoBehaviour
{
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

    Rigidbody rb;
    Animator anim;

    Vector3 moveDir;
    bool canMove = true;
    bool isChasing;
    bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Start()
    {
        StartCoroutine(DecisionRoutine());
    }

    void Update()
    {
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        isGrounded = IsGrounded();

        HandleMarkedTilePanic();

        if (!canMove) return;

        Vector3 velocity = moveDir * moveSpeed;
        if (isChasing)
            velocity *= chaseSpeedMultiplier;

        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;

        if (moveDir.sqrMagnitude > 0.01f)
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
        anim.SetBool("Jump", !isGrounded);
        anim.SetBool("Hanging", !isGrounded);

        float speed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;

        if (isGrounded && speed > 0.1f)
            anim.SetFloat("Run", 1f, runDampTime, Time.deltaTime);
        else
            anim.SetFloat("Run", 0f, runDampTime, Time.deltaTime);
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
