using UnityEngine;
using System.Collections;

public class MonkeyController : MonoBehaviour
{
    public static MonkeyController Instance;
    public float jumpSpeed = 5f;
    public float jumpHeight = 2f;
    public float returnSpeed = 3f;

    private Vector3 startPosition;
    private bool isJumping = false;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Setup sprite renderer
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sortingOrder = 2; // Make sure monkey appears above trees
            }

            // Setup collider
            boxCollider = GetComponent<BoxCollider2D>();
            if (boxCollider == null)
            {
                boxCollider = gameObject.AddComponent<BoxCollider2D>();
                // Set collider size based on sprite
                if (spriteRenderer.sprite != null)
                {
                    Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
                    boxCollider.size = spriteSize * 0.8f; // Slightly smaller than sprite
                    boxCollider.offset = new Vector2(0, 0); // Center the collider
                }
                else
                {
                    boxCollider.size = new Vector2(1f, 1f); // Default size if no sprite
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        startPosition = transform.position;
        Debug.Log($"Monkey initialized at position: {startPosition}");
    }

    public void JumpToTree(TreeObject tree)
    {
        if (!isJumping && !GameManager.Instance.isPaused && !GameManager.Instance.isGameOver)
        {
            Debug.Log($"Monkey jumping to tree at position: {tree.transform.position}");
            StartCoroutine(JumpAndCollect(tree));
        }
    }

    private IEnumerator JumpAndCollect(TreeObject tree)
    {
        isJumping = true;
        Vector3 targetPosition = tree.transform.position + new Vector3(0, 1f, 0);

        // Jump to tree
        float journeyLength = Vector3.Distance(transform.position, targetPosition);
        float startTime = Time.time;
        float distanceCovered = 0;

        while (distanceCovered < journeyLength)
        {
            if (GameManager.Instance.isPaused)
            {
                yield return new WaitUntil(() => !GameManager.Instance.isPaused);
                startTime = Time.time - (distanceCovered / jumpSpeed);
            }

            distanceCovered = (Time.time - startTime) * jumpSpeed;
            float fractionOfJourney = Mathf.Clamp01(distanceCovered / journeyLength);
            
            // Add a parabolic arc to the jump
            float height = Mathf.Sin(fractionOfJourney * Mathf.PI) * jumpHeight;
            Vector3 currentPos = Vector3.Lerp(transform.position, targetPosition, fractionOfJourney);
            currentPos.y += height;
            
            transform.position = currentPos;
            yield return null;
        }

        // Wait a moment at the tree
        yield return new WaitForSeconds(0.5f);

        // Handle the result
        // Handle the result
        if (tree.hasBanana)
        {
            GameManager.Instance.AddScore(1);

            // EARLY‐OUT: if we've revealed all bananas in n–1 trees, jump to next level:
            int total = LevelManager.Instance.TreeCount;
            int found = LevelManager.Instance.BananasRevealed;

            if (found >= total - 1)
            {
                Debug.Log($"All bananas found in {found} of {total} trees ⇒ skipping last tree.");
                LevelManager.Instance.NextLevel();
            }
            else if (LevelManager.Instance.AllBananasCollected())
            {
                // fallback if you also want the "reveal all" case
                LevelManager.Instance.NextLevel();
            }
        }
        else
        {
            GameManager.Instance.GameOver();
        }


        // Return to start position if not game over
        if (!GameManager.Instance.isGameOver)
        {
            startTime = Time.time;
            distanceCovered = 0;
            journeyLength = Vector3.Distance(transform.position, startPosition);

            while (distanceCovered < journeyLength)
            {
                if (GameManager.Instance.isPaused)
                {
                    yield return new WaitUntil(() => !GameManager.Instance.isPaused);
                    startTime = Time.time - (distanceCovered / returnSpeed);
                }

                distanceCovered = (Time.time - startTime) * returnSpeed;
                float fractionOfJourney = Mathf.Clamp01(distanceCovered / journeyLength);
                
                // Add a parabolic arc to the return
                float height = Mathf.Sin(fractionOfJourney * Mathf.PI) * jumpHeight;
                Vector3 currentPos = Vector3.Lerp(transform.position, startPosition, fractionOfJourney);
                currentPos.y += height;
                
                transform.position = currentPos;
                yield return null;
            }
        }
        GameManager.Instance.isInteracting = false;
        isJumping = false;
    }

    public void ResetPosition()
    {
        transform.position = startPosition;
        isJumping = false;
        Debug.Log($"Monkey reset to position: {startPosition}");
    }
} 