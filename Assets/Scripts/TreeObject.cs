using UnityEngine;

public class TreeObject : MonoBehaviour
{
    [Header("Tree Settings")]
    public GameObject hiddenObject;
    public bool hasBanana = true;
    public Sprite bananaSprite;
    public Sprite snakeSprite;

    [Header("Size Settings")]
    public float treeScale = 1.5f; // Increased tree size
    public float hiddenObjectScale = 0.7f; // Decreased banana/snake size

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private bool isRevealed = false;

    private void Start()
    {
        Debug.Log($"Tree {gameObject.name} initialized. Has Banana: {hasBanana}");
        
        // Get or add required components
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        // Apply tree scale
        transform.localScale = new Vector3(treeScale, treeScale, 1f);

        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
            // Set collider size based on sprite
            if (spriteRenderer.sprite != null)
            {
                Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
                boxCollider.size = spriteSize * 0.9f; // Slightly smaller than sprite
                boxCollider.offset = Vector2.zero; // Center the collider
            }
        }

        // Ensure hidden object exists
        if (hiddenObject == null)
        {
            hiddenObject = new GameObject("HiddenObject");
            hiddenObject.transform.SetParent(transform);
            hiddenObject.transform.localPosition = Vector3.zero;
            hiddenObject.transform.localScale = new Vector3(hiddenObjectScale, hiddenObjectScale, 1f);
            
            SpriteRenderer hiddenRenderer = hiddenObject.AddComponent<SpriteRenderer>();
            hiddenRenderer.sprite = hasBanana ? bananaSprite : snakeSprite;
            hiddenRenderer.sortingOrder = 1; // Render above the tree
        }
        else
        {
            // Ensure hidden object is properly parented
            if (hiddenObject.transform.parent != transform)
            {
                hiddenObject.transform.SetParent(transform);
                hiddenObject.transform.localPosition = Vector3.zero;
            }
            
            // Set the correct sprite and scale
            SpriteRenderer hiddenRenderer = hiddenObject.GetComponent<SpriteRenderer>();
            if (hiddenRenderer != null)
            {
                hiddenRenderer.sprite = hasBanana ? bananaSprite : snakeSprite;
                hiddenRenderer.sortingOrder = 1;
            }
            hiddenObject.transform.localScale = new Vector3(hiddenObjectScale, hiddenObjectScale, 1f);
        }

        // Hide the hidden object initially
        hiddenObject.SetActive(false);
        Debug.Log($"Tree {gameObject.name} sprite set to: {(hasBanana ? "Banana" : "Snake")}");
    }

    private void OnMouseDown()
    {
        if (!CanInteract())
        {
            return;
        }

        Debug.Log($"Tree {gameObject.name} clicked! Revealed: {isRevealed}, GameOver: {GameManager.Instance.isGameOver}, Paused: {GameManager.Instance.isPaused}");
        RevealObject();
    }

    private bool CanInteract()
    {
        if (isRevealed)
        {
            return false;
        }

        if (GameManager.Instance.isGameOver)
        {
            return false;
        }

        if (GameManager.Instance.isPaused)
        {
            return false;
        }
        if (GameManager.Instance.isInteracting) return false;

        if (Time.timeScale <= 0)
        {
            return false;
        }

        return true;
    }

    private void RevealObject()
    {
        if (hiddenObject == null)
        {
            Debug.LogError($"Hidden object reference missing on tree {gameObject.name}!");
            return;
        }
        GameManager.Instance.isInteracting = true;
        Debug.Log($"Tree {gameObject.name} revealing object. Has Banana: {hasBanana}");
        hiddenObject.SetActive(true);
        isRevealed = true;

        // Notify the monkey to jump to this tree
        if (MonkeyController.Instance != null)
        {
            MonkeyController.Instance.JumpToTree(this);
        }
    }

    public void ResetTree()
    {
        isRevealed = false;
        if (hiddenObject != null)
        {
            hiddenObject.SetActive(false);
        }
    }
} 