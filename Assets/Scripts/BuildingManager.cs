using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    // Percentage of the building's height at which the character can start walking
    public float HeightPercent = 0.9f;

    // Reference to the SwingTPSController script
    private SwingTPSController _swinger;

    // Reference to the MeshRenderer component
    private MeshRenderer _meshRenderer;

    void Start()
    {
        // Find the SwingTPSController object in the scene
        _swinger = FindObjectOfType<SwingTPSController>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        // Check if the character's position is above the specified height
        if (
            _swinger != null
            && _swinger.transform.position.y >= _meshRenderer.bounds.max.y * HeightPercent
        )
        {
            // If above the height, tag the object as "Walkable"
            tag = "Walkable";
        }
        else
        {
            // Otherwise, tag the object as "Climbable"
            tag = "Climbable";
        }
    }
}
