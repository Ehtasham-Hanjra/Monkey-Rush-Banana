using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [Header("Level Settings")]
    public GameObject treePrefab;
    public Transform treesParent;
    public int baseTreeCount = 5;
    public float minX = -8f;
    public float maxX = 8f;
    public float minY = -4f;
    public float maxY = 4f;
    public float snakeProbability = 0.3f;

    public List<TreeObject> trees = new List<TreeObject>();
    private int currentLevel = 1;

    private void Awake()
    {
        Debug.Log("LevelManager Awake called");
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("LevelManager instance created");
        }
        else
        {
            Debug.Log("LevelManager instance already exists, destroying duplicate");
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// How many trees did we instantiate this level?
    /// </summary>
    public int TreeCount => trees.Count;

    /// <summary>
    /// How many banana-trees have been revealed so far?
    /// </summary>
    public int BananasRevealed
    {
        get
        {
            int count = 0;
            foreach (var t in trees)
                if (t.hasBanana && t.hiddenObject.activeSelf)
                    count++;
            return count;
        }
    }

    private void Start()
    {
        Debug.Log("LevelManager Start called");
        if (treePrefab == null)
        {
            Debug.LogError("Tree prefab reference missing!");
            return;
        }
        if (treesParent == null)
        {
            Debug.LogError("Trees parent reference missing!");
            return;
        }
        SetupLevel();
    }

    public void SetupLevel()
    {
        Debug.Log($"Setting up level {currentLevel}");
        
        // Clear existing trees
        if (treesParent != null)
        {
            foreach (Transform child in treesParent)
            {
                Destroy(child.gameObject);
            }
        }
        trees.Clear();

        // Calculate number of trees for this level
        int treeCount = baseTreeCount + (currentLevel - 1);
        Debug.Log($"Creating {treeCount} trees for level {currentLevel}");

        // Create new trees
        for (int i = 0; i < treeCount; i++)
        {
            // Random position
            Vector3 position = new Vector3(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY),
                0
            );

            // Create tree
            GameObject treeObj = Instantiate(treePrefab, position, Quaternion.identity, treesParent);
            TreeObject tree = treeObj.GetComponent<TreeObject>();

            if (tree == null)
            {
                Debug.LogError($"Tree component missing on instantiated tree {i}!");
                continue;
            }

            // Randomly assign banana or snake
            tree.hasBanana = Random.value > snakeProbability;
            Debug.Log($"Tree {i} created at {position}, hasBanana: {tree.hasBanana}");

            trees.Add(tree);
        }

        // Reset monkey position
        if (MonkeyController.Instance != null)
        {
            Debug.Log("Resetting monkey position");
            MonkeyController.Instance.ResetPosition();
        }
        else
        {
            Debug.LogError("MonkeyController instance missing!");
        }
        foreach (TreeObject t in trees) t.ResetTree();
    }

    public bool AllBananasCollected()
    {
        foreach (TreeObject tree in trees)
        {
            if (tree.hasBanana && !tree.hiddenObject.activeSelf)
            {
                Debug.Log("Not all bananas collected yet");
                return false;
            }
        }
        Debug.Log("All bananas collected! Moving to next level...");
        return true;
    }

    public void NextLevel()
    {
        Debug.Log($"Moving to level {currentLevel + 1}");
        currentLevel++;
        GameManager.Instance.AddScore(5); // Bonus points for completing level
        SetupLevel();
        
        // Reset game state
        GameManager.Instance.isGameOver = false;
        Time.timeScale = 1f;
    }

    public void ResetLevel()
    {
        Debug.Log("Resetting level");
        currentLevel = 1;
        SetupLevel();
    }
} 