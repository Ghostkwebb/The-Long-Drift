using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
using System.Linq;

public class DynamicTargetGroup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineTargetGroup targetGroup;
    [SerializeField] private Transform playerTransform;
    [Tooltip("The parent object containing all planets in the level.")]
    [SerializeField] private Transform planetsParent;

    [Header("Tuning")]
    [SerializeField] private float framingDistance = 40f;
    [SerializeField] private float weightBlendSpeed = 2f;

    private const int PLAYER_TARGET_INDEX = 0;
    private const int PLANET_TARGET_INDEX = 1;

    private List<Transform> allPlanetTransforms;
    private Transform closestPlanet;

    void Start()
    {
        allPlanetTransforms = new List<Transform>();
        if (planetsParent != null)
        {
            foreach (Transform planet in planetsParent)
            {
                allPlanetTransforms.Add(planet);
            }
        }

        if (targetGroup != null)
        {
            targetGroup.Targets[PLAYER_TARGET_INDEX].Object = playerTransform;
            targetGroup.Targets[PLAYER_TARGET_INDEX].Weight = 1;
        }
    }

    void Update()
    {
        if (targetGroup == null || playerTransform == null || allPlanetTransforms.Count == 0)
        {
            return;
        }

        FindClosestPlanet();

        if (closestPlanet == null) return;

        targetGroup.Targets[PLANET_TARGET_INDEX].Object = closestPlanet;

        float distance = Vector3.Distance(playerTransform.position, closestPlanet.position);
        float targetWeight = (distance < framingDistance) ? 1f : 0f;

        float currentWeight = targetGroup.Targets[PLANET_TARGET_INDEX].Weight;
        float newWeight = Mathf.Lerp(currentWeight, targetWeight, Time.deltaTime * weightBlendSpeed);
        targetGroup.Targets[PLANET_TARGET_INDEX].Weight = newWeight;
    }

    private void FindClosestPlanet()
    {
        closestPlanet = allPlanetTransforms
            .OrderBy(p => Vector3.Distance(playerTransform.position, p.position))
            .FirstOrDefault();
    }
}