using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class Snake : MonoBehaviour
{
    public int xSize, ySize;
    public GameObject block;
    public Material headMaterial, tailMaterial;
    public Text points;
    public Text timerText;
    public GameObject gameOverUI;
    public Text totalFoodText;
    public Text mapEatenText;
    public GameObject textPrefab;
    public LineRenderer pathRenderer;

    private GameObject head;
    private List<GameObject> tail;
    private Vector2 dir;
    private GameObject food;
    private bool isAlive;
    private float movementTimer, timeBetweenMovements;
    private float elapsedTime;
    private GameObject borderFolder;
    private GameObject snakeFolder;
    private int foodEaten;
    private int totalFoodSpots;

    private readonly Vector2[] directions = new Vector2[]
    {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right
    };

    private int patrolDirectionIndex = 0;
    private string logFilePath;

    void Start()
    {
        logFilePath = Application.dataPath + "/SnakeGameLog.csv";
        InitializeGame();
    }

    void Update()
    {
        if (isAlive)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerDisplay();

            movementTimer += Time.deltaTime;
            if (movementTimer >= timeBetweenMovements)
            {
                movementTimer = 0;
                MoveTowardsFood();
            }
        }
    }

    private void InitializeGame()
    {
        // Clean up previous game objects if they exist
        if (borderFolder != null) Destroy(borderFolder);
        if (snakeFolder != null) Destroy(snakeFolder);

        // Initialize game state variables
        timeBetweenMovements = 0.15f;
        dir = Vector2.right;
        tail = new List<GameObject>();
        borderFolder = new GameObject("BorderBlocks");
        snakeFolder = new GameObject("SnakeParts");
        isAlive = true;
        elapsedTime = 0f;
        foodEaten = 0;
        totalFoodSpots = xSize * ySize;
        block.SetActive(true);

        // Setup the game grid and snake
        CreateGrid();
        CreatePlayer();
        SpawnFood();

        block.SetActive(false);

        UpdateTotalFoodText(totalFoodSpots);

        // Initialize and configure the path renderer
        if (pathRenderer == null)
        {
            GameObject pathObject = new GameObject("PathRenderer");
            pathRenderer = pathObject.AddComponent<LineRenderer>();
        }
        ConfigurePathRenderer();
    }

    private void ConfigurePathRenderer()
    {
        pathRenderer.startWidth = 0.1f;
        pathRenderer.endWidth = 0.1f;
        pathRenderer.material = new Material(Shader.Find("Sprites/Default"));
        pathRenderer.startColor = Color.cyan;
        pathRenderer.endColor = Color.cyan;
        pathRenderer.sortingOrder = 5;
    }

    private void CreateGrid()
    {
        for (int x = -1; x <= xSize; x++)
        {
            CreateBlock(new Vector3(x, -1, 0));
            CreateBlock(new Vector3(x, ySize, 0));
        }
        for (int y = -1; y <= ySize; y++)
        {
            CreateBlock(new Vector3(-1, y, 0));
            CreateBlock(new Vector3(xSize, y, 0));
        }
    }

    private void CreateBlock(Vector3 position)
    {
        GameObject borderBlock = Instantiate(block, borderFolder.transform);
        borderBlock.transform.position = position;
        borderBlock.tag = "Border";
        borderBlock.AddComponent<BoxCollider2D>();
    }

    private void CreatePlayer()
    {
        head = Instantiate(block, snakeFolder.transform);
        head.transform.position = new Vector2(0, 0);
        head.tag = "SnakePart";
        head.GetComponent<MeshRenderer>().material = headMaterial;
        head.AddComponent<BoxCollider2D>().isTrigger = true;
    }

    private Vector2 GetRandomPos()
    {
        return new Vector2(Random.Range(0, xSize), Random.Range(0, ySize));
    }

    private bool IsPositionOccupied(Vector2 pos)
    {
        return pos == (Vector2)head.transform.position || tail.Exists(item => (Vector2)item.transform.position == pos);
    }

    private void SpawnFood()
    {
        Vector2 spawnPos = GetRandomPos();
        while (IsPositionOccupied(spawnPos))
        {
            spawnPos = GetRandomPos();
        }
        if (food != null)
        {
            Destroy(food);
        }
        food = Instantiate(block);
        food.transform.position = new Vector3(spawnPos.x, spawnPos.y, 0);
        food.SetActive(true);
        food.tag = "Food";
        food.GetComponent<MeshRenderer>().material.color = new Color(Random.value, Random.value, Random.value);
        food.AddComponent<BoxCollider2D>().isTrigger = true;
    }

    private void MoveTowardsFood()
    {
        Vector2 nextMove = FindBestPathToFood();
        if (nextMove == Vector2.zero) // No path found, start patrolling
        {
            nextMove = PatrolWithPurpose();
        }

        dir = nextMove;
        MoveSnake();
    }

    private Vector2 FindBestPathToFood()
    {
        Vector2 start = head.transform.position;
        Vector2 goal = food.transform.position;

        List<Vector2> openSet = new List<Vector2> { start };
        Dictionary<Vector2, Vector2> cameFrom = new Dictionary<Vector2, Vector2>();
        Dictionary<Vector2, float> gScore = new Dictionary<Vector2, float> { [start] = 0 };
        Dictionary<Vector2, float> fScore = new Dictionary<Vector2, float> { [start] = Vector2.Distance(start, goal) };

        while (openSet.Count > 0)
        {
            Vector2 current = GetLowestScoreNode(openSet, fScore);
            if (current == goal)
            {
                List<Vector2> path = ReconstructPath(cameFrom, current);
                UpdatePathRenderer(path);
                return path[1] - start;
            }

            openSet.Remove(current);

            foreach (var direction in directions)
            {
                Vector2 neighbor = current + direction;

                if (!IsSafeMove(neighbor))
                {
                    continue;
                }

                float tentativeGScore = gScore[current] + Vector2.Distance(current, neighbor);

                if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + Vector2.Distance(neighbor, goal);

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        return Vector2.zero; // No path found
    }

    private Vector2 GetLowestScoreNode(List<Vector2> openSet, Dictionary<Vector2, float> fScore)
    {
        float minScore = float.MaxValue;
        Vector2 lowestNode = openSet[0];

        foreach (var node in openSet)
        {
            float score = fScore.ContainsKey(node) ? fScore[node] : float.MaxValue;
            if (score < minScore)
            {
                minScore = score;
                lowestNode = node;
            }
        }

        return lowestNode;
    }

    private List<Vector2> ReconstructPath(Dictionary<Vector2, Vector2> cameFrom, Vector2 current)
    {
        List<Vector2> totalPath = new List<Vector2> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }
        return totalPath;
    }

    private Vector2 PatrolWithPurpose()
    {
        Vector2 bestDirection = Vector2.zero;
        float maxOpenSpace = float.MinValue;

        foreach (var direction in directions)
        {
            Vector2 potentialPosition = (Vector2)head.transform.position + direction;
            if (IsSafeMove(potentialPosition))
            {
                float openSpace = CalculateOpenSpace(potentialPosition);
                if (openSpace > maxOpenSpace)
                {
                    maxOpenSpace = openSpace;
                    bestDirection = direction;
                }
            }
        }

        if (bestDirection != Vector2.zero)
        {
            return bestDirection;
        }

        patrolDirectionIndex = (patrolDirectionIndex + 1) % directions.Length;
        return directions[patrolDirectionIndex];
    }

    private float CalculateOpenSpace(Vector2 position)
    {
        float openSpace = 0;

        foreach (var direction in directions)
        {
            Vector2 checkPosition = position + direction;
            if (IsSafeMove(checkPosition))
            {
                openSpace += 1;
            }
        }

        return openSpace;
    }

    private bool IsSafeMove(Vector2 nextPosition)
    {
        return nextPosition.x >= 0 && nextPosition.x < xSize &&
               nextPosition.y >= 0 && nextPosition.y < ySize &&
               !IsPositionOccupied(nextPosition);
    }

    private void MoveSnake()
    {
        Vector3 newPosition = head.transform.position + new Vector3(dir.x, dir.y, 0);

        if (CheckCollision(newPosition))
        {
            Debug.Log("Collision detected. Logging data and restarting.");
            LogGameData(); // Log the data
            //Invoke("RestartGame", 1f); // Restart the game after 2 seconds
            RestartGame();
            return;
        }

        if (newPosition == food.transform.position)
        {
            GrowSnake(newPosition);
            SpawnFood();
            foodEaten++;
            UpdateMapEatenText(foodEaten, totalFoodSpots);
            points.text = "Points: " + tail.Count;
        }
        else
        {
            MoveHead(newPosition);
        }
    }

    private void LogGameData()
    {
        string data = $"{elapsedTime:F2},{GetSolvedPercentage():F2},{foodEaten},{totalFoodSpots}";
        if (!File.Exists(logFilePath))
        {
            File.WriteAllText(logFilePath, "Time,Percentage,EatenFood,totalFoodSpot\n");
        }
        File.AppendAllText(logFilePath, data + "\n");
    }

    private float GetSolvedPercentage()
    {
        return (foodEaten / (float)totalFoodSpots) * 100f;
    }

    private void RestartGame()
    {
        InitializeGame();
    }

    private void GrowSnake(Vector3 newPosition)
    {
        GameObject newTile = Instantiate(block, snakeFolder.transform);
        newTile.SetActive(true);
        newTile.transform.position = newPosition;
        newTile.tag = "SnakePart";
        head.GetComponent<MeshRenderer>().material = tailMaterial;
        tail.Add(head);
        head = newTile;
        head.GetComponent<MeshRenderer>().material = headMaterial;
        head.AddComponent<BoxCollider2D>().isTrigger = true;
    }

    private void MoveHead(Vector3 newPosition)
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

    private void GameOver()
    {
        isAlive = false;
        gameOverUI.SetActive(true);
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = "Time: " + Mathf.FloorToInt(elapsedTime).ToString();
        }
    }

    private void UpdateTotalFoodText(int totalFoodSpots)
    {
        if (totalFoodText != null)
        {
            totalFoodText.text = "Total Food Spots: " + totalFoodSpots.ToString();
        }
    }

    private void UpdateMapEatenText(int foodEaten, int totalFoodSpots)
    {
        if (mapEatenText != null)
        {
            float mapEatenPercentage = (foodEaten / (float)totalFoodSpots) * 100f;
            mapEatenText.text = "Solved: " + mapEatenPercentage.ToString("F2") + "%";
        }
    }

    private bool CheckCollision(Vector3 newPosition)
    {
        Collider2D[] colliders = Physics2D.OverlapPointAll(newPosition);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Border") || collider.CompareTag("SnakePart"))
            {
                return true;
            }
        }
        return false;
    }

    private void UpdatePathRenderer(List<Vector2> path)
    {
        if (pathRenderer != null && path.Count > 0)
        {
            pathRenderer.positionCount = path.Count;
            for (int i = 0; i < path.Count; i++)
            {
                pathRenderer.SetPosition(i, new Vector3(path[i].x, path[i].y, 0));
            }
        }
    }
}
