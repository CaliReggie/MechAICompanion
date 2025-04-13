using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eAlienType
{
    Grunt,
    Brute,
    Tank,
    Sniper,
    Support
}

[CreateAssetMenu(fileName = "NewAlienUnit", menuName = "ScriptableObjects/Units/AlienUnitSO")]
public class AlienUnitSO : BaseUnitSO
{
    [Header("Alien type")]

    public eAlienType alienType;
    
    //Methods
    
    // TODO: gonna have to add range checks and special case shit here
    public override bool CheckCanBeTargeted(BaseUnitSO other)
    {
        if (other.GetType() == this.GetType()) return false;

        return true;
    }
}
