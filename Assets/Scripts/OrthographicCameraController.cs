using System;
using UnityEngine;

public class OrthographicCameraController : MonoBehaviour
{
    public float orthographicSize = 100.0f;   // Initial orthographic size (zoom level)
    public float rotationSpeed = 5.0f;        // Speed at which the camera rotates
    public float rotationSpeedMobile = 1.0f;  // Speed at which the camera rotates
    public float zoomSpeed = 0.1f;            // Exponential zoom speed
    public float zoomSpeedMobile = 0.05f;            // Exponential zoom speed
    public float minZoomSize = 0.001f;        // Minimum zoom size (close to 0 but never 0 to avoid errors)
    public float maxZoomSize = 1e5f;        // Minimum zoom size (close to 0 but never 0 to avoid errors)
    private Vector3 currentRotation;           // To keep track of the current rotation

    void Start()
    {
        // Initialize the camera's rotation based on the starting angles
        currentRotation = new Vector3(30, 45, 0); // Set some initial angles for the camera (optional)
        UpdateCameraPosition();
    }

    void Update()
    {
        // Check for desktop or mobile inputs
        if (Input.touchSupported && Input.touchCount > 0)
        {
            HandleTouchInput();
        }
        else
        {
            HandleMouseInput();
        }

        // Update the camera's position based on any changes
        UpdateCameraPosition();
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButton(0)) // Left mouse button held
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // Rotate the camera based on the mouse movement
            currentRotation.x -= mouseY * rotationSpeed;
            currentRotation.y += mouseX * rotationSpeed;

            // Limit the vertical rotation
            currentRotation.x = Mathf.Clamp(currentRotation.x, -80, 80);
        }

        // Exponential zooming with the scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        // Apply exponential zooming
        orthographicSize *= Mathf.Pow(1f - zoomSpeed, scroll);

        // Prevent the orthographic size from going below 0 (or very close to it)
        orthographicSize = Mathf.Clamp(orthographicSize, minZoomSize, maxZoomSize);

        Camera.main.orthographicSize = orthographicSize;
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 1) // Single touch for rotation
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                // Rotate the camera based on touch movement
                float deltaX = touch.deltaPosition.x * rotationSpeedMobile * 0.1f;
                float deltaY = touch.deltaPosition.y * rotationSpeedMobile * 0.1f;

                currentRotation.x -= deltaY;
                currentRotation.y += deltaX;

                // Limit the vertical rotation
                currentRotation.x = Mathf.Clamp(currentRotation.x, -80, 80);
            }
        }
        else if (Input.touchCount == 2) // Pinch to zoom
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            // Find the position in the previous frame of each touch
            Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
            Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

            // Calculate the magnitude of the vector (distance) between the touches
            float prevTouchDeltaMag = (touch1PrevPos - touch2PrevPos).magnitude;
            float touchDeltaMag = (touch1.position - touch2.position).magnitude;

            // Find the difference in the distances between each frame
            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            // Only update zoom if touches are in the appropriate phase
            if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved)
            {
                // Exponential zooming based on the pinch gesture
                orthographicSize *= Mathf.Pow(1f - zoomSpeedMobile, deltaMagnitudeDiff * 0.01f);

                // Prevent the orthographic size from going below 0 (or very close to it)
                orthographicSize = Mathf.Clamp(orthographicSize, minZoomSize, maxZoomSize);

                Camera.main.orthographicSize = orthographicSize;
            }
        }
    }

    void UpdateCameraPosition()
    {
        // Convert the current rotation into a direction vector
        Quaternion rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);
        Vector3 direction = rotation * Vector3.forward;

        // Set the camera's position based on the direction, while maintaining the same distance from the target point
        transform.position = direction * -orthographicSize;  // Camera's distance in orthographic mode is relative to orthographic size

        // Always look at the origin (0, 0, 0)
        transform.LookAt(Vector3.zero);
    }
}
