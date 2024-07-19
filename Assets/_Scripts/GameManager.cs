using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] Level currentLevel;
    [SerializeField] List<BlockSO> allBlocks;

    [Header("Prefabs")]
    [SerializeField] GameObject blockPrefab;
    [SerializeField] GameObject levelPrefab;
    [Header("UI")]
    [SerializeField] TMP_Text currentScoreText;
    [SerializeField] TMP_Text scoreToBeatLevelText;

    public GameObject BlockPrefab { get => blockPrefab; set => blockPrefab = value; }

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
        StartCoroutine(currentLevel.StartLevel(scoreToBeatLevelText, GridManager.instance.TileList));
    }
    private Level GenerateNewLevel()
    {
        GameObject newLevel = Instantiate(levelPrefab);
        Level level = newLevel.GetComponent<Level>();

        int randomScoreRequirement = Random.Range(50, 100);
        level.SetProperties(randomScoreRequirement,allBlocks);
        
        return level;
    }
}
