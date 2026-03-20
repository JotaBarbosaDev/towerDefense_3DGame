using System;
using TowerDefense.Towers;
using UnityEngine;

[Serializable]
public class SimpleTowerArchetype
{
    public string displayName;
    public Tower towerPrefab;
    [Min(0)] public int cost = 100;
    public Vector3 position;
    public Vector3 eulerAngles;
    [Range(0, 2)] public int level = 0;
    [Min(0.1f)] public float damage = 1f;
    [Min(0.1f)] public float fireRate = 1f;
    public TowerTargetMode targetMode = TowerTargetMode.All;
    public Color uiColor = Color.white;
}

public enum TowerTargetMode
{
    GroundOnly,
    AirOnly,
    All
}
