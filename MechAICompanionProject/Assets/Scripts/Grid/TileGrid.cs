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

    [Header("Pre Runtime Settings")]

    [SerializeField] private TileDefinition[] tileDefinitions;
    
    [Header("Debug")]
    
    [Space]
    
    [SerializeField] private bool showAllPositions;

    [SerializeField] private bool showTargetPositions;
    
    [Header("Tile Grid Dynamic")]

    [Tooltip("Exists for serialization purposes")]
    [SerializeField] private bool emptyBool;
    
    [field: SerializeField] public List<GridTile> GridTiles { get; private set; }
    
    //Private, or Non Serialized Below
    public Tilemap TileMap { get; private set; }
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        
        Instance = this;
        
        if (tileDefinitions == null || tileDefinitions.Length < 1)
        {
            Debug.LogError("Tile definitions not set in Tile Grid!");

            enabled = false;
            
            return;
        }
        
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
        
        ReadTileMap();

        List<GridTile> neighboredTiles = new List<GridTile>(); 
        
        GridTile.SetGridTileNeighbors(GridTiles, TileMap, out neighboredTiles);
        
        if (neighboredTiles != null)
        {
            GridTiles = neighboredTiles;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        
        if (GridTiles != null)
        {
            GridTiles.Clear();
        }
    }

    private void ReadTileMap()
    {
        
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
                    foreach (var tileDefinition in tileDefinitions)
                    {
                        if (tileDefinition.correspondingTileAsset == currTile)
                        {
                            GridTiles.Add(new GridTile(tileDefinition, position, TileMap));
                            
                            break;
                        }
                    }
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
