using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class Block : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    [SerializeField] BlockSO blockSO;

    Image image;
    CanvasGroup canvasGroup;

    Vector2 startDragPosition;
    Vector2 currentDragPosition;
    Vector2 dragDirection;

    bool isSwipeDetected;
    float swipeThreshold = 50f;

    public BlockSO BlockSO { get => blockSO; set => blockSO = value; }

    private void Awake()
    {
        image = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();

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
        if (GameManager.instance.IsSwapInProgress) return;

        startDragPosition = eventData.pointerDrag.gameObject.transform.position;
        canvasGroup.blocksRaycasts = false;
        isSwipeDetected = false;
    }
    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        if (GameManager.instance.IsSwapInProgress) return;

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

            //Debug.Log("Swipe Direction: " + maxDirection + " with value: " + maxValue);

            Block TargetBlock = GameManager.instance.GetTargetBlockWithDirection(GridManager.instance.GetTileWithBlock(this), maxDirection);

            if (TargetBlock != null)
            {
                isSwipeDetected = true;
                GameManager.instance.SwapBlocksWithAnimation(this, TargetBlock);
            }
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        currentDragPosition = Vector2.zero;
        canvasGroup.blocksRaycasts = true;
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
