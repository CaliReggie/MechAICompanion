using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class TileGrid : MonoBehaviour
{
    public static TileGrid Instance { get; private set; }
    
    [Header("Tile Grid Generation")]
    
    [Space]
    
    [SerializeField] private bool clearGridTiles;
    
    [SerializeField] private bool parseToGridTiles;
    
    [field: SerializeField] public List<GridTile> GridTiles { get; private set; }
    
    [Header("Tile Grid Settings")]
    
    [Space]
    
    [SerializeField] private Tile[] ignoredTiles;
    
    [Header("Position Debug")]
    
    [Space]
    
    [SerializeField] private bool showAllPositions;

    [SerializeField] private bool showTargetPositions;
    
    [Header("Pathfinding Debug")]
    
    [Space]
    
    [SerializeField] private Vector3Int startGridPos;

    [SerializeField] private Vector3Int endGridPos;
    
    [Space]
    
    [SerializeField] private bool findPath;
    
    [SerializeField] private bool clearPath;
    
    [Space]
    
    [SerializeField] private List<GridTile> currentPath;
    
    //Private, or Non Serialized Below
    public Tilemap TileMap { get; private set; }

    private void OnValidate()
    {
        if (clearGridTiles)
        {
            if (GridTiles != null)
            {
                GridTiles.Clear();
            }
            
            clearGridTiles = false;
        }
        
        if (parseToGridTiles)
        {
            ReadTileMap();
            
            GridTiles = GridTile.SetGridTileNeighbors(GridTiles, TileMap, ignoredTiles);
            
            parseToGridTiles = false;
        }
        
        if (findPath)
        {
            currentPath = Pathfinding.FindPath(startGridPos, endGridPos, GridTiles);
            
            findPath = false;
        }
        
        if (clearPath)
        {
            if (currentPath != null)
            {
                currentPath.Clear();
            }
            
            currentPath = null;
            
            clearPath = false;
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
        }
        
        Instance = this;
        
        
        if (TileMap == null)
        {
            TileMap = GetComponent<Tilemap>();
        }
        
        if (GridTiles == null)
        {
            GridTiles = new List<GridTile>();
        }
        else
        {
            GridTiles.Clear();
        }
        
        if (ignoredTiles == null)
        {
            ignoredTiles = Array.Empty<Tile>();
        }
        
        ReadTileMap();
        
        GridTiles = GridTile.SetGridTileNeighbors(GridTiles, TileMap, ignoredTiles);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void ReadTileMap()
    {
        if (TileMap == null)
        {
            TileMap = GetComponent<Tilemap>();
        }
        
        if (GridTiles != null)
        {
            GridTiles.Clear();
        }
        
        GridTiles = new List<GridTile>();
        
        BoundsInt bounds = TileMap.cellBounds;
        Tile currTile = null;

        for (int x = bounds.x; x < bounds.xMax; x++)
        {
            for (int y = bounds.y; y < bounds.yMax; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                
                TryGetTile(position, out currTile);
                
                if (currTile != null)
                {
                    if (ignoredTiles != null && Array.Exists(ignoredTiles, t => t == currTile))
                    {
                        continue;
                    }
                    
                    GridTiles.Add(new GridTile(currTile, position, TileMap));
                }
            }
        }
        
        return;
        
        void TryGetTile(Vector3Int position, out Tile tile) //Check if tilemap is null before using
        {
            tile = TileMap.GetTile<Tile>(position);
        }
    }
    
    public Vector3 ClosestGridPosAtWorldPos(Vector3 worldPos)
    {
        Vector3Int gridPos = TileMap.WorldToCell(worldPos);
        
        GridTile closestTile = GridTiles.Find(t => t.gridPosition == gridPos);
        
        if (closestTile != null)
        {
            return closestTile.worldPosition;
        }
        
        return Vector3.zero;
    }
    
    public GridTile ClosestGridTileAtWorldPos(Vector3 worldPos)
    {
        Vector3Int gridPos = TileMap.WorldToCell(worldPos);
        
        GridTile closestTile = GridTiles.Find(t => t.gridPosition == gridPos);
        
        if (closestTile != null)
        {
            return closestTile;
        }
        
        return null;
    }
    
        
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        
        if (showAllPositions)
        {
            ShowAllPositions();
        }
        
        if (showTargetPositions)
        {
            ShowTargetPositions();
        }
        
        if (currentPath != null)
        {
            Gizmos.color = Color.green;
            foreach (var tile in currentPath)
            {
                Gizmos.DrawWireCube(tile.worldPosition, Vector3.one * 0.3f);
            }
        }
        
        return;
        
        void ShowAllPositions()
        {
            if (GridTiles == null || GridTiles.Count == 0) { return; }
            
            if (TileMap == null)
            {
                TileMap = GetComponent<Tilemap>();
            }
            
            foreach (GridTile tile in GridTiles)
            {
                Gizmos.DrawSphere(tile.worldPosition, 0.25f);
            }
        }
        
        void ShowTargetPositions()
        {
            if (GridTiles == null || GridTiles.Count == 0) { return; }
            
            if (TileMap == null)
            {
                TileMap = GetComponent<Tilemap>();
            }

            foreach (GridTile tile in GridTiles)
            {
                if (tile.isTargetPosition)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(tile.worldPosition, 0.25f);
                }
            }
        }
        
    }
}
