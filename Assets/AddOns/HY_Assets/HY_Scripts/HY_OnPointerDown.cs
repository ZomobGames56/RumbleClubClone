using UnityEngine;
using UnityEngine.EventSystems;

public class HY_OnPointerDown : MonoBehaviour, IPointerDownHandler
{
    [SerializeField]
    HY_Player_Control player_Ref;
    void Awake()
    {
        if (player_Ref == null)
        {
            player_Ref = FindAnyObjectByType<HY_Player_Control>();
        }

    }
    public void OnPointerDown(PointerEventData eventData)
    {
        player_Ref.MobileJumpBtn();     
        Debug.Log("Jump Function");
    }

    

   
}
