using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum EnemyClass
{
    Grunt, // Basic / aggressive
    Commander, // Aggressive
    Tank, // Front lines, less movement
    Ranger, // Near hive, ranged
    Support // Sticks around a target
}

public enum StatusEffect
{
    None,
    Burn,
    Corrosion,
    Paralysis,
    SystemDamage,
    Shutdown,
    Barrier,
    Camo,
}

public class Enemy : MonoBehaviour
{
    [Header("Class")]
    
    public EnemyClass enemyClass;
    
    [Header("Health")]
    
    public int currentHealth;

    public int maxHealth;

    [Header("Location")] 
    
    public Vector2 gridLocation;

    [Header("Status Effect")] 
    
    public StatusEffect statusEffect = StatusEffect.None;
    
    [Header("Abilities")]
    
    public List<Ability> abilities = new ();
    
    //Properties
    
    public float HealthPercentage => (float) currentHealth / maxHealth;
    
    public List<Enemy> FriendliesInRange { get; set; } = new ();
    
    public List<Mech> MechsInRange { get; set; } = new ();
}

[Serializable]
public class Ability
{
    public string name;

    public int range;

    public int value;
    
    public StatusEffect statusEffect = StatusEffect.None;
}
