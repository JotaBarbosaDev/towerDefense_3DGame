using System;
using UnityEngine;

[Serializable]
public class SimpleEnemyArchetype
{
    public string displayName;
    public GameObject visualPrefab;
    public EnemyMovementKind movementKind;
    [Min(1)] public int count = 1;
    [Min(0.1f)] public float health = 5f;
    [Min(0.1f)] public float moveSpeed = 3f;
    [Min(1)] public int goalDamage = 1;
    [Min(0)] public int currencyReward = 1;
    [Min(0f)] public float spawnInterval = 0.8f;
    public Vector3 visualOffset = Vector3.zero;
    public Vector3 visualScale = Vector3.one;
    public Color visualTint = Color.white;
}

public enum EnemyMovementKind
{
    Ground,
    Air
}
