using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    [SerializeField] BlockSO blockSO;

    Image image;


    public BlockSO BlockSO { get => blockSO; set => blockSO = value; }

    private void Awake()
    {
        image = GetComponent<Image>();
        image.sprite = BlockSO.sprite;

        blockSO.type = BlockSO.BlockType.Normal;
        blockSO.color = GetColorFromSpriteName(blockSO.sprite.name);
    }
    private void Update()
    {
        image.sprite = BlockSO.sprite;
    }
    private BlockSO.BlockColor GetColorFromSpriteName(string spriteName)
    {
        foreach (BlockSO.BlockColor color in System.Enum.GetValues(typeof(BlockSO.BlockColor)))
        {
            if (spriteName.ToLower().Contains(color.ToString().ToLower()))
            {
                return color;
            }
        }
        Debug.LogWarning("No matching color found for sprite name: " + spriteName);
        return BlockSO.BlockColor.Red;
    }
    private void OnMouseEnter()
    {
        
    }
    private void OnMouseDrag()
    {
        
    }
}
