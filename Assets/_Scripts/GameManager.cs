using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] Level currentLevel;
    [SerializeField] List<BlockSO> allBlocks;

    [Header("Block Swapping")]
    [SerializeField] float blockSwapDuration;
    [SerializeField] bool isSwapInProgress;

    bool firstTweenCompleted = false;
    bool secondTweenCompleted = false;
    bool retrySwap = false;

    [Header("Prefabs")]
    [SerializeField] GameObject blockPrefab;
    [SerializeField] GameObject levelPrefab;

    [Header("UI")]
    [SerializeField] TMP_Text currentScoreText;
    [SerializeField] TMP_Text scoreToBeatLevelText;

    public UnityEvent OnBlocksSwapped;
    public GameObject BlockPrefab { get => blockPrefab; set => blockPrefab = value; }
    public bool IsSwapInProgress { get => isSwapInProgress; set => isSwapInProgress = value; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

    }
    private void Start()
    {
        currentLevel = GenerateNewLevel();

        StartCoroutine(currentLevel.StartLevel(currentScoreText,scoreToBeatLevelText, GridManager.instance.TileList));      
    }
    private Level GenerateNewLevel()
    {
        GameObject newLevel = Instantiate(levelPrefab);
        Level level = newLevel.GetComponent<Level>();

        int randomScoreRequirement = Random.Range(50, 100);

        level.SetProperties(randomScoreRequirement,allBlocks);
        
        return level;
    }
    public void SwapBlocksWithAnimation(Block firstBlock, Block secondBlock)
    {
        IsSwapInProgress = true;

        GridTile firstBlocksTile = GridManager.instance.GetTileWithBlock(firstBlock);
        GridTile secondBlocksTile = GridManager.instance.GetTileWithBlock(secondBlock);

        firstTweenCompleted = false;
        secondTweenCompleted = false;

        Tween firstBlockTween = firstBlock.transform.DOMove(secondBlocksTile.transform.position, blockSwapDuration)
            .OnStart(() =>
            {
                firstBlock.transform.SetParent(GridManager.instance.Canvas.transform, true);
                firstBlocksTile.CurrentBlock = null;
            })
            .OnComplete(() =>
            {
                firstBlock.transform.SetParent(secondBlocksTile.transform);
                firstBlock.transform.localPosition = Vector3.zero;
                secondBlocksTile.CurrentBlock = firstBlock;
                firstTweenCompleted = true;
            });

        Tween secondBlockTween = secondBlock.transform.DOMove(firstBlocksTile.transform.position, blockSwapDuration)
            .OnStart(() =>
            {
                secondBlock.transform.SetParent(GridManager.instance.Canvas.transform, true);
                secondBlocksTile.CurrentBlock = null;
            })
            .OnComplete(() =>
            {
                secondBlock.transform.SetParent(firstBlocksTile.transform);
                secondBlock.transform.localPosition = Vector3.zero;
                firstBlocksTile.CurrentBlock = secondBlock;
                secondTweenCompleted = true;
            });

        StartCoroutine(WaitForSwappingToEnd(firstBlocksTile, secondBlocksTile));
    }

    public IEnumerator WaitForSwappingToEnd(GridTile firstTile, GridTile secondTile)
    {
        yield return new WaitUntil(() => firstTweenCompleted && secondTweenCompleted);

        IsSwapInProgress = false;

        bool firstTilesMatched = CheckForMatchingBlocks(firstTile.GridCellIndex.x, firstTile.GridCellIndex.y);
        bool secondTilesMatched = CheckForMatchingBlocks(secondTile.GridCellIndex.x, secondTile.GridCellIndex.y);

        if (!firstTilesMatched && !secondTilesMatched)
        {
            if (!retrySwap)
            {
                retrySwap = true;
                SwapBlocksWithAnimation(firstTile.CurrentBlock, secondTile.CurrentBlock);
            }
            else
            {
                retrySwap = false;
            }
        }
        else
        {
            retrySwap = false;
        }
    }
    public Block GetTargetBlockWithDirection(GridTile currentTile,string direction)
    {
        GridTile targetTile = null;

        switch (direction)
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

        if (targetTile != null && targetTile.CurrentBlock != null)
        {
            return targetTile.CurrentBlock;
        }
        else return null;
    }
    public bool CheckForMatchingBlocks(int rowIndex, int columnIndex)
    {
        List<GridTile> rowTiles = GridManager.instance.GetTilesInRow(rowIndex);
        List<GridTile> columnTiles = GridManager.instance.GetTilesInColumn(columnIndex);

        List<GridTile> matchedRowTiles = GetConsecutiveMatchingTiles(rowTiles);
        List<GridTile> matchedColumnTiles = GetConsecutiveMatchingTiles(columnTiles);

        if(matchedRowTiles.Count > 0 || matchedColumnTiles.Count > 0)
        {
            StartCoroutine(DestroyBlocksWithAnimation(matchedRowTiles));
            StartCoroutine(DestroyBlocksWithAnimation(matchedColumnTiles));

            /*foreach (var tile in matchedRowTiles)
            {
                //tile.GetComponent<Image>().color = Color.red;
            }

            foreach (var tile in matchedColumnTiles)
            {
                //tile.GetComponent<Image>().color = Color.blue;
            }*/

            OnBlocksSwapped.Invoke();
            return true;
        }
        else
        {
            return false;
        }

    }
    private IEnumerator DestroyBlocksWithAnimation(List<GridTile> tiles)
    {
        foreach (GridTile tile in tiles)
        {
            if (tile.CurrentBlock != null)
            {
                CanvasGroup canvasGroup = tile.CurrentBlock.GetComponent<CanvasGroup>();

                if (canvasGroup != null)
                {
                    canvasGroup.DOFade(0f, 0.2f).OnComplete(() =>
                    {
                        Destroy(tile.CurrentBlock.gameObject);
                    });
                }
                yield return new WaitForSeconds(0.1f);
            }
        }

        currentLevel.AddScore(5, currentScoreText,0.1f);

    }
    private List<GridTile> GetConsecutiveMatchingTiles(List<GridTile> tiles)
    {
        List<GridTile> matchedTiles = new List<GridTile>();
        List<GridTile> tempMatchedTiles = new List<GridTile>();

        for (int i = 0; i < tiles.Count; i++)
        {
            if (i == 0 || tiles[i].CurrentBlock.BlockSO == tiles[i - 1].CurrentBlock.BlockSO)
            {
                tempMatchedTiles.Add(tiles[i]);
            }
            else
            {
                if (tempMatchedTiles.Count >= 3)
                {
                    matchedTiles.AddRange(tempMatchedTiles);
                }
                tempMatchedTiles.Clear();
                tempMatchedTiles.Add(tiles[i]);
            }
        }

        if (tempMatchedTiles.Count >= 3)
        {
            matchedTiles.AddRange(tempMatchedTiles);
        }

        return matchedTiles;
    }
}
