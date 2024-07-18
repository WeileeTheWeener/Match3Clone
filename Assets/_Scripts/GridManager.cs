using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Grid))]
public class GridManager : MonoBehaviour
{

    [SerializeField] GameObject tilePrefab;
    [SerializeField] GameObject tilesHolder;
    [SerializeField] int tileCountX,tileCountY;
    [SerializeField] Vector3Int gridOffset;  
    [SerializeField] List<GridTile> tileList;
    Grid grid;

    public List<GridTile> TileList { get => tileList; set => tileList = value; }

    private void Awake()
    {
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
        for(int i = 0; i < tileCountX; i++)
        {
            for(int j= 0; j < tileCountY; j++)
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

}
