using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class Minimap : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform planetsParent;
    [SerializeField] private RectTransform iconContainer;

    [Header("Icon Prefabs")]
    [SerializeField] private Image playerIconPrefab;
    [SerializeField] private Image planetIconPrefab;
    [SerializeField] private Image destinationIconPrefab;
    [SerializeField] private Image checkpointIconPrefab;

    [Header("Map Settings")]
    [Tooltip("The real-world height (Y-axis) that the minimap will display. The width will be calculated automatically based on the UI's shape.")]
    [SerializeField] private float mapWorldHeight = 300f;
    [SerializeField] private float iconRotationOffset = -90f;

    [Header("Opacity Control")]
    [Range(0f, 1f)]
    [SerializeField] private float fadedOpacity = 0.3f;
    [SerializeField] private float opacityFadeSpeed = 5f;

    private List<MinimapIcon> planetIcons = new List<MinimapIcon>();
    private Image playerIcon;
    private RectTransform panelRectTransform;
    private CanvasGroup canvasGroup;

    private class MinimapIcon { public Image icon; public Transform worldTarget; }

    void Start()
    {
        panelRectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        InstantiateIcons();
    }

    void LateUpdate()
    {
        if (playerTransform == null) return;
        UpdateIconPositions();
        UpdatePanelOpacity();
    }

    private void InstantiateIcons()
    {
        if (playerTransform != null && playerIconPrefab != null)
        {
            playerIcon = Instantiate(playerIconPrefab, iconContainer);
        }

        if (planetsParent != null)
        {
            foreach (GravitySource source in planetsParent.GetComponentsInChildren<GravitySource>())
            {
                Image prefabToUse = planetIconPrefab; // Default to the normal planet icon.

                // This logic is now more specific.
                if (source.isDestination)
                {
                    prefabToUse = destinationIconPrefab;
                }
                else if (source.isCheckpoint)
                {
                    prefabToUse = checkpointIconPrefab;
                }

                if (prefabToUse == null) continue; // Skip if no suitable prefab is assigned.

                Image newIcon = Instantiate(prefabToUse, iconContainer);
                planetIcons.Add(new MinimapIcon { icon = newIcon, worldTarget = source.transform });
            }
        }
    }

    private void UpdateIconPositions()
    {
        // 1. The player is always at the center of the minimap.
        playerIcon.rectTransform.anchoredPosition = Vector2.zero;
        playerIcon.rectTransform.rotation = Quaternion.Euler(0, 0, playerTransform.eulerAngles.z + iconRotationOffset);

        // 2. Calculate the dimensions of our world-space "view rectangle" based on the UI's aspect ratio.
        float uiWidth = iconContainer.rect.width;
        float uiHeight = iconContainer.rect.height;
        float mapWorldWidth = mapWorldHeight * (uiWidth / uiHeight);

        Vector2 playerPos = playerTransform.position;

        foreach (var item in planetIcons)
        {
            Vector2 targetPos = item.worldTarget.position;
            Vector2 diff = targetPos - playerPos; // Vector from player to planet

            // 3. Calculate the planet's position as a percentage of the world-space view rectangle.
            float percentX = diff.x / (mapWorldWidth / 2f);
            float percentY = diff.y / (mapWorldHeight / 2f);

            // 4. Clamp these percentages to a -1 to 1 range. This keeps the icons on the map.
            percentX = Mathf.Clamp(percentX, -1f, 1f);
            percentY = Mathf.Clamp(percentY, -1f, 1f);

            // 5. Map these final percentages to the UI's dimensions.
            Vector2 iconPosition = new Vector2(
                percentX * (uiWidth / 2f),
                percentY * (uiHeight / 2f)
            );

            item.icon.rectTransform.anchoredPosition = iconPosition;
        }
    }

    private void UpdatePanelOpacity()
    {
        Rect panelScreenRect = new Rect(panelRectTransform.position.x, panelRectTransform.position.y, panelRectTransform.sizeDelta.x, panelRectTransform.sizeDelta.y);
        Vector2 playerScreenPos = Camera.main.WorldToScreenPoint(playerTransform.position);
        float targetOpacity = panelScreenRect.Contains(playerScreenPos) ? fadedOpacity : 1.0f;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetOpacity, Time.deltaTime * opacityFadeSpeed);
    }
}