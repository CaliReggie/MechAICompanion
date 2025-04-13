using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eGameState
{
    Load,
    Place,
    Play,
    Over
}

public enum eTurnState
{
    P1,
    P2,
    P3,
    P4,
    Alien
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    
    //Private or Non Serialized Below

    public event Action<eGameState> OnGameStateChanged;

    public event Action<eTurnState> OnTurnStateChanged;
    
    [field: SerializeField] public eGameState GameState { get; private set; }
    
    [field: SerializeField] public eTurnState TurnState { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    private void ThisOnGameStateChange(eGameState gameState)
    {
        
    }
    
    private void ThisOnTurnStateChange(eTurnState turnState)
    {
        
    }
    
    public void SetGameState(eGameState toState)
    {
        
    }
    
    public void CycleTurnState()
    {
        
    }
}
