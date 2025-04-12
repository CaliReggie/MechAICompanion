using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Pathfinding
{
    public static List<GridTile> FindPath(Vector3Int startGridPos, Vector3Int endGridPos, List<GridTile> gridTiles)
    {
        if (gridTiles == null || gridTiles.Count == 0)
        {
            Debug.LogWarning("Grid tiles list is empty or null!");
            
            return null;
        }
        
        if (startGridPos == endGridPos)
        {
            Debug.Log("Start and End positions are the same!");
            
            return null;
        }
        
        
        GridTile startTile = gridTiles.Find(t => t.gridPosition == startGridPos);
        GridTile endTile = gridTiles.Find(t => t.gridPosition == endGridPos);

        if (startTile == null || endTile == null)
        {
            Debug.LogWarning("Start or End tile not found!");
            
            return null;
        }

        var openSet = new List<PathNode>();
        var closedSet = new HashSet<GridTile>();

        PathNode startNode = new PathNode(startTile);
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            // Get node with lowest fCost
            openSet.Sort((a, b) => a.fCost.CompareTo(b.fCost));
            PathNode currentNode = openSet[0];

            if (currentNode.tile == endTile)
            {
                return RetracePath(currentNode);
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode.tile);

            foreach (GridTile.eNeighbours dir in Enum.GetValues(typeof(GridTile.eNeighbours)))
            {
                if (dir == GridTile.eNeighbours.none) continue;
                
                if (!currentNode.tile.neighbours.HasFlag(dir)) continue;

                Vector3Int offset = (Mathf.Abs(currentNode.tile.gridPosition.y) % 2 == 0) 
                    ? currentNode.tile.EvenFlaxHexOffsets[dir]
                    : currentNode.tile.OddFlaxHexOffsets[dir];

                Vector3Int neighborPos = currentNode.tile.gridPosition + offset;
                GridTile neighborTile = gridTiles.Find(t => t.gridPosition == neighborPos);

                if (neighborTile == null || closedSet.Contains(neighborTile)) continue;

                float tentativeG = currentNode.gCost + 1f; // Adjust if different tile costs

                PathNode neighborNode = openSet.Find(n => n.tile == neighborTile);
                if (neighborNode == null)
                {
                    neighborNode = new PathNode(neighborTile);
                    neighborNode.gCost = tentativeG;
                    neighborNode.hCost = Vector3Int.Distance(neighborTile.gridPosition, endTile.gridPosition);
                    neighborNode.parent = currentNode;
                    openSet.Add(neighborNode);
                }
                else if (tentativeG < neighborNode.gCost)
                {
                    neighborNode.gCost = tentativeG;
                    neighborNode.parent = currentNode;
                }
            }
        }

        return null;
    }
    
    private static List<GridTile> RetracePath(PathNode endNode)
    {
        List<GridTile> path = new List<GridTile>();
        PathNode currentNode = endNode;

        while (currentNode != null)
        {
            path.Add(currentNode.tile);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        return path;
    }
    
    private class PathNode
    {
        public GridTile tile;
        public PathNode parent;
        public float gCost; // Distance from start
        public float hCost; // Heuristic to end
        public float fCost => gCost + hCost;

        public PathNode(GridTile tile)
        {
            this.tile = tile;
        }
    }
}
