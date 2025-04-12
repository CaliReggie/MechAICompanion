using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eTeam
{
    AllRounder,
    Defender,
    Rushdown,
    Ranger,
    Alien
}

public enum eMechType
{
    Swordfighter,
    Shield,
    Medic,
    Rifleman,
    CombatKnife,
    EnergySword,
    Railgunner
}

public enum eAlienType
{
    Grunt,
    Brute,
    Tank,
    Sniper,
    Support
}

[CreateAssetMenu(fileName = "New Unit", menuName = "ScriptableObjects/UnitSO")]
public class UnitSO : ScriptableObject
{
    [Header("Team Settings")]

    public eTeam team;

    [Header("Mech Classification")]

    public eMechType mechClass;

    [Header("Alien Classification")]

    public eAlienType alienClass;

}
