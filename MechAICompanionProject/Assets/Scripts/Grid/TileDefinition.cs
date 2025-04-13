using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public enum eTileType
{
    Base,
    Hq,
    MechSpawn,
    Heal,
    Shop,
    Hive
}


[Serializable] //Create in an editor
public class TileDefinition
{
    public string name;
    
    public Tile correspondingTileAsset;
    
    public eTileType tileType;
}

[Serializable] //Create at parse time
public class GridTile
{
    [System.Flags] public enum eNeighbours
    {
        none = 0,
        up = 1,
        upRight = 2,
        downRight = 4,
        down = 8,
        downLeft = 16,
        upLeft = 32,
    }
    
    public readonly Dictionary<eNeighbours, Vector3Int> OddFlaxHexOffsets = new ()
    {
        {eNeighbours.up, new Vector3Int(1, 0, 0)},
        {eNeighbours.upRight, new Vector3Int(1, 1, 0)},
        {eNeighbours.downRight, new Vector3Int(0, 1, 0)},
        {eNeighbours.down, new Vector3Int(-1, 0, 0)},
        {eNeighbours.downLeft, new Vector3Int(0, -1, 0)},
        {eNeighbours.upLeft, new Vector3Int(1, -1, 0)}
    };
    
    public readonly Dictionary<eNeighbours, Vector3Int> EvenFlaxHexOffsets = new ()
    {
        {eNeighbours.up, new Vector3Int(1, 0, 0)},
        {eNeighbours.upRight, new Vector3Int(0, 1, 0)},
        {eNeighbours.downRight, new Vector3Int(-1, 1, 0)},
        {eNeighbours.down, new Vector3Int(-1, 0, 0)},
        {eNeighbours.downLeft, new Vector3Int(-1, -1, 0)},
        {eNeighbours.upLeft, new Vector3Int(0, -1, 0)}
    };
    
    [Header("Identification")]
    
    public string name;

    public TileDefinition tileDefinition;
    
    [Header("Positioning")]
    
    public Vector3Int gridPosition;
    
    public Vector3 worldPosition;
    
    [Header("Neighbor Information")]
    
    public eNeighbours neighbours;
    
    [Header("Debug")]
    
    public bool isTargetPosition;

    [Header("Dynamic")]

    public int currentArtifacts;

    public BaseUnitSO currentUnit;
    
    public GameObject currentUnitModel;
    
    //Properties

    public Tile Tile => tileDefinition.correspondingTileAsset;

    public eTileType TileType => tileDefinition.tileType;
    
    public bool Occupied => currentUnit != null;
    
    public bool HasModel => currentUnitModel != null;

    public bool HasArtifact => currentArtifacts > 0;
    
    public GridTile (TileDefinition setTileDef, Vector3Int setGridPos, Tilemap setTilemap)
    {
        this.tileDefinition = setTileDef;
        
        this.name = (tileDefinition.name + " (" + setGridPos.x + ", " + setGridPos.y + ")");
        
        this.gridPosition = setGridPos;
        
        this.worldPosition = setTilemap.GetCellCenterWorld(setGridPos);
    }
    
    public void PlaceUnit(BaseUnitSO unit)
    {
        if (Occupied) return;

        currentUnit = unit;
        
        Vector3 positionOffset = new Vector3(0, -0.25f, 0);
        
        Vector3 location = worldPosition + positionOffset;
        
        currentUnitModel = GameObject.Instantiate(currentUnit.model, location, Quaternion.identity);
    }
    
    public void RemoveUnit()
    {
        if (!Occupied) return;

        currentUnit = null;
        
        if (currentUnitModel != null)
        {
            GameObject.Destroy(currentUnitModel);
            currentUnitModel = null;
        }
    }
    
    public void ChangeArtifacts(int numArtifacts)
    {
        currentArtifacts += numArtifacts;

        if (currentArtifacts < 0) currentArtifacts = 0;
    }
    
    public static void SetGridTileNeighbors(List<GridTile> inGrid, Tilemap tileMap, out List<GridTile> outGrid)
    {
        if (tileMap != null && inGrid != null && inGrid.Count != 0)
        {
            foreach (GridTile tile in inGrid)
            {
                tile.neighbours = GridTile.eNeighbours.none;
                
                foreach (eNeighbours neighbour in Enum.GetValues(typeof(eNeighbours)))
                {
                    if (neighbour == eNeighbours.none) { continue; }

                    Vector3Int offset = Vector3Int.zero;
                    
                    if (Mathf.Abs(tile.gridPosition.y) % 2 == 0)
                    {
                        offset = tile.EvenFlaxHexOffsets[neighbour];
                    }
                    else
                    {
                        offset = tile.OddFlaxHexOffsets[neighbour];
                    }
                    
                    Vector3Int neighbourPosition = tile.gridPosition + offset;
                    
                    if (inGrid.Exists(t => t.gridPosition == neighbourPosition))
                    {
                        tile.neighbours |= neighbour;
                    }
                }
            }
            
            outGrid = inGrid;
        }
        else
        {
            Debug.LogError("Error in inputted TileGrid! (TileDefinition)");

            outGrid = null;
        }
    }
}