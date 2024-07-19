using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class Block : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] BlockSO blockSO;

    Image image;
    CanvasGroup canvasGroup;

    private Vector2 startDragPosition;
    private Vector2 currentDragPosition;
    private Vector2 dragDirection;

    public UnityEvent OnBlockPositionChanged;

    private GridTile currentTile;
    private GridTile targetTile;

    private bool isSwipeDetected;
    public float swipeThreshold = 50f;

    public BlockSO BlockSO { get => blockSO; set => blockSO = value; }

    private void Awake()
    {
        image = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
        currentTile = GetComponentInParent<GridTile>();

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
        Debug.LogWarning("No matching color found for sprite name: " + spriteName + " setting color to red");
        return BlockSO.BlockColor.Red;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        //Debug.Log("on begin drag " + eventData.pointerClick.gameObject.name, this);
        startDragPosition = eventData.pointerClick.gameObject.transform.position;
        currentTile = GetComponentInParent<GridTile>();
        canvasGroup.blocksRaycasts = false;
        targetTile = null;
        isSwipeDetected = false;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log("cliked on " + eventData.pointerClick.gameObject.name, this);
    }
    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        //Debug.Log("on drag " + eventData.pointerClick.gameObject.name, this);
        currentDragPosition = eventData.position;
        dragDirection = (currentDragPosition - startDragPosition).normalized;

        if(!isSwipeDetected && Vector2.Distance(startDragPosition, currentDragPosition) > swipeThreshold)
        {
            float swipeUpValue = Vector2.Dot(transform.up, dragDirection);
            float swipeDownValue = Vector2.Dot(-transform.up, dragDirection);
            float swipeRightValue = Vector2.Dot(transform.right, dragDirection);
            float swipeLeftValue = Vector2.Dot(-transform.right, dragDirection);

            List<(string direction, float value)> swipeValues = new List<(string, float)>
            {
                ("Up", swipeUpValue),
                ("Down", swipeDownValue),
                ("Right", swipeRightValue),
                ("Left", swipeLeftValue)
            };

            string maxDirection = null;
            float maxValue = -1f;

            foreach (var swipe in swipeValues)
            {
                if (swipe.value > maxValue)
                {
                    maxValue = swipe.value;
                    maxDirection = swipe.direction;
                }
            }

            Debug.Log("Swipe Direction: " + maxDirection + " with value: " + maxValue);

            switch (maxDirection)
            {
                case "Up":
                    targetTile = GridManager.instance.GetGridTile(currentTile.GridCellIndex.x, currentTile.GridCellIndex.y + 1);
                    break;
                case "Down":
                    targetTile = GridManager.instance.GetGridTile(currentTile.GridCellIndex.x, currentTile.GridCellIndex.y - 1);
                    break;
                case "Right":
                    targetTile = GridManager.instance.GetGridTile(currentTile.GridCellIndex.x + 1, currentTile.GridCellIndex.y);
                    break;
                case "Left":
                    targetTile = GridManager.instance.GetGridTile(currentTile.GridCellIndex.x - 1, currentTile.GridCellIndex.y);
                    break;
            }

            if (targetTile != null)
            {
                //HighlightGridTile(targetTile);
                isSwipeDetected = true;
                SwapBlocks();
            }
        }
    }
    private void HighlightGridTile(GridTile tile)
    {
        tile.GetComponent<Image>().color = Color.red;
    }
    private void SwapBlocks()
    {
        Block currentBlock = currentTile.CurrentBlock;
        Block targetBlock = targetTile.CurrentBlock;

        currentTile.CurrentBlock = targetBlock;
        targetTile.CurrentBlock = currentBlock;

        targetBlock.transform.SetParent(currentTile.transform);
        targetBlock.transform.localPosition = Vector3.zero;

        currentBlock.transform.SetParent(targetTile.transform);
        currentBlock.transform.localPosition = Vector3.zero;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        //Debug.Log("on end drag " + eventData.pointerClick.gameObject.name, this);
        currentDragPosition = Vector2.zero;
        canvasGroup.blocksRaycasts = true;
        targetTile = null;
    }
    private void OnDrawGizmos()
    {
        if(currentDragPosition != Vector2.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(startDragPosition, startDragPosition + dragDirection * 50);
        }
    }
}
