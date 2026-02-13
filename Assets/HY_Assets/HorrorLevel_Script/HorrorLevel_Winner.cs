using UnityEngine;

public class HorrorLevel_Winner : MonoBehaviour
{

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Player" || collision.transform.tag == "Enemy")
        {
            collision.gameObject.SetActive(false);
        }
    }
}
