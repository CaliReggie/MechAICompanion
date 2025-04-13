using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eMechTeam
{
    AllRounder,
    Defender,
    Rushdown,
    Ranger
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

[CreateAssetMenu(fileName = "NewMechUnit", menuName = "ScriptableObjects/Units/MechUnitSO")]
public class MechUnitSO : BaseUnitSO
{
    [Header("Team Settings")]

    public eMechTeam team;
    
    [Header("Mech Type")]

    public eMechType mechType;
    
     // TODO: gonna have to add range checks and special case shit here
    public override bool CheckCanBeTargeted(BaseUnitSO other)
    {
        if (other.GetType() == this.GetType())
        {
            MechUnitSO otherMech = other as MechUnitSO;
            
            if (otherMech != null)
            {
                if (otherMech.team == this.team) return false;
            }
        }

        return true;
    }
}
