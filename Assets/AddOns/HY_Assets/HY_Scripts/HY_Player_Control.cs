using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HY_Player_Control : MonoBehaviour
{
    [SerializeField]
    Joystick joystick;// JoyStick Refrence Given in the canvas.
    [SerializeField]
    Rigidbody rb;//Player Rigdbody
    Button btn;
    Vector3 move;
    [SerializeField]
    float moveSpeed = 10f, force = 7f, defaultSpeed = 0.97f, onSliderSpeed = 2.0f,
        waitForSec = 0.5f, transformMoveSpeed = 8f;//Jump Force
    [SerializeField]
    public Animator animator;
    [SerializeField]
    bool isGrounded, isDashing;
    [SerializeField]
    Transform cam;
    bool jumpbtnPressed = false;
    [SerializeField]
    Transform spawnPoint, firstSp, secondSp, thirdSp, fourthSp;
    [SerializeField]
    bool inAir;
    [SerializeField]
    GameObject effect;

    //[SerializeField]
    //float lastTapTime = 0f, doubleTapThreshold = 0.3f;
    [SerializeField]
    float dashDuration = 0.5f, dashSpeed = 10f;
    Vector3 playerScale;
    [SerializeField]
    float scale = 0.75f;
    [SerializeField]
    public static bool canControl;
    float inAirTime;
    RaycastHit hit;

    bool isCalled;
    public bool rigidBodyControl, transformControl;
    [SerializeField]
    ParticleSystem dustEffect;

    [SerializeField]
    AudioClip jumpSound, fallInWater, collideSound;
    bool collideToWater;
    public GameObject dummyScreen;
    [SerializeField] HY_CameraControl camControl;




    void Start()
    {
        collideToWater = false;

        isCalled = false;

        canControl = true;

        rb = GetComponent<Rigidbody>();

        isGrounded = false;

        animator = GetComponent<Animator>();

        if (rb != null&& rigidBodyControl)
        {

            rb.position = spawnPoint.position;
            rb.rotation = spawnPoint.rotation;

        }

        //rb.MovePosition(spawnPoint.position);
        playerScale = new Vector3(scale, scale, scale);

        transform.localScale = playerScale;
        //  rigidBodyControl = true;
        //  transformControl = false;
        jumpbtnPressed = false;

    }
    // Update is called once per frame

    void Update()
    {
        PlayerOutOfBounds();
        if (InAirTime() >= 0.15f)
        {
            HangingAnimation();
        }
        if (canControl == true)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                MobileJumpBtn();
            }
            CanAniamte();
        }
    }
    private void FixedUpdate()
    {
        if (canControl == true)
        {
            PlayerMovement();
        }
    }
    void HangingAnimation()
    {
        animator.SetBool("Hanging", true);
    }// hanging animation true..

    public void PlayerMovement()
    {
        if ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer && canControl
            || Application.platform == RuntimePlatform.WindowsEditor))
        {
            #region
            ////Debug.Log("Mobile");
            //joystick.gameObject.SetActive(true);
            //move = (cam.right * joystick.Horizontal +
            //       cam.forward * joystick.Vertical).normalized;
            //move.y = 0f;
            //if (transformControl)
            //{
            //    transform.position += move * moveSpeed * Time.deltaTime;
            //   // Debug.Log("Transform one is calling");
            //}
            //if (rigidBodyControl)
            //{
            //    if (rb != null)
            //    {
            //        rb.MovePosition(transform.position + move * moveSpeed * Time.fixedDeltaTime);
            //       // Debug.Log("rigid one is calling");

            //    }
            //}

            //if (move.magnitude != 0)
            //{
            //  //  dustEffect.Play();
            //    Rotate();
            //}
            ////if (move.magnitude == 0)
            ////{
            ////    dustEffect.Stop();
            ////}
            #endregion
            joystick.gameObject.SetActive(true);

            Vector3 camForwad = cam.forward;

            Vector3 camRight = cam.right;
            camForwad.y = 0f; camRight.y = 0f;
            camForwad.Normalize(); camRight.Normalize();
            // float h = Input.GetAxis("Horizontal");
            // float v = Input.GetAxis("Vertical");
            move = camRight * joystick.Horizontal + camForwad * joystick.Vertical;

            move = Vector3.ClampMagnitude(move, 1f);

            move.y = 0f;

            // for camera rotation.!!!!!!
            if (camControl != null && canControl)
            {
                camControl.playerMoveDir = move;
            }

            if (transformControl)
            {
                transform.position += move * moveSpeed * Time.deltaTime;
                Debug.Log("Transform one is calling");
            }
            if (rigidBodyControl)
            {
                if (rb != null)
                {

                    Vector3 velocity = rb.linearVelocity;
                    if (move.sqrMagnitude > 0.01f)
                    {
                        velocity.x = move.x * moveSpeed;
                        velocity.z = move.z * moveSpeed;
                    }
                    else
                    {
                        // Hard stop when input released
                        velocity.x = 0f;
                        velocity.z = 0f;
                    }

                    rb.linearVelocity = velocity;
                    // rb.velocity = move*moveSpeed;


                    // animator.SetFloat("Run",rb.linearVelocity.magnitude);
                    //Debug.Log("rigid one is calling");

                }
            }
            if (move.magnitude != 0)
            {
                Rotate();
                // dustEffect.Play();
            }
        }

        else if ((Application.platform == RuntimePlatform.WindowsPlayer) && canControl)
        {
            joystick.gameObject.SetActive(false);
            Vector3 camForwad = cam.forward;
            Vector3 camRight = cam.right;
            camForwad.y = 0f;
            camRight.y = 0f;
            camForwad.Normalize();
            camRight.Normalize();
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            move = camRight * h + camForwad * v;
            move = Vector3.ClampMagnitude(move, 1f);

            // move.Normalize();

            move.y = 0f;
            if (transformControl)
            {
                transform.position += move * moveSpeed * Time.deltaTime;
                Debug.Log("Transform one is calling");
            }
            if (rigidBodyControl)
            {
                if (rb != null)
                {
                    // rb.MovePosition(transform.position + move * moveSpeed * Time.fixedDeltaTime);
                    //rb.linearVelocity = Vector3 velocity = rb.linearVelocity;
                    Vector3 velocity = rb.linearVelocity;
                    if (move.sqrMagnitude > 0.01f)
                    {
                        velocity.x = move.x * moveSpeed;
                        velocity.z = move.z * moveSpeed;
                    }
                    else
                    {
                        // Hard stop when input released
                        velocity.x = 0f;
                        velocity.z = 0f;
                    }

                    rb.linearVelocity = velocity;
                    // rb.velocity = move*moveSpeed;


                    //animator.SetFloat("Run",rb.velocity.magnitude);
                    //Debug.Log("rigid one is calling");

                }
            }
            if (move.magnitude != 0)
            {
                Rotate();
                // dustEffect.Play();
            }
        }

    }// joy stick movment.
    public void MobileJumpBtn()
    {
        if (isGrounded && !jumpbtnPressed)
        {
            animator.SetBool("Jump", true);
            //HY_AudioManager.instance.PlayAudioEffectOnce(jumpSound);

            rb.AddForce(Vector3.up * force, ForceMode.Impulse);
            isGrounded = false;
            inAir = true;
            jumpbtnPressed = true;
            StartCoroutine(JumpUp());
        }
    }
    public IEnumerator JumpUp()
    {
        yield return new WaitForSeconds(0.2f);
        animator.SetBool("Jump", false);
        animator.SetBool("Hanging", true);
        //rb.AddForce(Vector3.up * (-gravity), ForceMode.Impulse);
    }
    IEnumerator Dash()
    {
        //dash animation.
        animator.SetBool("Dash", true);
        isDashing = true;
        float timer = 0;
        while (timer < dashDuration)
        {
            rb.MovePosition(Vector3.forward * dashSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            print("Dash Animation called");
            yield return null;
        }
        isDashing = false;
        //dash animation stop
        animator.SetBool("Dash", false);
    }//dash animation....//Not in use yet...............
    public void CanAniamte()
    {

        if (move.magnitude != 0)
        {
            animator.SetFloat("Run", move.magnitude);
        }
        else
        {
            animator.SetFloat("Run", 0);
        }

    }

    public void Rotate()
    {
        Quaternion rot = Quaternion.LookRotation(move, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rot,
            720f * Time.deltaTime);
    }

    public void OnCollisionStay(Collision collision)
    {

        if (collision.transform.tag == "Slider")
        {
            moveSpeed = onSliderSpeed;
            isDashing = true;
            animator.SetBool("Jump", false);
            animator.SetBool("Dash", true);
            animator.SetTrigger("Dashing");
            animator.SetBool("Hanging", false);
            isGrounded = true;
            jumpbtnPressed = false;
            inAir = false;

        }


    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Obstacle")
        {
           // HY_AudioManager.instance.PlayAudioEffectOnce(collideSound);
            HY_PlayerRagdollActive.instance.OnObstacleCollide();
        }
        if (collision.transform.tag == "Water" && !collideToWater)
        {
            collideToWater = true;
            OnCollideWater();
        }
    }
    void PlayerOutOfBounds()
    {
        if (transform.position.y <= -51 && !isCalled)
        {
            Debug.Log("Player out of bound");
            // gameObject.SetActive(false
            isCalled = true;
            canControl = false;
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 500f);
            //rb.isKinematic = true;
            StartCoroutine(SpawnWait());


        }
    }

    //This function is responsible for Transform collide with water.
    private void OnCollideWater()
    {
        canControl = false;
       // HY_AudioManager.instance.PlayAudioEffectOnce(fallInWater);
        Instantiate(effect, transform.position, Quaternion.Euler(90, 0, 0));
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 5f);

        StartCoroutine(SpawnWait());
    }
    public IEnumerator SpawnWait()
    {
       
        yield return new WaitForSeconds(waitForSec);

        // Stop physics


        // Teleport correctly
        rb.position = spawnPoint.position;
        rb.rotation = spawnPoint.rotation;
        //rb.isKinematic = false;
        // Reset scale instantly
        transform.localScale = playerScale;

        // Reset states
        canControl = true;
        isCalled = false;
        isGrounded = true;
        inAir = false;
        jumpbtnPressed = false;
        isDashing = false;
        moveSpeed = defaultSpeed;

        animator.SetBool("Dash", false);
        animator.SetBool("Hanging", false);
        animator.SetBool("Jump", false);

        if (camControl != null)
        {
            camControl.currentX = 360f;
        }
        rb.isKinematic = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "FirstSp":
                spawnPoint = firstSp;
                break;
            case "SecondSp":
                spawnPoint = secondSp;
                break;
            case "ThirdSp":
                spawnPoint = thirdSp;
                break;
            case "FourthSp":
                spawnPoint = fourthSp;
                break;
            case "Goal":
                dummyScreen.SetActive(true);
                //Time.timeScale = 0;
                StartCoroutine(LevelSelectionScene());
                rb.isKinematic = true;
                break;


        }

    }
    IEnumerator LevelSelectionScene()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(6);
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Ground")
        {
            collideToWater = false;
            canControl = true;
            isCalled = false;
            isGrounded = true;
            animator.SetBool("Hanging", false);
            jumpbtnPressed = false;
            inAir = false;
            moveSpeed = defaultSpeed;
            animator.SetBool("Dash", false);
            isDashing = false;
            // targert point  a-b = c 
            //in air-timer stop
            //inAirTime = 0;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Ground")
        {
            inAir = true;
            isGrounded = false;

            //in air-timer go
            //inAirTime += Time.deltaTime;
        }
    }


    float InAirTime()
    {
        if (!isGrounded && inAir && !isDashing)
        {
            inAirTime += Time.deltaTime;
        }
        else
        {
            inAirTime = 0;
        }
        return inAirTime;
    }
}
