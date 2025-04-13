using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

public abstract class BaseUnitSO : ScriptableObject
{
    [Header("Visuals")]

    public GameObject model;
    
    [Header("Health")]

    public int maxHealth;

    public int startHealth;

    public int currentHealth;
    
    [Header("Movement")]
    
    public int baseMoveRange;
    
    public int currentMoveRange;
    
    [Header("Status Effects")]
    
    public List<StatusEffectCounter> statusEffectCounters;
    
    //Properties
    
    //Can act if we don't have a shutdown status effect
    public bool CanAct => !statusEffectCounters.Exists(x => x.effectType ==
                                            eStatusEffectType.Shutdown);
    
    //Can move if range is greater than 0 and can act
    public bool CanMove => currentMoveRange > 0 && CanAct;
    
    //Invisible if camo counter
    public bool IsInvisible => !statusEffectCounters.Exists(x => x.effectType ==
                                                              eStatusEffectType.Camouflage);
    
    //Equal to the number of system damage counters
    public int ClashRollReduction
    {
        get
        {
            int value = 0;

            foreach (var statusEffectCounter in statusEffectCounters)
            {
                if (statusEffectCounter.effectType == eStatusEffectType.SystemDamage)
                {
                    value += statusEffectCounter.counters;
                }
            }

            return value;
        }
    }
    
    //Equal to number of barrier counters
    public int DefenseRollAddition
    {
        get
        {
            int value = 0;

            foreach (var statusEffectCounter in statusEffectCounters)
            {
                if (statusEffectCounter.effectType == eStatusEffectType.Barrier)
                {
                    value += statusEffectCounter.counters;
                }
            }

            return value;
        }
    }
    
    //Methods
    
    // TODO: gonna have to add range checks and shit here
    public virtual bool CheckCanBeTargeted(BaseUnitSO fromUnit)
    {
        return true;
    }
    
    public void OnTurnStart()
    {
        int currentEffectValue = 0;

        foreach (StatusEffectCounter effect in statusEffectCounters) //Effects counted and applied at beginning
        {
            currentEffectValue = effect.CountEffectCounters(true);

            switch (effect.effectType)
            {
                case eStatusEffectType.None:
                    break;
                case eStatusEffectType.Burn:
                    currentHealth -= currentEffectValue;
                    break;
                case eStatusEffectType.Corrosion:
                    currentHealth -= currentEffectValue;
                    break;
                case eStatusEffectType.Paralysis:
                    currentMoveRange = baseMoveRange - currentEffectValue;
                    break;
                case eStatusEffectType.SystemDamage:
                    break;
                case eStatusEffectType.Shutdown:
                    break;
                case eStatusEffectType.Barrier:
                    break;
                case eStatusEffectType.Camouflage:
                    break;
            }
        }

        CleanEffects();
    }
    
    public void OnTurnEnd()
    {
        foreach (StatusEffectCounter effect in statusEffectCounters) //Certain effects need to just be counted at end
        {
            effect.CountEffectCounters(false);
        }

        CleanEffects();
    }
    
    //Used for cleaning void effects or cases like defense move used
    private void CleanEffects(eStatusEffectType effectType = eStatusEffectType.None) 
    {
        List<StatusEffectCounter> cleanedEffects = new ();
        
        for (int i = 0; i < statusEffectCounters.Count; i++)
        {
            StatusEffectCounter currEffectCounter = statusEffectCounters[i];
            
            if (!(currEffectCounter.effectType == effectType || currEffectCounter.Void)) //Keep valid effects
            {
                cleanedEffects.Add(statusEffectCounters[i]);
            }
            else
            {
                if (currEffectCounter.effectType == eStatusEffectType.Paralysis) //Can move once paralysis removed
                {
                    currentMoveRange = baseMoveRange;
                }
            }
        }

        statusEffectCounters = cleanedEffects;
    }
}

public enum eStatusEffectType
{
    None,
    Burn,
    Corrosion,
    Paralysis,
    SystemDamage,
    Shutdown,
    Barrier,
    Camouflage
}

[Serializable]
public class StatusEffectCounter
{
    public eStatusEffectType effectType;
    
    public int counters;
    
    public bool Void => effectType == eStatusEffectType.None || counters <= 0;
    
    public StatusEffectCounter(eStatusEffectType effectType, int startCounters)
    {
        this.effectType = effectType;
        
        this.counters = startCounters;
    }
    
    public int CountEffectCounters(bool isTurnStart)
    {
        int value = 0;

        bool shouldCount = false;
        
        switch (effectType)
        {
            case eStatusEffectType.None: //Doesn't matter
                shouldCount = true;
                break;
            
            case eStatusEffectType.Burn: //Decrement on turn start
                
                for (int i = 0; i < counters; i++)
                {
                    value += new Random().Next(1, 3);
                }

                shouldCount = isTurnStart;

                break;
            
            case eStatusEffectType.Corrosion: //On turn start
                
                value = counters;

                int randCounterAddChange = new Random().Next(0, 1);
                
                if (randCounterAddChange == 1)
                {
                    counters++;
                }
                
                shouldCount = isTurnStart;
                
                break;
            
            case eStatusEffectType.Paralysis: //On turn start

                int paralysisStack = Mathf.FloorToInt(counters / 2);

                value = paralysisStack;
                
                shouldCount = isTurnStart;
                
                break;
            
            case eStatusEffectType.SystemDamage: //On turn start, used during combat calculations
                
                shouldCount = isTurnStart;
                
                break;
            
            case eStatusEffectType.Shutdown: //On turn end, used to deny mech turn
                
                shouldCount = !isTurnStart;
                
                break;
            
            case eStatusEffectType.Barrier: //All on defense skill, on turn start, used during combat calculations
                
                shouldCount = isTurnStart;
                
                break;
            case eStatusEffectType.Camouflage: //On turn start, used to deny engaging with
                
                shouldCount = isTurnStart;
                
                break;
        }
        
        if (shouldCount)
        {
            counters--;
        }
        
        if (counters <= 0)
        {
            counters = 0;
            effectType = eStatusEffectType.None;
        }
        
        if (shouldCount)
        {
            return value;
        }
        else
        {
            return 0;
        }
    }
}
