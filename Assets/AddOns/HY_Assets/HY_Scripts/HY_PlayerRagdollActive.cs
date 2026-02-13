using System.Collections;
using UnityEngine;

public class HY_PlayerRagdollActive : MonoBehaviour
{
    public static HY_PlayerRagdollActive instance;
    Rigidbody[] childRbs;
    Animator animator;

    [SerializeField]
    Transform hip;

    public GameObject Parent;
    [SerializeField] GameObject effect;
    Transform spawnPoint;
    //public HY_NavMeshEnemy _refNavMesh;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        childRbs = GetComponentsInChildren<Rigidbody>();
        EnableKinamatic();
        animator = GetComponentInParent<Animator>();

    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            OnObstacleCollide();

        }
    }
    void EnableKinamatic()
    {
        foreach (var child in childRbs)
        {
            child.isKinematic = true;
            child.constraints = RigidbodyConstraints.FreezeAll;
        }
    }
    void DisableKinamatic()
    {
        foreach (var child in childRbs)
        {
            child.isKinematic = false;
            child.constraints = RigidbodyConstraints.None;
        }
    }
    IEnumerator ResetRagoll(float wait)
    {
        yield return new WaitForSeconds(wait);
        Parent.transform.position = transform.position;
        animator.enabled = true;
        HY_Player_Control.canControl = true;
        Debug.Log("Just called");
        foreach (var child in childRbs)
        {
            child.isKinematic = true;
            child.constraints = RigidbodyConstraints.FreezeAll;
        }


    }
    public void RagdollActivate()
    {
        animator.enabled = false;
        DisableKinamatic();
        StartCoroutine(ResetRagoll(3f));
        HY_Player_Control.canControl = false;
        Debug.Log("Just called");
    }
    // [System.Obsolete]
    private void OnCollisionEnter(Collision collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Obstacle":
                HY_Player_Control.canControl = false;
                animator.enabled = false;
                DisableKinamatic();
                // StartCoroutine(ResetRagoll(5f));
                Debug.Log("Collide Obstacle " + gameObject.name);
                break;

        }


    }

    public void OnObstacleCollide()
    {
        HY_Player_Control.canControl = false;
        animator.enabled = false;
        DisableKinamatic();
        StartCoroutine(ResetRagoll(2.5f));
    }


}


