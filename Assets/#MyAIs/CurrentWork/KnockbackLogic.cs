using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockbackLogic : MonoBehaviour
{
    [SerializeField]

    float knockBackforce = 15f, upfoce=5; 
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            HY_Player_Control.canControl=false;  
            Debug.Log("Collide");

            Rigidbody rb = collision.rigidbody;
            if (rb == null) return;

            Vector3 dir = (collision.transform.position - transform.position).normalized;
            dir.y = 0f; // keep horizontal knockback clean
            
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(dir * knockBackforce + Vector3.up * upfoce, ForceMode.VelocityChange);
            StartCoroutine(BackControl());


        }
    }

   IEnumerator BackControl()
    {
        yield return new WaitForSeconds(1);
        HY_Player_Control.canControl = true;
    }

}
