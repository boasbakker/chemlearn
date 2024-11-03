using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 3.0f; // Initial speed of camera movement
    public float minSpeed = 0.0f; // Minimum speed of camera movement
    public float maxSpeed = 100000.0f; // Maximum speed of camera movement
    public float lookSpeed = 2.0f; // Speed of camera rotation
    public float scrollSpeedAdjustment = 5.0f; // Speed adjustment for scrolling
    private Vector3 moveDirection;

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Update()
    {
        // Handle camera movement
        MoveCamera();

        // Handle camera rotation
        RotateCamera();

        // Handle speed adjustment with mouse scroll
        AdjustSpeedWithScroll();
    }

    void MoveCamera()
    {
        // Reset move direction
        moveDirection = Vector3.zero;

        // Get input for horizontal movement (X and Z only)
        if (Input.GetKey(KeyCode.W))
            moveDirection += Vector3.forward; // Move forward
        if (Input.GetKey(KeyCode.S))
            moveDirection -= Vector3.forward; // Move backward
        if (Input.GetKey(KeyCode.A))
            moveDirection -= Vector3.right; // Move left
        if (Input.GetKey(KeyCode.D))
            moveDirection += Vector3.right; // Move right

        // Normalize direction and move the camera strictly in XZ plane
        moveDirection = transform.TransformDirection(moveDirection); // Convert to world space
        moveDirection.y = 0; // Prevent Y movement

        // Move the camera
        transform.position += moveSpeed * Time.deltaTime * moveDirection.normalized;

        // Handle vertical movement (Y only)
        if (Input.GetKey(KeyCode.Space))
            transform.position += moveSpeed * Time.deltaTime * Vector3.up; // Move up
        if (Input.GetKey(KeyCode.LeftShift))
            transform.position += moveSpeed * Time.deltaTime * Vector3.down; // Move down
    }

    void RotateCamera()
    {
        // Handle mouse drag for rotation
        if (Input.GetMouseButton(0)) // Left mouse button
        {
            // Get mouse movement
            rotationX += Input.GetAxis("Mouse X") * lookSpeed;
            rotationY -= Input.GetAxis("Mouse Y") * lookSpeed;

            // Clamp vertical rotation
            rotationY = Mathf.Clamp(rotationY, -80f, 80f);
            transform.eulerAngles = new Vector3(rotationY, rotationX, 0);
        }
    }

    void AdjustSpeedWithScroll()
    {
        // Adjust movement speed with mouse scroll wheel
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            moveSpeed += scrollInput * scrollSpeedAdjustment;
            moveSpeed = Mathf.Clamp(moveSpeed, minSpeed, maxSpeed); // Clamp the speed within min and max limits
        }
    }
}
