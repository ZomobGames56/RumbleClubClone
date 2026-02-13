using Unity.VisualScripting;
using UnityEngine;

public class HY_CameraControl : MonoBehaviour
{
    [Header("Distance")]
    [SerializeField] float dis = 3f;

    [Header("Rotation Limits")]
    [SerializeField] float minY = -50f;
    [SerializeField] float maxY = 50f;

    [Header("Manual Control")]
    [SerializeField] float sensivity = 200f;
    [SerializeField] float sensivityY = 60f;

    [Header("Auto Follow")]
    [SerializeField] float followYawSpeed = 3f;   // how fast camera aligns
    [SerializeField] float followStrength = 0.4f; // how much it helps

    [SerializeField] Transform lookAt;
    public FixedTouchField TouchField;

    Quaternion rot;
    Vector3 dir;

    public float currentX;
    public float currentY;
    [SerializeField]
    int currentY_ = 360;
    // 🔥 received from player
    [HideInInspector] public Vector3 playerMoveDir;


   
    void Start()
    {
        currentY = 30f;   // starting pitch
        currentX = currentY_;
    }

    void LateUpdate()
    {
        MouseRotation();
    }

    void MouseRotation()
    {
        // 1️⃣ MANUAL CAMERA INPUT
        currentX += TouchField.TouchDist.x * sensivity * Time.deltaTime;
        currentY -= TouchField.TouchDist.y * sensivityY * Time.deltaTime;
        currentY = Mathf.Clamp(currentY, minY, maxY);

        Quaternion manualRot = Quaternion.Euler(currentY, currentX, 0);

        // 2️⃣ AUTO ALIGN TO PLAYER MOVE (YAW ONLY)
        if (playerMoveDir.sqrMagnitude > 0.1f)
        {
            Vector3 flatMove = playerMoveDir;
            flatMove.y = 0;

            Quaternion targetYaw = Quaternion.LookRotation(flatMove);
            Quaternion yawOnly = Quaternion.Euler(0, targetYaw.eulerAngles.y, 0);

            manualRot = Quaternion.Slerp(
                manualRot,
                Quaternion.Euler(currentY, yawOnly.eulerAngles.y, 0),
                followStrength * followYawSpeed * Time.deltaTime
            );

            // keep internal yaw synced (prevents snap)
            currentX = manualRot.eulerAngles.y;
        }
        
        rot = manualRot;
        dir = new Vector3(0, 0, -dis);

        transform.position = lookAt.position + rot * dir;
        transform.LookAt(lookAt.position);
    }
}
