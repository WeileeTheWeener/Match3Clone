using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Level : MonoBehaviour
{
    [SerializeField] int levelIndex;
    [SerializeField] int currentScore;
    [SerializeField] int requiredScoreForCompletion;
    [SerializeField] float timeLimit;
    [SerializeField] List<BlockSO> availableBlocks;


    public UnityEvent OnScoreAdded;
    public UnityEvent OnLevelEnded;

    public int LevelIndex { get => levelIndex; set => levelIndex = value; }

    public void SetProperties(int requiredScoreForCompletion,List<BlockSO> availableBlocks)
    {
        currentScore = 0;
        this.requiredScoreForCompletion = requiredScoreForCompletion;
        this.availableBlocks = availableBlocks;
    }
    public void SetProperties(int requiredScoreForCompletion, List<BlockSO> availableBlocks, float timeLimit,TMP_Text scoreToBeatLevelText)
    {
        currentScore = 0;
        this.requiredScoreForCompletion = requiredScoreForCompletion;
        this.availableBlocks = availableBlocks;
        this.timeLimit = timeLimit;
    }
    public void AddScore(int scoreToAdd, TMP_Text currentScoreText, float tweenDuration)
    {
        int oldScore = currentScore;
        int newScore = currentScore + scoreToAdd;

        DOTween.To(() => oldScore, x =>
        {
            oldScore = x;
            currentScoreText.text = "Score : " + oldScore.ToString();
        }, newScore, tweenDuration).OnComplete(() =>
        {
            currentScore = newScore;

            if (currentScore >= requiredScoreForCompletion)
            {
                LevelCompleted();
            }

            OnScoreAdded.Invoke();
        });
    }
    public void LevelCompleted()
    {
        Debug.Log("You beat the level");
        GameManager.instance.LastLevelIndex = GameManager.instance.CurrentLevel.LevelIndex;
        GameManager.instance.LevelCompletedPanel.SetActive(true);
        OnLevelEnded.Invoke();
    }
    public void StartLevel(TMP_Text levelIndexText,TMP_Text currentScoreText, TMP_Text scoreToBeatLevelText)
    {
        scoreToBeatLevelText.text = "Score To Beat : " + requiredScoreForCompletion.ToString();
        currentScoreText.text = "Score : " + currentScore.ToString();
        levelIndexText.text = "Level : " + levelIndex.ToString();

        StartCoroutine(GenerateNonMatchingBlocks(GridManager.instance.TileList));
    }
    private IEnumerator GenerateNonMatchingBlocks(List<GridTile> gridTileList)
    {
        BlockSO lastGeneratedBlockSO = null;

        foreach (GridTile gridTile in gridTileList)
        {
            GameObject newBlock = Instantiate(GameManager.instance.BlockPrefab);
            newBlock.transform.SetParent(gridTile.transform);
            Block newBlockComponent = newBlock.GetComponent<Block>();

            BlockSO randomAvailableBlock = availableBlocks[UnityEngine.Random.Range(0, availableBlocks.Count)];

            int tileIndexX = gridTile.GridCellIndex.x;
            int tileIndexY = gridTile.GridCellIndex.y;

            if (lastGeneratedBlockSO != null)
            {
                do
                {
                    randomAvailableBlock = availableBlocks[UnityEngine.Random.Range(0, availableBlocks.Count)];
                }
                while ((randomAvailableBlock.type == lastGeneratedBlockSO.type && randomAvailableBlock.color == lastGeneratedBlockSO.color) ||
                (tileIndexX > 0 && randomAvailableBlock == GridManager.instance.GetGridTile(tileIndexX - 1, tileIndexY).CurrentBlock.BlockSO));
            }

            newBlockComponent.BlockSO = randomAvailableBlock;
            lastGeneratedBlockSO = newBlockComponent.BlockSO;

            gridTile.CurrentBlock = newBlockComponent;

            yield return new WaitForSeconds(0.05f);

        }
    }
    public Block GenerateRandomBlockForLevel(GridTile tile)
    {
        GameObject newBlock = Instantiate(GameManager.instance.BlockPrefab);
        newBlock.transform.SetParent(tile.transform);
        Block newBlockComponent = newBlock.GetComponent<Block>();
        tile.CurrentBlock = newBlockComponent;
        BlockSO randomAvailableBlock = availableBlocks[UnityEngine.Random.Range(0, availableBlocks.Count)];
        newBlockComponent.BlockSO = randomAvailableBlock;
        return newBlockComponent;
    }

}
