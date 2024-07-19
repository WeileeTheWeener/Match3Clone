using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class Level : MonoBehaviour
{
    [SerializeField] int currentScore;
    [SerializeField] int requiredScoreForCompletion;
    [SerializeField] float timeLimit;
    [SerializeField] List<BlockSO> availableBlocks;


    public UnityEvent OnScoreAdded;
    public UnityEvent OnLevelEnded;

    public void SetProperties(int requiredScoreForCompletion,List<BlockSO> availableBlocks)
    {
        this.requiredScoreForCompletion = requiredScoreForCompletion;
        this.availableBlocks = availableBlocks;
    }
    public void SetProperties(int requiredScoreForCompletion, List<BlockSO> availableBlocks, float timeLimit,TMP_Text scoreToBeatLevelText)
    {
        this.requiredScoreForCompletion = requiredScoreForCompletion;
        this.availableBlocks = availableBlocks;
        this.timeLimit = timeLimit;
    }
    public void AddScore(int score,TMP_Text currentScoreText)
    {
        currentScore += score;
        currentScoreText.text = "Score : " + score.ToString();

        if(currentScore >= requiredScoreForCompletion)
        {
            EndLevel();
        }

        OnScoreAdded.Invoke();
    }
    public void EndLevel()
    {
        Debug.Log("You beat the level");
        OnLevelEnded.Invoke();
    }
    public IEnumerator StartLevel(TMP_Text scoreToBeatLevelText,List<GridTile> gridTileList)
    {
        scoreToBeatLevelText.text = "Score To Beat : " + requiredScoreForCompletion.ToString();
        BlockSO lastGeneratedBlockSO = null;

        foreach(GridTile gridTile in gridTileList)
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
                (tileIndexX > 0 && randomAvailableBlock.type == GridManager.instance.GetGridTile(tileIndexX-1,tileIndexY).CurrentBlock.BlockSO.type &&
                randomAvailableBlock.color == GridManager.instance.GetGridTile(tileIndexX - 1, tileIndexY).CurrentBlock.BlockSO.color));
            }

            newBlockComponent.BlockSO = randomAvailableBlock;
            lastGeneratedBlockSO = newBlockComponent.BlockSO;

            gridTile.CurrentBlock = newBlockComponent;
            yield return new WaitForSeconds(0.1f);
            
        }
    
    }

}
