using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform lookAt;
    [SerializeField] Vector3 offSet = new Vector3(0, 3, -6);

    [SerializeField] float sensitivity = 200f;
    [SerializeField] float minY = -40f;
    [SerializeField] float maxY = 70f;

    float currentX;
    float currentY;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void LateUpdate()
    {
        MouseInput();
    }

    void MouseInput()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        currentX += mouseX * sensitivity * Time.deltaTime;
        currentY -= mouseY * sensitivity * Time.deltaTime;

        currentY = Mathf.Clamp(currentY, minY, maxY);

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 dir = rotation * offSet;

        transform.position = lookAt.position + dir;
        transform.LookAt(lookAt.position);
    }
}
