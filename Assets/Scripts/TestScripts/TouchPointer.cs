using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TouchPointer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    bool isHolding, chargedAttack, canAttack = true;
    float timer;
    [SerializeField]
    float holdTime = 3f;
    Image attackBtn;
    Animator animator;
    bool waitForFreshPress = false;
    private void Awake()
    {
        attackBtn = GetComponent<Image>();
        //animator = GetComponent<Animator>();
    }
    private void Update()
    {
        if (!canAttack) return;

        if (!isHolding) return;

        timer += Time.deltaTime;
        if (timer > holdTime && !chargedAttack)
        {
            chargedAttack = true;
            Debug.Log("Hard Attack"); // Punch Attack 
            StartCoroutine(AttackCoolDown());
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!canAttack) return;

        isHolding = true;
        timer = 0;
        chargedAttack = false;

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (waitForFreshPress)
        {
            waitForFreshPress = false;
            return;
        }
        if(!canAttack) return;

        if (!chargedAttack)
        {
            Debug.Log("Normal Punch");
            StartCoroutine(AttackCoolDown());
        }

        ResetPress();
    }

    void ResetPress()
    {
        isHolding = false;
        timer = 0;
        chargedAttack = false;
    }
    IEnumerator AttackCoolDown()
    {
        canAttack = false;
        waitForFreshPress = true;
        Color oldColor = attackBtn.color;
        attackBtn.CrossFadeColor(Color.red, 0.25f, true, true);
        yield return new WaitForSeconds(2);
        attackBtn.CrossFadeColor(oldColor,0.25f, true, true); 
        canAttack = true;
    }

}
