using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GridTileInteraction : MonoBehaviour
{
    public enum eSelectionState
    {
        None,
        General,
        PathStart,
        PathEnd,
        PathFind
    }
    
    [Header("Selection")]

    [SerializeField] private GameObject selectionOutlinePrefab;
    
    [Header("Pathing")]
    
    [SerializeField] private GameObject pathPrefab;
    
    [Header("Debug")]
    
    [SerializeField] private eSelectionState selectionState;
    
    //Private, or Non Serialized Below
    
    private GameObject _currentSelectionOutline;
    
    private GridTile _currentGridTile;
    
    private GameObject _currentPathStart;
    
    private GameObject _currentPathEnd;
    
    private List<GameObject> _currentPath = new ();

    private void Start()
    {
        if (selectionOutlinePrefab == null)
        {
            Debug.LogError("Selection Outline Prefab is not assigned!");
            return;
        }
        
        if (pathPrefab == null)
        {
            Debug.LogError("Path Prefab is not assigned!");
            return;
        }
        
        if (TileGrid.Instance == null)
        {
            Debug.LogError("TileGrid Instance is not assigned!");
            return;
        }
        
        _currentSelectionOutline = Instantiate(selectionOutlinePrefab, transform);
        
        _currentSelectionOutline.SetActive(false);
        
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            switch (selectionState)
            {
                case eSelectionState.General:
                    
                    if (_currentPathStart != null)
                    {
                        Destroy(_currentPathStart);
                    }
                    
                    if (_currentPathEnd != null)
                    {
                        Destroy(_currentPathEnd);
                    }
                    
                    TrySelectClick(mousePos);
                    
                    break;
                
                case eSelectionState.PathStart:
                    
                    if (_currentPathStart != null)
                    {
                        Destroy(_currentPathStart);
                    }
                    
                    Vector3 placePos = TileGrid.Instance.ClosestGridPosAtWorldPos(mousePos);
                    
                    _currentPathStart = Instantiate(pathPrefab, placePos, Quaternion.identity);
                    
                    break;
                
                case eSelectionState.PathEnd:
                    
                    if (_currentPathEnd != null)
                    {
                        Destroy(_currentPathEnd);
                    }
                    
                    placePos = TileGrid.Instance.ClosestGridPosAtWorldPos(mousePos);
                    
                    _currentPathEnd = Instantiate(pathPrefab, placePos, Quaternion.identity);
                    
                    break;
            }
        }
    }
    
    private void TrySelectClick( Vector3 worldPos)
    {
        _currentGridTile = TileGrid.Instance.ClosestGridTileAtWorldPos(worldPos);
        
        if (_currentGridTile == null)
        {
            _currentSelectionOutline.SetActive(false);
            
            return;
        } 
        
        _currentSelectionOutline.transform.position = _currentGridTile.worldPosition;
        
        _currentSelectionOutline.SetActive(true);
    }
    
    public void SetNoSelectionState()
    {
        selectionState = eSelectionState.None;
        
        if (_currentSelectionOutline != null)
        {
            _currentSelectionOutline.SetActive(false);
        }
        
        if (_currentPathStart != null)
        {
            Destroy(_currentPathStart);
            
            _currentPathStart = null;
        }
        
        if (_currentPathEnd != null)
        {
            Destroy(_currentPathEnd);
            
            _currentPathEnd = null;
        }
        
        foreach (GameObject path in _currentPath)
        {
            Destroy(path);
        }
        
        _currentPath.Clear();
    }
    
    public void SetGeneralSelectionState()
    {
        selectionState = eSelectionState.General;
    }
    
    public void SetPathStartSelectionState()
    {
        selectionState = eSelectionState.PathStart;
    }
    
    public void SetPathEndSelectionState()
    {
        selectionState = eSelectionState.PathEnd;
    }
    
    public void SetPathFindSelectionState()
    {
        if (selectionState == eSelectionState.PathFind) { return; }
        
        
        selectionState = eSelectionState.PathFind;
        
        if (_currentPathStart == null || _currentPathEnd == null)
        {
            Debug.LogError("Path Start or End is not set!");
            return;
        }
        
        Vector3Int startPos = TileGrid.Instance.ClosestGridTileAtWorldPos(_currentPathStart.transform.position).gridPosition;
        
        Vector3Int endPos = TileGrid.Instance.ClosestGridTileAtWorldPos(_currentPathEnd.transform.position).gridPosition;
        
        List<GridTile> path = Pathfinding.FindPath(startPos, endPos, TileGrid.Instance.GridTiles);
        
        if (path == null || path.Count == 0)
        {
            Debug.LogError("Path not found!");
            return;
        }
        
        foreach (GameObject existingPath in _currentPath)
        {
            Destroy(existingPath);
        }
        
        _currentPath.Clear();
        
        if (_currentPathStart != null)
        {
            Destroy(_currentPathStart);
        }
        
        if (_currentPathEnd != null)
        {
            Destroy(_currentPathEnd);
        }
        
        foreach (GridTile tile in path)
        {
            GameObject pathTile = Instantiate(pathPrefab, tile.worldPosition, Quaternion.identity);
            
            _currentPath.Add(pathTile);
        }
    }
}
