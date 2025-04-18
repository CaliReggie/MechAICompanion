using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GridTileInteraction : MonoBehaviour
{
    public enum eSelectionState
    {
        General,
        Placing,
        Pathfinding
    }
    
    [Header("Selection")]

    [SerializeField] private GameObject outlinePrefab;
    
    [Header("All Rounder Button Definitions")]
    
    [SerializeField] private ButtonDefinition[] allRounderButtonDefinitions;
    
    [Header("Defender Button Definitions")]
    
    [SerializeField] private ButtonDefinition[] defenderButtonDefinitions;
    
    [Header("Rushdown Button Definitions")]
    
    [SerializeField] private ButtonDefinition[] rushdownButtonDefinitions;
    
    [Header("Ranger Button Definitions")]
    
    [SerializeField] private ButtonDefinition[] rangerButtonDefinitions;
    
    [Header("Alien Button Definitions")]
    
    [SerializeField] private ButtonDefinition[] alienButtonDefinitions;
    
    [Header("Debug")]
    
    [SerializeField] private eSelectionState selectionState;
    
    //Private, or Non Serialized Below
    
    private GridTile _currentSelectedTile;
    
    private List<GameObject> _currentOutlines;
    
    private List<GridTile> _currentTilePath = new ();

    private List<BaseUnitSO> _units = new();
    
    //Properties
    
    private ButtonDefinition[] AllMechButtonDefinitions
    {
        get
        {
            List<ButtonDefinition> allButtonDefinitions = new List<ButtonDefinition>();
            
            allButtonDefinitions.AddRange(allRounderButtonDefinitions);
            allButtonDefinitions.AddRange(defenderButtonDefinitions);
            allButtonDefinitions.AddRange(rushdownButtonDefinitions);
            allButtonDefinitions.AddRange(rangerButtonDefinitions);

            return allButtonDefinitions.ToArray();
        }
    }

    private void Start()
    {
        if (outlinePrefab == null)
        {
            Debug.LogError("Selection Outline Prefab is not assigned!");
            return;
        }
        if (TileGrid.Instance == null)
        {
            Debug.LogError("TileGrid Instance is not assigned!");
            return;
        }

        _currentOutlines = new List<GameObject>();

        _currentTilePath = new List<GridTile>();

        _units = new List<BaseUnitSO>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            switch (selectionState)
            {
                case eSelectionState.General:
                    
                    TrySelect(mousePos);
                    
                    break;
                
                case eSelectionState.Placing:

                    TrySelect(mousePos);
                    
                    break;
                
                case eSelectionState.Pathfinding:

                    TrySelect(mousePos);
                    
                    break;
            }
        }
        
        if (Input.GetMouseButtonUp(1))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            switch (selectionState)
            {
                case eSelectionState.General:
                    break;
                
                case eSelectionState.Pathfinding:

                    TryPathfindToPoint(mousePos);
                    
                    break;
            }
            
        }
    }
    
    private bool TrySelect( Vector3 worldPos)
    {
        ClearSelections();
        
        _currentSelectedTile = TileGrid.Instance.ClosestGridTileAtWorldPos(worldPos);

        if (_currentSelectedTile == null) return false;

        _currentOutlines.Add(Instantiate(outlinePrefab, _currentSelectedTile.worldPosition, Quaternion.identity));

        return true;
    }
    
    private void ClearSelections()
    {
        if (_currentOutlines != null)
        {
            foreach(var outline in _currentOutlines)
            {
                Destroy(outline.gameObject);
            }
            
            _currentOutlines.Clear();
        }
        
        _currentSelectedTile = null;
        
        if (_currentTilePath != null)
        {
            _currentTilePath.Clear();
        }
    }
    
    public void SetSelection()
    {
        selectionState = eSelectionState.General;

        ClearSelections();
    }
    
    public void SetPlacing()
    {
        selectionState = eSelectionState.Placing;

        ClearSelections();
    }
    
    public void TryPlaceUnit(BaseUnitSO unit)
    {
        if (selectionState != eSelectionState.Placing) return;
        
        if (_currentSelectedTile == null) return;
        
        if (_currentSelectedTile.Occupied) return;
        
        //Searching all button definitions for direct unit match
        //If valid, placing unit
        //Diasbling button interaction once placed
        
        if (unit.GetType() == Type.GetType("MechUnitSO"))
        {
            
            foreach (ButtonDefinition buttonDef in AllMechButtonDefinitions)
            {
                if (buttonDef.correspondingUnit == unit)
                {
                    buttonDef.correspondingButton.interactable = false;
                    
                    _currentSelectedTile.PlaceUnit(unit);
                    
                    break;
                }
            }
        }
        else if (unit.GetType() == Type.GetType("AlienUnitSO"))
        {
            foreach (ButtonDefinition buttonDef in alienButtonDefinitions)
            {
                if (buttonDef.correspondingUnit == unit)
                {
                    buttonDef.correspondingButton.interactable = false;
                    
                    _currentSelectedTile.PlaceUnit(unit);
                    
                    break;
                }
            }
        }
        else
        {
            Debug.LogError("Invalid Unit Type!");
            return;
        }
    }
    
    public void SetPathfinding()
    {
        selectionState = eSelectionState.Pathfinding;
        
        ClearSelections();
    }
    
    private void TryPathfindToPoint(Vector3 worldPos)
    {
        if (_currentSelectedTile == null || _currentSelectedTile.currentUnit == null) return;

        GridTile endTile = TileGrid.Instance.ClosestGridTileAtWorldPos(worldPos);
        
        if (endTile == null) return;

        List<GridTile> potentialPath = new List<GridTile>();

        Pathfinding.FindPath(TileGrid.Instance.GridTiles, _currentSelectedTile.gridPosition, endTile.gridPosition, 
            out potentialPath, _currentSelectedTile.currentUnit.currentMoveRange);
        
        if (potentialPath == null) return;

        OutlinePath(potentialPath);
    }
    
    private void OutlinePath(List<GridTile> tilePath)
    {
        if (tilePath == null || tilePath.Count < 1) return;

        ClearSelections();

        foreach (var tile in tilePath)
        {
            _currentOutlines.Add(Instantiate(outlinePrefab, tile.worldPosition, Quaternion.identity));
        }
        
        _currentSelectedTile = tilePath[0];
        
        _currentTilePath = tilePath;
    }
}

[Serializable]
public class ButtonDefinition
{
    public string name;
    
    public Button correspondingButton;
    
    public BaseUnitSO correspondingUnit;
    
    public bool AlreadyPlaced => correspondingButton.interactable == false;
}
