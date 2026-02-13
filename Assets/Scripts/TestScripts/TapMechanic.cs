using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TapMechanic : MonoBehaviour 
{
    float holdTimer = 0f;
    bool isHolding = false;
    bool chargedFired = false;

    float chargeTime = 3f;
    [SerializeField]
    Button punchBtn;
    bool canAttack = true;
   
    
    private void Start()
    {
        canAttack = true;
        punchBtn.interactable = true;
    }
    void Update()
    {
        if (!canAttack) return;
        // Mouse pressed
        if (Input.GetMouseButtonDown(0))
        {
            isHolding = true;
            holdTimer = 0f;
            chargedFired = false;

            Debug.Log("Start loading attack...");
        }

        // While holding
        if (isHolding)
        {
            holdTimer += Time.deltaTime;

            // loading animation state
            //animator.Play(1,2);
            Debug.Log($"Loading... {(int)holdTimer:F2}");

            if (holdTimer >= chargeTime && !chargedFired)
            {
                chargedFired = true;
                isHolding = false;

                Debug.Log("CHARGED PUNCH!");
                HardPunch();
            }
        }

        // Mouse released
        if (Input.GetMouseButtonUp(0))
        {
            if (!chargedFired)
            {
                Debug.Log("Quick tap → Normal Punch");
                NormalPunch();
            }

            isHolding = false;
            holdTimer = 0f;
            //make can attack false
            //canAttack = false;
        }
    }

    void PunchAttack()
    {
        Debug.Log("Punch attack executed");
        // Set can attack bool false; and start coroutine
    }
    void HardPunch()
    {
        Debug.Log("Hard Punch");
        // Set can attack bool false; and start coroutine
        canAttack = false;
        punchBtn.interactable = canAttack;
        //Play Punch Animation 


        StartCoroutine(CanAttackReset());

    }
    void NormalPunch()
    {
        Debug.Log("Normal punch ");
        // Set can attack bool false; and start coroutine
        canAttack = false;
        punchBtn.interactable = canAttack;
        //Play Punch Animation 
        StartCoroutine(CanAttackReset());
    }
    void Holding()
    {
        Debug.Log("Can Animate Hold Punch");
    }
    IEnumerator CanAttackReset()
    {
        yield return new WaitForSeconds(2f);
        canAttack = true;
        punchBtn.interactable = canAttack;

    }


}
