using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Grid))]
public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    [SerializeField] Canvas canvas;
    [SerializeField] GameObject tilePrefab;
    [SerializeField] GameObject tilesHolder;
    [SerializeField] int tileCountX,tileCountY;
    [SerializeField] Vector3Int gridOffset;  
    [SerializeField] List<GridTile> tileList;
    Grid grid;

    public List<GridTile> TileList { get => tileList; set => tileList = value; }
    public int TileCountX { get => tileCountX; set => tileCountX = value; }
    public int TileCountY { get => tileCountY; set => tileCountY = value; }
    public Canvas Canvas { get => canvas; set => canvas = value; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        grid = GetComponent<Grid>();
        InitializeGrid();
    }
    private void Update()
    {
        foreach(GridTile tile in TileList)
        {
            tile.transform.position = grid.GetCellCenterWorld(tile.GridCellIndex + gridOffset);
        }
    }
    private void InitializeGrid()
    {
        for(int i = 0; i < TileCountX; i++)
        {
            for(int j= 0; j < TileCountY; j++)
            {
                GameObject newGridElement = Instantiate(tilePrefab);
                newGridElement.name = $"Tile {i},{j}";
                Vector3 gridPos = grid.GetCellCenterWorld(new Vector3Int(i,j,0));
                newGridElement.transform.SetParent(tilesHolder.transform);
                newGridElement.transform.position = gridPos;
                GridTile newElementsGridTile = newGridElement.GetComponent<GridTile>();
                newElementsGridTile.GridCellIndex = grid.WorldToCell(gridPos);
                TileList.Add(newElementsGridTile);                           
            }        
        }       
    }
    public GridTile GetGridTile(int x, int y)
    {
        if (tileList.Contains(TileList.FirstOrDefault(tile => tile.GridCellIndex.x == x && tile.GridCellIndex.y == y)))
        {
            return TileList.FirstOrDefault(tile => tile.GridCellIndex.x == x && tile.GridCellIndex.y == y);
        }
        else return null;        
    }


}
