using UnityEngine;
// This is a package I have installed
using Sylves;
using System.Collections.Generic;
using System.Linq;

public class HexGridDrawer : MonoBehaviour
{
    public GameObject gridMesh;
    public Vector3Int min;
    public Vector3Int max;
    public Vector2 size;
    HexGrid hexGrid;
    MeshGrid meshGrid;
    
    public Hex_Grid hex_Grid;
    public  void GenerateGrid()
    {
        hexGrid = new HexGrid(size, HexOrientation.FlatTopped, new HexBound(min, max));
        
        List<Hex_Cell> grid = new List<Hex_Cell>();
        foreach(var cell in hexGrid.GetCells()) {
            var poly = hexGrid.GetPolygon(cell);
            Hex_Corner[] corners = new Hex_Corner[poly.Length];
            for(int i = 0; i < poly.Length; i++) {
                corners[i] = new Hex_Corner(poly[i]);
            }
            Hex_Cell hex_Cell = new Hex_Cell(new Vector3(cell.x, cell.y, cell.z), corners);
            grid.Add(hex_Cell);
        }
        // print the sum of all corners in the grid
        int sum = 0;
        foreach(Hex_Cell cell in grid)
        {
            sum += cell.corners.Length;
        }
        Debug.Log("num of cells before " + sum);
        // remove duplicate corners, any removed corners should be set to the same corner in the other cell
        RemoveDuplicateHex_Corners(ref grid, out List<Hex_Corner> uniqueHex_Corners);
        hex_Grid = new Hex_Grid(grid, uniqueHex_Corners);

        sum = 0;
        foreach(Hex_Cell cell in grid)
        {
            sum += cell.corners.Length;
        }
    }

    public void RemoveDuplicateHex_Corners(ref List<Hex_Cell> hex_Cells, out List<Hex_Corner> uniqueHex_Corners)
    {
        uniqueHex_Corners = new List<Hex_Corner>();
        uniqueHex_Corners.Add(new Hex_Corner(new Vector3(-9999,-9999,-9999)));
        

        foreach (var hex_Cell in hex_Cells)
        {
            
            for (int i = 0; i < hex_Cell.corners.Length; i++)
            {
                if (uniqueHex_Corners.Any(c => c == hex_Cell.corners[i]))
                {
                    // Replace the duplicate Hex_Corner with the common one
                    hex_Cell.corners[i] = uniqueHex_Corners.Find(c => c == hex_Cell.corners[i]);
                    Debug.Log("duplicate found");
                }
                else
                {
                    uniqueHex_Corners.Add(hex_Cell.corners[i]);
                }

            }
        }
        Debug.Log(uniqueHex_Corners.Count + " unique corners");

        
    }
    public int pointCount = 10;
    void OnDrawGizmos()
    {
        //if(hexGrid == null || meshGrid == null) GenerateGrid();

        // get all polygons in mesh grid and draw their vertecies
        /* foreach(var cell in meshGrid.GetCells()) {
            var poly = meshGrid.GetPolygon(cell);
            foreach(var v in poly) {
                Gizmos.DrawSphere(v, 0.1f);
            }
        } */
        /* bool p_m = false;
        foreach(var cell in hexGrid.GetCells()) {
            p_m = !p_m;
            Gizmos.color = p_m ? Color.red : Color.blue;
            var poly = hexGrid.GetPolygon(cell);
            foreach(var v in poly) {
                int val = p_m ? 1 : 0;
                Vector3 pos = v;
                pos.z += val;
                Gizmos.DrawSphere(pos, 0.1f);
            }
        } */
        
        for(int i = 0; i < pointCount; i++)
        {
            Gizmos.DrawSphere(hex_Grid.gridCorners[i].position, 0.2f);
        }
        foreach(var cell in hex_Grid.gridCells)
        {
            Gizmos.DrawLine(cell.corners[0].position, cell.corners[1].position);
            Gizmos.DrawLine(cell.corners[1].position, cell.corners[2].position);
            Gizmos.DrawLine(cell.corners[2].position, cell.corners[3].position);
            Gizmos.DrawLine(cell.corners[3].position, cell.corners[4].position);
            Gizmos.DrawLine(cell.corners[4].position, cell.corners[5].position);
            Gizmos.DrawLine(cell.corners[5].position, cell.corners[0].position);
        }
    }

    void OnValidate()
    {
        if(min.x > max.x) min.x = max.x - 1;
        if(min.y > max.y) min.y = max.y - 1;
        if(min.z > max.z) min.z = max.z - 1;
        if(max.x < min.x) max.x = min.x + 1;
        if(max.y < min.y) max.y = min.y + 1;
        if(max.z < min.z) max.z = min.z + 1;
        if(pointCount < 2) pointCount = 2;
        if(pointCount > hex_Grid.gridCorners.Count) pointCount = hex_Grid.gridCorners.Count;
        //GenerateGrid();
        hex_Grid.UpdateCorners();
    }
    public void RelaxGrid()
    {
        hex_Grid.RelaxGrid();
    }
    
}
[System.Serializable]
public class Hex_Cell
{
    public Vector3 center;
    public Hex_Corner[] corners;
    public Hex_Cell(Vector3 center, Hex_Corner[] corners)
    {
        this.center = center;
        this.corners = corners;
    }

    public float altidude;
    public float moisture;
    public float temperature;
}
[System.Serializable]
#pragma warning disable CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
#pragma warning disable CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
public class Hex_Corner
#pragma warning restore CS0661 // Type defines operator == or operator != but does not override Object.GetHashCode()
#pragma warning restore CS0660 // Type defines operator == or operator != but does not override Object.Equals(object o)
{
    public Vector3 position;
    public Hex_Corner(Vector3 pos)
    {
        position = pos;
    }

    // override the == operator
    public static bool operator ==(Hex_Corner c1, Hex_Corner c2)
    {
        if (c1.position == c2.position)
        {
            return true;
        }
        return false;
    }
    // override the != operator
    public static bool operator !=(Hex_Corner c1, Hex_Corner c2)
    {
        return !(c1 == c2);
    }
}
[System.Serializable]
public class Hex_Grid
{
    public List<Hex_Cell> gridCells;
    public List<Hex_Corner> gridCorners;
    private Dictionary<Hex_Corner, List<Hex_Cell>> cornerCellMap;

    public Hex_Grid(List<Hex_Cell> gridCells, List<Hex_Corner> gridCorners)
    {
        this.gridCells = gridCells;
        this.gridCorners = gridCorners;
        this.cornerCellMap = new Dictionary<Hex_Corner, List<Hex_Cell>>();

        // Map corners to cells.
        foreach (var cell in this.gridCells)
        {
            foreach (var corner in cell.corners)
            {
                if (!this.cornerCellMap.ContainsKey(corner))
                {
                    this.cornerCellMap[corner] = new List<Hex_Cell>();
                }
                this.cornerCellMap[corner].Add(cell);
            }
        }
    }

    public void UpdateCorners()
    {
        foreach(var corner in gridCorners)
        {
            // Find all cells that use this corner.
            if (this.cornerCellMap.TryGetValue(corner, out var cells))
            {
                // Update the corner in each cell.
                foreach (var cell in cells)
                {
                    for (int i = 0; i < cell.corners.Length; i++)
                    {
                        if (cell.corners[i] == corner)
                        {
                            cell.corners[i] = corner;
                        }
                    }
                }
            }
        }
    }

    public void RelaxGrid()
    {
        
    }
}