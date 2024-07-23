using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] Level currentLevel;
    [SerializeField] List<BlockSO> allBlocks;

    [Header("Block Properties")]
    [SerializeField] float blockSwapDuration;
    [SerializeField] float blockFallDuration;
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

        currentLevel.StartLevel(currentScoreText,scoreToBeatLevelText, GridManager.instance.TileList);      
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

        OnBlocksSwapped.Invoke();
        StartCoroutine(WaitForSwappingToEnd(firstBlocksTile, secondBlocksTile));
    }

    public IEnumerator WaitForSwappingToEnd(GridTile firstTile, GridTile secondTile)
    {
        yield return new WaitUntil(() => firstTweenCompleted && secondTweenCompleted);

        IsSwapInProgress = false;

        bool tilesMatched = CheckForMatchingBlocks();

        if (!tilesMatched)
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
    public bool CheckForMatchingBlocks()
    {
        HashSet<GridTile> matchedTiles = GetConsecutiveMatchingTiles();

        if(matchedTiles.Count > 0)
        {
            StartCoroutine(DestroyBlocksWithAnimation(matchedTiles));
            currentLevel.AddScore(matchedTiles.Count * 1, currentScoreText, 0.2f);


            foreach (var tile in matchedTiles)
            {
                //tile.GetComponent<Image>().color = Color.red;
            }
            
            return true;
        }
        else
        {
            return false;
        }

    }
    private IEnumerator DestroyBlocksWithAnimation(HashSet<GridTile> tiles)
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
                        tile.CurrentBlock = null;
                    });
                }
                yield return new WaitForSeconds(0.1f);
            }
        }

        yield return new WaitForSeconds(0.1f);
        StartCoroutine(SpawnNewBlocks(tiles));
    }
    private IEnumerator SpawnNewBlocks(HashSet<GridTile> tiles)
    {
        foreach (GridTile tile in tiles)
        {
            if (tile.CurrentBlock == null)
            {
                Block newBlock = currentLevel.GenerateRandomBlockForLevel(tile);

                CanvasGroup canvasGroup = newBlock.GetComponent<CanvasGroup>();

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 0f;

                    canvasGroup.DOFade(1, 0.2f).OnComplete(() =>
                    {
                        tile.CurrentBlock = newBlock;
                    });
                }
            }
                yield return new WaitForSeconds(0.1f);
        }

        CheckForMatchingBlocks();
    }
    private HashSet<GridTile> GetConsecutiveMatchingTiles()
    {
        HashSet<GridTile> matchingTiles = new HashSet<GridTile>();

        // Check horizontal matches
        for (int y = 0; y < GridManager.instance.TileCountY; y++)
        {
            for (int x = 0; x < GridManager.instance.TileCountX - 2; x++)
            {
                GridTile tileA = GridManager.instance.GetGridTile(x, y);
                GridTile tileB = GridManager.instance.GetGridTile(x + 1, y);
                GridTile tileC = GridManager.instance.GetGridTile(x + 2, y);

                if (tileA.CurrentBlock != null && tileB.CurrentBlock != null && tileC.CurrentBlock != null)
                {
                    if (tileA.CurrentBlock.BlockSO == tileB.CurrentBlock.BlockSO && tileB.CurrentBlock.BlockSO == tileC.CurrentBlock.BlockSO)
                    {
                        matchingTiles.Add(tileA);
                        matchingTiles.Add(tileB);
                        matchingTiles.Add(tileC);
                    }
                }
            }
        }

        // Check vertical matches
        for (int x = 0; x < GridManager.instance.TileCountX; x++)
        {
            for (int y = 0; y < GridManager.instance.TileCountY - 2; y++)
            {
                GridTile tileA = GridManager.instance.GetGridTile(x, y);
                GridTile tileB = GridManager.instance.GetGridTile(x, y + 1);
                GridTile tileC = GridManager.instance.GetGridTile(x, y + 2);

                if (tileA.CurrentBlock != null && tileB.CurrentBlock != null && tileC.CurrentBlock != null)
                {
                    if (tileA.CurrentBlock.BlockSO == tileB.CurrentBlock.BlockSO && tileB.CurrentBlock.BlockSO == tileC.CurrentBlock.BlockSO)
                    {
                        matchingTiles.Add(tileA);
                        matchingTiles.Add(tileB);
                        matchingTiles.Add(tileC);
                    }
                }
            }
        }

        return matchingTiles;
    }
}
