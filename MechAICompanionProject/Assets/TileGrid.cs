using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class TileGrid : MonoBehaviour
{
    [Header("Tile Grid Generation")]
    
    [SerializeField] private bool clearGridTiles;
    
    [SerializeField] private bool parseToGridTiles;
    
    [Header("Tile Grid Settings")]
    
    [SerializeField] private Tile[] ignoredTiles;
    
    [Header("Debug")]
    
    [SerializeField] private bool showAllPositions;

    [SerializeField] private bool showTargetPositions;
    
    [Header("Grid Tiles")]
    
    [SerializeField] private List<GridTile> gridTiles;
    
    //Private, or Non Serialized Below
    
    private Tilemap _tileMap;

    private void OnValidate()
    {
        if (clearGridTiles)
        {
            if (gridTiles != null)
            {
                gridTiles.Clear();
            }
            
            clearGridTiles = false;
        }
        
        if (parseToGridTiles)
        {
            ReadTileMap();
            
            SetGridTileNeighbors();
            
            parseToGridTiles = false;
        }
    }
    
    private void ReadTileMap()
    {
        if (_tileMap == null)
        {
            _tileMap = GetComponent<Tilemap>();
        }
        
        if (gridTiles != null)
        {
            gridTiles.Clear();
        }
        
        gridTiles = new List<GridTile>();
        
        BoundsInt bounds = _tileMap.cellBounds;
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
                    
                    gridTiles.Add(new GridTile(currTile, position, _tileMap));
                }
            }
        }
        
        return;
        
        void TryGetTile(Vector3Int position, out Tile tile) //Check if tilemap is null before using
        {
            tile = _tileMap.GetTile<Tile>(position);
        }
    }
    
    private void SetGridTileNeighbors()
    {
        if (gridTiles == null || gridTiles.Count == 0) { return; }
        
        if (_tileMap == null)
        {
            _tileMap = GetComponent<Tilemap>();
        }
        
        foreach (GridTile tile in gridTiles)
        {
            tile.neighbours = GridTile.eNeighbours.none;
            
            foreach (GridTile.eNeighbours neighbour in Enum.GetValues(typeof(GridTile.eNeighbours)))
            {
                if (neighbour == GridTile.eNeighbours.none) { continue; }
                
                Vector3Int offset = tile.FlatHexOffsets[neighbour];
                
                Vector3Int neighbourPosition = tile.gridPosition + offset;
                
                if (gridTiles.Exists(t => t.gridPosition == neighbourPosition))
                {
                    Tile neighbourTile = _tileMap.GetTile<Tile>(neighbourPosition);

                    if (neighbourTile == null || ignoredTiles != null &&
                        Array.Exists(ignoredTiles, t => t == neighbourTile))
                    {
                        Debug.Log("Tile is null or ignored");
                        
                        continue;
                    }
                    
                    tile.neighbours |= neighbour;
                }
            }
        }
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
            if (gridTiles == null || gridTiles.Count == 0) { return; }
            
            if (_tileMap == null)
            {
                _tileMap = GetComponent<Tilemap>();
            }
            
            foreach (GridTile tile in gridTiles)
            {
                Gizmos.DrawSphere(tile.worldPosition, 0.25f);
            }
        }
        
        void ShowTargetPositions()
        {
            if (gridTiles == null || gridTiles.Count == 0) { return; }
            
            if (_tileMap == null)
            {
                _tileMap = GetComponent<Tilemap>();
            }

            foreach (GridTile tile in gridTiles)
            {
                if (tile.isTargetPosition)
                {
                    Gizmos.DrawSphere(tile.worldPosition, 0.25f);
                }
            }
        }
        
    }

    private void Awake()
    {
        _tileMap = GetComponent<Tilemap>();
    }
    
    
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
        
        public readonly Dictionary<eNeighbours, Vector3Int> FlatHexOffsets = new ()
        {
            {eNeighbours.up, new Vector3Int(0, 1, 0)},
            {eNeighbours.upRight, new Vector3Int(1, 0, 0)},
            {eNeighbours.downRight, new Vector3Int(1, -1, 0)},
            {eNeighbours.down, new Vector3Int(0, -1, 0)},
            {eNeighbours.downLeft, new Vector3Int(-1, -1, 0)},
            {eNeighbours.upLeft, new Vector3Int(-1, 0, 0)}
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
            this.name = (tile.name + " (" + gridPosition.y + ", " + gridPosition.x + ")");
            
            this.tile = tile;
            
            this.gridPosition = gridPosition;
            
            this.worldPosition = tilemap.GetCellCenterWorld(gridPosition);
        }
    }
}
