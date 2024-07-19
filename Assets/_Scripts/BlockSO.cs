using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tile Block", menuName = "Create New Tile Block")]
public class BlockSO : ScriptableObject
{
    public enum BlockType
    {
        Normal,
        Special
    }
    public enum BlockColor
    {
        Red,
        Green,
        Blue,
        Brown,
        Black,
        Yellow,
        Purple
    }

    [SerializeField] public BlockType type;
    [SerializeField] public BlockColor color;
    [SerializeField] public Sprite sprite;
}
