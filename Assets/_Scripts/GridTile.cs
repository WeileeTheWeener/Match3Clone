using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour
{
    [SerializeField] Vector3Int gridCellIndex;
    [SerializeField] Block currentBlock;

    public Vector3Int GridCellIndex { get => gridCellIndex; set => gridCellIndex = value; }
    public Block CurrentBlock { get => currentBlock; set => currentBlock = value; }
}
