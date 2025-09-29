using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    public Transform cameraTransform;
    public GameObject backgroundTilePrefab; // The prefab with the ParallaxTile script

    void Start()
    {
        if (cameraTransform == null) cameraTransform = Camera.main.transform;

        // Get the size of our tile from the prefab's sprite renderer
        Sprite sprite = backgroundTilePrefab.GetComponent<SpriteRenderer>().sprite;
        float tileWidth = sprite.texture.width / sprite.pixelsPerUnit;
        float tileHeight = sprite.texture.height / sprite.pixelsPerUnit;

        // Create the initial 3x3 grid of tiles and parent them to this manager object
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector3 position = new Vector3(x * tileWidth, y * tileHeight, 10);
                Instantiate(backgroundTilePrefab, position, Quaternion.identity, transform);
            }
        }
    }
}