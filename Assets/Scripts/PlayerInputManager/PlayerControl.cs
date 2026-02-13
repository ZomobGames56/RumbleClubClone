using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerState
{
    None = 0,
    Idle,
    Run,
    Jump,
    Hang,
    Dash,
    Swim,
    Throw,
    HandAttack,
    Attacked,
    Attacked_2

}
[RequireComponent(typeof(Rigidbody))]
public class PlayerControl : MonoBehaviour,IDamageable
{
    #region Variables
    Rigidbody rb;
    [Header("Player Move Speed")]
    [SerializeField]
    float moveSpeed = 8;
    [Header("Player Jump Force")]
    [SerializeField]
    float jumpForce = 8;
    //Input Value
    float h, v;
    Vector3 move;
    Animator animator;

    PlayerState currentState = PlayerState.Idle;

    [Header("Attack position ")]
    [SerializeField]
    Transform attackPosition;

    [Header("Attack Radius ")]
    [SerializeField]
    float attackRadius = 2;

    [Header("Attack Cool Down Time")]
    [SerializeField]
    float handAttackCoolDownTime = 2f;

    [Header("Attack Force")]
    [SerializeField]
    float attackForce = 12f;

    int reactionLayer, actionLayer;
    bool isStateLocked = false;

    [Header("Camera Transform")]
    [SerializeField]
    Transform cam;
    [SerializeField]
    GameObject effect;
    bool canAttack = true;

    float holdTimer = 0f;
    bool isHolding = false;
    bool chargedFired = false;

    float chargeTime = 3f;
    [SerializeField]
    Button attackBtn;

    bool isInputStateLock = false;
    public bool isGrounded = true;
    float inAirTimer;
    bool canMove = true;
    [SerializeField]
    int count = 0;
    bool isStunned = false;
    bool isPunching = false;
    #endregion

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        reactionLayer = animator.GetLayerIndex("Reaction Layer");
        actionLayer = animator.GetLayerIndex("Action Layer");


    }
    private void Update()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        if (move.magnitude != 0)
        {
            Rotate();
        }

        PunchAttackHandle();
        UpdateLocoMotion();
        Jump();

    }
    private void FixedUpdate()
    {
        PlayerMovement();
    }
    #region Player Movement Handle
    void PlayerMovement()
    {
        if (!canMove) return;
        Vector3 camForwad = cam.forward;
        Vector3 camRight = cam.right;

        camForwad.y = 0;
        camRight.y = 0;
        camForwad.Normalize(); camRight.Normalize();
        move = camRight * h + camForwad * v;
        // move = new Vector3( h, 0, v);
        move = Vector3.ClampMagnitude(move, 1f);
        move.y = 0;
        Vector3 velocity = rb.linearVelocity;
        if (move.magnitude > 0.01f)
        {
            velocity.x = move.x * moveSpeed;
            velocity.z = move.z * moveSpeed;

        }
        else
        {
            velocity.x = 0;
            velocity.z = 0;

        }

        rb.linearVelocity = velocity;

    }
    #endregion
    public void Rotate()
    {
        Quaternion rot = Quaternion.LookRotation(move, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rot,
            720f * Time.deltaTime);
    }

    #region Jump Function
    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            isStateLocked = true;
            isGrounded = false;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            //if (isStunned) return;
            PlayerAnimationStateUpdate(PlayerState.Jump, true,0.05f);
            StartCoroutine(JumpRoutine());
        }
    }
    #endregion
    IEnumerator JumpRoutine()
    {
        // small delay so jump anim shows
        yield return new WaitForSeconds(0.5f);
        if(isStunned) yield break;
        // while in air
        while (!isGrounded)
        {
            PlayerAnimationStateUpdate(PlayerState.Hang, true, 0.1f);
            yield return null;
        }

        // landed
        isStateLocked = false;
    }
    void PlayerAnimationStateUpdate(PlayerState state, bool lockState = false, float transactionDuration = 0.2f)
    {
        if (currentState == state) return;
        if (lockState)
        {
            isStateLocked = true;
        }


        currentState = state;
        animator.CrossFade(state.ToString(), transactionDuration);
        //animator.SetInteger((int)state, 1);
    }
    void UpdateLocoMotion(float transactionTime = 0.2f)
    {
        if (isStateLocked) return;
        if(isStunned) return;


        if (InAirCheck())
        {
            PlayerAnimationStateUpdate(PlayerState.Hang, true, 0.2f);
            Debug.Log("Hang");
            return;
        }

        if (move.magnitude != 0)
        {
            PlayerAnimationStateUpdate(PlayerState.Run,isStateLocked,0.1f);
        }
        else
        {
            PlayerAnimationStateUpdate(PlayerState.Idle,isStateLocked,0.1f);

        }
    }

    #region Punch Attack Input handle
    void PunchAttackHandle()
    {

        if (Input.GetMouseButtonDown(0))
        {
            // if attack locked → mark this input as invalid
            
            if (!canAttack)
            {
                isInputStateLock = true;
                return;
            }

            isHolding = true;
            holdTimer = 0f;
            chargedFired = false;
            isInputStateLock = false;
        }

        // While holding
        if (isHolding)
        {
            holdTimer += Time.deltaTime;
            OnHoldingAttack();

            Debug.Log($"Loading... {(int)holdTimer:F2}");

            if (holdTimer >= chargeTime && !chargedFired)
            {
                chargedFired = true;
                isHolding = false;

                Debug.Log("CHARGED PUNCH!");
                StartCoroutine(HandPunchAttack());
            }
        }

        // Mouse released
        if (Input.GetMouseButtonUp(0))
        {
            if (isInputStateLock)
            {
                isInputStateLock = false;
                isHolding = false;
                holdTimer = 0f;
                return;
            }
            if (!chargedFired && isHolding && canAttack)
            {
                Debug.Log("Quick tap → Normal Punch");

                StartCoroutine(HandPunchAttack());
            }
            isHolding = false;
            holdTimer = 0f;
        }

    }
    #endregion 
    #region Attack Hold
    void OnHoldingAttack()
    {
        var state = animator.GetCurrentAnimatorStateInfo(actionLayer);
        var next = animator.GetNextAnimatorStateInfo(actionLayer);

        if (state.IsName("HandRotate") || next.IsName("HandRotate"))
            return;

        animator.SetLayerWeight(actionLayer, 1f);
        animator.CrossFade("HandRotate", 0.08f, actionLayer);


    }
    #endregion
    #region  Punch Attack
    IEnumerator HandPunchAttack()
    {
        if (!canAttack) yield break;

        canAttack = false;
        isPunching = true;
        attackBtn.interactable = canAttack;


        //action set to 0
        animator.Play("Empty", actionLayer);
        animator.SetLayerWeight(actionLayer, 0f);

        //Reaction layer set to 1
        animator.SetLayerWeight(reactionLayer, 1);
        animator.Play("HandAttack", reactionLayer);

        yield return new WaitForSeconds(0.5f);

        DoHit();

        //StartCoroutine(AttackCoolDown());
        yield return new WaitForSeconds(handAttackCoolDownTime);
        animator.Play("Empty", reactionLayer);
        animator.SetLayerWeight(reactionLayer, 0f);

        //Reset Attack Time
        yield return new WaitForSeconds(0.15f);
        canAttack = true;
        attackBtn.interactable = canAttack;
        isPunching = false;
        // canAttack = true;
    }
    void DoHit()
    {
        Collider[] hits = Physics.OverlapSphere(attackPosition.position, attackRadius);
        foreach (Collider hit in hits)
        {
            if (hit.transform.root == transform.root) continue;

            Debug.Log($"Hit: {hit.name}");
            Vector3 effectSpawn = hit.ClosestPoint(attackPosition.position);
            effectSpawn.y += 1.25f;
            Instantiate(effect, effectSpawn, Quaternion.identity);
            if (hit.attachedRigidbody != null)
            {
                Vector3 dir = (hit.attachedRigidbody.position - transform.position).normalized;
                dir.y += 0.6f;
                IDamageable otherDmg = hit.GetComponentInParent<IDamageable>();

                if (otherDmg != null)
                    otherDmg.TakeDamage(dir, attackForce);
            }
            //if (hit.attachedRigidbody != null)
            //{

            //    Vector3 dir = (hit.attachedRigidbody.position - transform.position).normalized;
            //    dir.y += 0.5f;
            //    hit.attachedRigidbody.AddForce(dir * attackForce, ForceMode.Impulse);
            //}
        }
    }
    #endregion

  
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
            //isStateLocked = false;
            isGrounded = true;
            count++;
            // UpdateLocoMotion();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ground"))
        {
          //  isStateLocked = true;
            //isGrounded = false;
            count--;
            if (count == 0)
            {
                isGrounded = false;
            }
            Debug.Log("Exit");
        }
    }

    void HangingAniamtion()
    {
        if (InAirCheck())
            PlayerAnimationStateUpdate(PlayerState.Hang, true, 0.2f);
        else
            UpdateLocoMotion(0.1f);
    }
    bool InAirCheck()
    {
        if (isGrounded)
        {
            inAirTimer = 0f;
            return false;
        }

        inAirTimer += Time.deltaTime;

        if (inAirTimer >= 0.15f)
            return true;

        return false;
    }
    private void OnDrawGizmos()
    {
        if (attackPosition == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPosition.position, attackRadius);
    }
    #region Take Damage
    public void TakeDamage(Vector3 dir, float force)
    {
        //can control fasle, rb's velocity = 0, add force dir 
        if (isPunching) return;
        if (isStunned) return;
       canMove = false;
        isStunned = true;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(dir * force, ForceMode.Impulse);

        // PlayerAnimationStateUpdate(PlayerState.Attacked, true, 0.2f);
        animator.SetLayerWeight(reactionLayer, 1);
        animator.Play("Attacked", reactionLayer);
        animator.CrossFade("Attacked_2", 0.2f, reactionLayer);
        StartCoroutine(StunnedBack());
    }

    IEnumerator StunnedBack()
    {
        yield return new WaitForSeconds(2f);
       
        canMove = true;
        isStunned = false;  
        isStateLocked = false;

        animator.Play("Empty", reactionLayer);
        animator.SetLayerWeight(reactionLayer, 0f);
    }
    #endregion
}
