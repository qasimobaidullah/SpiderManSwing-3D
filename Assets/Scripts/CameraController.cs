using UnityEngine;

// This script controls the camera's movement and rotation to follow the player.
// It includes zoom functionality and rotational constraints to enhance gameplay experience.
public class CameraController : MonoBehaviour
{
    // Determines if the camera can zoom in and out
    public bool canZoom = true;

    // Sensitivity for camera rotation and zoom adjustments
    public float sensitivity = 5f;

    // Limits for the vertical rotation of the camera (in degrees)
    public Vector2 cameraLimit = new Vector2(-45, 40);

    // Variables to track mouse input and vertical offset
    float mouseX;
    float mouseY;
    float offsetDistanceY;

    // Reference to the player's Transform component
    Transform player;

    void Start()
    {
        // Finds the player object in the scene by its tag and gets its Transform
        player = GameObject.FindWithTag("Player")?.transform;

        // Check if the player was found, and handle the situation if it's null
        if (player == null)
        {
            Debug.LogError(
                "Player object not found! Ensure the Player GameObject has the correct tag."
            );
            enabled = false; // Disable the script to prevent further errors
            return;
        }

        // Set the initial vertical offset based on the camera's starting position
        offsetDistanceY = transform.position.y;
    }

    void Update()
    {
        // Keeps the camera positioned above the player, maintaining the vertical offset
        transform.position = player.position + new Vector3(0, offsetDistanceY, 0);

        // Handles zoom functionality if enabled and mouse scroll wheel input is detected
        if (canZoom && Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            Camera.main.fieldOfView -= Input.GetAxis("Mouse ScrollWheel") * sensitivity * 2;
        }

        // Accumulate mouse movement for rotation
        mouseX += Input.GetAxis("Mouse X") * sensitivity;
        mouseY += Input.GetAxis("Mouse Y") * sensitivity;

        // Clamp the vertical rotation to stay within specified limits
        mouseY = Mathf.Clamp(mouseY, cameraLimit.x, cameraLimit.y);

        // Apply the calculated rotation to the camera
        transform.rotation = Quaternion.Euler(-mouseY, mouseX, 0);
    }
}
