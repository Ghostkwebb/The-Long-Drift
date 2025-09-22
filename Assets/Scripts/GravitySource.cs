using System.Collections.Generic;
using UnityEngine;

public class GravitySource : MonoBehaviour
{
    // A static list containing all active GravitySource instances.
    public static List<GravitySource> AllSources = new List<GravitySource>();

    [SerializeField] public float mass = 1000f;

    private void OnEnable()
    {
        // Add this instance to the list when it becomes active.
        if (!AllSources.Contains(this))
        {
            AllSources.Add(this);
        }
    }

    private void OnDisable()
    {
        // Remove this instance from the list when it becomes inactive.
        AllSources.Remove(this);
    }
}