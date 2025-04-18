using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Pathfinding
{
    public static void FindPath(List<GridTile> grid, Vector3Int startGridPos, Vector3Int endGridPos,
        out List<GridTile> tilePath, int maxDistance = -1)
    {
        if (grid != null && grid.Count != 0 && startGridPos != endGridPos)
        {
            GridTile startTile = grid.Find(t => t.gridPosition == startGridPos);
            GridTile endTile = grid.Find(t => t.gridPosition == endGridPos);
            
            if (startTile == endTile)
            {
                tilePath = null;
                return;
            }
            
            if (startTile != null && endTile != null)
            {
                tilePath = null;
                
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
                        //if end tile is occupied, we want to retrace a path from the tile right before it
                        if (currentNode.tile.Occupied) 
                        {
                            PathNode previousNode = currentNode.parent;
                            while (previousNode != null && previousNode.tile.Occupied)
                            {
                                previousNode = previousNode.parent;
                            }
                            
                            if (previousNode != null)
                            {
                                tilePath = RetracePath(previousNode, maxDistance);
                            }
                        }
                        else tilePath = RetracePath(currentNode, maxDistance);
                        
                        return;
                    }

                    openSet.Remove(currentNode);
                    closedSet.Add(currentNode.tile);

                    foreach (GridTile.eNeighbours dir in Enum.GetValues(typeof(GridTile.eNeighbours)))
                    {
                        if (dir == GridTile.eNeighbours.none) continue;
                        
                        if (!currentNode.tile.neighbours.HasFlag(dir)) continue;
                        
                        if (currentNode.tile.Occupied && currentNode != startNode) continue;

                        Vector3Int offset = (Mathf.Abs(currentNode.tile.gridPosition.y) % 2 == 0) 
                            ? currentNode.tile.EvenFlaxHexOffsets[dir]
                            : currentNode.tile.OddFlaxHexOffsets[dir];

                        Vector3Int neighborPos = currentNode.tile.gridPosition + offset;
                        GridTile neighborTile = grid.Find(t => t.gridPosition == neighborPos);

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
                
                
            }
            else
            {
                tilePath = null;
            }
        }
        else
        {
            tilePath = null;
        }
    }
    
    private static List<GridTile> RetracePath(PathNode endNode, int maxDistance = -1)
    {
        List<GridTile> path = new List<GridTile>();
        PathNode currentNode = endNode;

        while (currentNode != null)
        {
            path.Add(currentNode.tile);
            currentNode = currentNode.parent;
        }

        path.Reverse();
        
        if (maxDistance > 0 && path.Count > maxDistance + 1)
        {
            path = path.GetRange(0, maxDistance + 1);
        }
        
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
