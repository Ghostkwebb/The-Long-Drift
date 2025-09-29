using UnityEngine;

public class SeamlessBackground : MonoBehaviour
{
    public Transform cameraTransform;
    [Range(0f, 1f)]
    public float parallaxMultiplier;

    private Vector3 lastCameraPosition;
    private float tileWidth, tileHeight;

    void Start()
    {
        if (cameraTransform == null) cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;

        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        tileWidth = sprite.texture.width / sprite.pixelsPerUnit;
        tileHeight = sprite.texture.height / sprite.pixelsPerUnit;
    }

    void LateUpdate()
    {
        // Apply parallax movement
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        transform.position += deltaMovement * parallaxMultiplier;
        lastCameraPosition = cameraTransform.position;

        // --- Updated Conveyor Belt Logic for a 3x3 Grid ---
        // If the tile has moved too far left of the camera...
        if (cameraTransform.position.x - transform.position.x >= tileWidth * 1.5f)
        {
            //...move it three widths to the right side.
            transform.position = new Vector3(transform.position.x + (tileWidth * 3f), transform.position.y, transform.position.z);
        }
        // If the tile has moved too far right...
        else if (cameraTransform.position.x - transform.position.x <= -tileWidth * 1.5f)
        {
            //...move it three widths to the left side.
            transform.position = new Vector3(transform.position.x - (tileWidth * 3f), transform.position.y, transform.position.z);
        }

        // Repeat for vertical wrapping
        if (cameraTransform.position.y - transform.position.y >= tileHeight * 1.5f)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + (tileHeight * 3f), transform.position.z);
        }
        else if (cameraTransform.position.y - transform.position.y <= -tileHeight * 1.5f)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - (tileHeight * 3f), transform.position.z);
        }
    }
}