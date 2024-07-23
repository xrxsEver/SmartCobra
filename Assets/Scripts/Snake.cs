using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Snake : MonoBehaviour
{
    public int xSize, ySize;
    public GameObject block;
    public Material headMaterial, tailMaterial;
    public Text points;
    public Text timerText;
    public GameObject gameOverUI;

    private GameObject head;
    private List<GameObject> tail;
    private Vector2 dir;
    private GameObject food;
    private bool isAlive;
    private float movementTimer, timeBetweenMovements;
    private float elapsedTime; 

    // GameObject folders
    private GameObject borderFolder;
    private GameObject snakeFolder;

    void Start()
    {
        timeBetweenMovements = 0.2f;
        dir = Vector2.right;
        tail = new List<GameObject>();

        // Create folders for organization
        borderFolder = new GameObject("BorderBlocks");
        snakeFolder = new GameObject("SnakeParts");

        createGrid();
        createPlayer();
        spawnFood();
        block.SetActive(false);
        isAlive = true;
        elapsedTime = 0f;

        if (timerText == null)
        {
            Debug.LogError("Timer Text UI element is not assigned.");
        }
    }

    private Vector2 getRandomPos()
    {
        return new Vector2(Random.Range(-xSize / 2 + 1, xSize / 2), Random.Range(-ySize / 2 + 1, ySize / 2));
    }

    private bool containedInSnake(Vector2 pos)
    {
        bool isInHead = pos == (Vector2)head.transform.position;
        bool isInTail = tail.Exists(item => (Vector2)item.transform.position == pos);
        return isInTail || isInHead;
    }

    private void spawnFood()
    {
        Vector2 spawnPos = getRandomPos();
        while (containedInSnake(spawnPos))
        {
            spawnPos = getRandomPos();
        }
        food = Instantiate(block);
        food.transform.position = new Vector3(spawnPos.x, spawnPos.y, 0);
        food.SetActive(true);
        // Randomize food color
        food.GetComponent<MeshRenderer>().material.color = new Color(Random.value, Random.value, Random.value);
    }

    private void createPlayer()
    {
        head = Instantiate(block, snakeFolder.transform);
        head.GetComponent<MeshRenderer>().material = headMaterial;
    }

    private void createGrid()
    {
        for (int x = 0; x <= xSize; x++)
        {
            createBlock(new Vector3(x - xSize / 2, -ySize / 2, 0));
            createBlock(new Vector3(x - xSize / 2, ySize - ySize / 2, 0));
        }
        for (int y = 0; y <= ySize; y++)
        {
            createBlock(new Vector3(-xSize / 2, y - ySize / 2, 0));
            createBlock(new Vector3(xSize - xSize / 2, y - ySize / 2, 0));
        }
    }

    private void createBlock(Vector3 position)
    {
        GameObject borderBlock = Instantiate(block, borderFolder.transform);
        borderBlock.transform.position = position;
    }

    void Update()
    {
        if (isAlive)
        {
            // Update elapsed time
            elapsedTime += Time.deltaTime;
            UpdateTimerDisplay(); 

            movementTimer += Time.deltaTime;
            if (movementTimer >= timeBetweenMovements)
            {
                movementTimer = 0;
                moveSnake();
            }

            handleInput();
        }
    }

    private void handleInput()
    {
        if (Input.GetKey(KeyCode.DownArrow) && dir != Vector2.up)
        {
            dir = Vector2.down;
        }
        else if (Input.GetKey(KeyCode.UpArrow) && dir != Vector2.down)
        {
            dir = Vector2.up;
        }
        else if (Input.GetKey(KeyCode.RightArrow) && dir != Vector2.left)
        {
            dir = Vector2.right;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && dir != Vector2.right)
        {
            dir = Vector2.left;
        }
    }

    private void moveSnake()
    {
        Vector3 newPosition = head.transform.position + new Vector3(dir.x, dir.y, 0);

        if (checkCollision(newPosition))
        {
            gameOver();
            return;
        }

        if (newPosition == food.transform.position)
        {
            growSnake(newPosition);
            spawnFood();
            points.text = "Points: " + tail.Count;
        }
        else
        {
            moveHead(newPosition);
        }
    }

    private bool checkCollision(Vector3 newPosition)
    {
        if (newPosition.x >= xSize / 2 || newPosition.x <= -xSize / 2 ||
            newPosition.y >= ySize / 2 || newPosition.y <= -ySize / 2)
        {
            return true;
        }

        foreach (var item in tail)
        {
            if (item.transform.position == newPosition)
            {
                return true;
            }
        }

        return false;
    }

    private void growSnake(Vector3 newPosition)
    {
        GameObject newTile = Instantiate(block, snakeFolder.transform);
        newTile.SetActive(true);
        newTile.transform.position = food.transform.position;
        DestroyImmediate(food);
        head.GetComponent<MeshRenderer>().material = tailMaterial;
        tail.Add(head);
        head = newTile;
        head.GetComponent<MeshRenderer>().material = headMaterial;
    }

    private void moveHead(Vector3 newPosition)
    {
        if (tail.Count > 0)
        {
            head.GetComponent<MeshRenderer>().material = tailMaterial;
            tail.Add(head);
            head = tail[0];
            head.GetComponent<MeshRenderer>().material = headMaterial;
            tail.RemoveAt(0);
        }
        head.transform.position = newPosition;
    }

    private void gameOver()
    {
        isAlive = false;
        gameOverUI.SetActive(true);
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null) // Check if timerText is assigned
        {
            timerText.text = "Time: " + Mathf.FloorToInt(elapsedTime).ToString();
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }
}
