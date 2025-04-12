using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
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
    
    public Tile tile;
    
    [Header("Positioning")]
    
    public Vector3Int gridPosition;
    
    public Vector3 worldPosition;
    
    [Header("Neighbor Information")]
    
    public eNeighbours neighbours;
    
    [Header("Debug")]
    
    public bool isTargetPosition;
    
    public GridTile (Tile tile, Vector3Int gridPosition, Tilemap tilemap)
    {
        this.name = (tile.name + " (" + gridPosition.x + ", " + gridPosition.y + ")");
        
        this.tile = tile;
        
        this.gridPosition = gridPosition;
        
        this.worldPosition = tilemap.GetCellCenterWorld(gridPosition);
    }
    
    public static List<GridTile> SetGridTileNeighbors(List<GridTile> gridTiles, Tilemap tileMap, Tile[] ignoredTiles)
    {
        if (gridTiles == null || gridTiles.Count == 0) { return null; }

        if (tileMap == null) { return null; }
        
        if (ignoredTiles == null) { return null; }
        
        foreach (GridTile tile in gridTiles)
        {
            tile.neighbours = GridTile.eNeighbours.none;
            
            foreach (GridTile.eNeighbours neighbour in Enum.GetValues(typeof(GridTile.eNeighbours)))
            {
                if (neighbour == GridTile.eNeighbours.none) { continue; }

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
                
                if (gridTiles.Exists(t => t.gridPosition == neighbourPosition))
                {
                    Tile neighbourTile = tileMap.GetTile<Tile>(neighbourPosition);

                    if (neighbourTile == null || Array.Exists(ignoredTiles, t => t == neighbourTile))
                    {
                        Debug.Log("Tile is null or ignored");
                        
                        continue;
                    }
                    
                    tile.neighbours |= neighbour;
                }
            }
        }
        
        return gridTiles;
    }
}
