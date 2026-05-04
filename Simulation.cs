using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    public float gravity, areaRadius, circleRadius;

    float screenRatio, screenHeight, screenWidth;
    int cIndex, sIndex, numIterations;

    public List<Point> points = new List<Point>();
    public List<Stick> sticks = new List<Stick>();

    [SerializeField] GameObject line;
    [SerializeField] GameObject circle;
    [SerializeField] GameObject sqaure;
    GameObject currentPrefab;

    float cellSize;
    Dictionary<Vector2Int, List<int>> grid = new Dictionary<Vector2Int, List<int>>();
    public Vector2Int cell;

    public void Start()
    {
        numIterations = 8;
        cIndex = 0;
        sIndex = 0;

        cellSize = circleRadius * 2f;
        
        SetUp();
        
        Debug.Log("Start Done (Simulation)");
    }

    void Update()
    {
        Spawn();
        Draw();
    }

    void FixedUpdate()
    {
        Simulate();
        UpdateAllGridCells();
        ScreenCollisions();

        for (int i = 0; i < numIterations; i++)
        {
            Constraints();
        }

        for (int i = 0; i < numIterations; i++)
        {
            CheckForCircles();
        }
    }

    void Simulate()
    {
        // Physics (Verlet Integration): Move points based on previous position
        for (int i = 0; i < points.Count; i++)
        {
            if (!points[i].locked)
            {
                Vector2 positionBeforeUpdate = points[i].position;

                points[i].position += points[i].position - points[i].prevPosition;
                points[i].position += Vector2.down * gravity * Time.deltaTime;

                points[i].prevPosition = positionBeforeUpdate;
            }
        }
    }

    void Constraints()
    {
        // Rope: constraints points to be a certain distance away from each other
        for (int j = 0; j < sticks.Count; j++)
        {
            Point p1 = points[sticks[j].point1];
            Point p2 = points[sticks[j].point2];

            Vector2 delta = p1.position - p2.position;
            float dist = delta.magnitude;

            if (dist == 0) continue;

            Vector2 dir = delta / dist;
            float error = dist - sticks[j].length;

            Vector2 correction = dir * error * 0.5f;

            if (!p1.locked)
                p1.position -= correction;

            if (!p2.locked)
                p2.position += correction;
        }

        // Circle Area where circles can be
        for (int i = 0; i < points.Count; i++)
        {
            float length = Vector2.Distance(points[i].position, transform.position);
            Vector2 stickDir = (points[i].position - (Vector2)transform.position).normalized;
            if (!points[i].locked && length > areaRadius)
            {
                points[i].position -= stickDir * (length - areaRadius);
            }
        }
    }

    void Collisions(int pos1, int pos2)
    {
        // Finds the distance of points and moves them away if they have overlapping radii.
        float length = Vector2.Distance(points[pos1].position, points[pos2].position);
        Vector2 delta = points[pos1].position - points[pos2].position;
        float dist = delta.magnitude;

        if (dist == 0) return;

        Vector2 stickDir = delta / dist;

        float minDist = (points[pos1].radius + points[pos2].radius) / 2;

        if (length < minDist && pos1 != pos2)
        {
            float correction = (minDist - length) / 2;

            if (!points[pos1].locked)
            {
                points[pos1].position += stickDir * correction;
            }

            if (!points[pos2].locked)
            {
                points[pos2].position -= stickDir * correction;
            }
        }
    }

    void CheckForCircles()
    {
        foreach (var cell in grid)
        {
            Vector2Int cellPos = cell.Key;
            List<int> cellPoints = cell.Value;

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Vector2Int neighbor = cellPos + new Vector2Int(x, y);

                    if (!grid.ContainsKey(neighbor)) continue;

                    List<int> neighborPoints = grid[neighbor];

                    for (int i = 0; i < cellPoints.Count; i++)
                    {
                        for (int j = 0; j < neighborPoints.Count; j++)
                        {
                            int a = cellPoints[i];
                            int b = neighborPoints[j];

                            if (a >= b) continue;

                            Collisions(a, b);
                        }
                    }
                }
            }
        }
    }

    Vector2Int GetCell(Vector2 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / cellSize),
            Mathf.FloorToInt(position.y / cellSize)
        );
    }

    void AddToCell(int index, Vector2Int cell)
    {
        if (!grid.ContainsKey(cell))
            grid[cell] = new List<int>();

        grid[cell].Add(index);
    }

    void RemoveFromCell(int index, Vector2Int cell)
    {
        if (!grid.ContainsKey(cell)) return;

        grid[cell].Remove(index);

        if (grid[cell].Count == 0)
            grid.Remove(cell);
    }

    void UpdateAllGridCells()
    {
        grid.Clear();

        for (int i = 0; i < points.Count; i++)
        {
            Vector2Int cell = GetCell(points[i].position);
            points[i].cell = cell;

            AddToCell(i, cell);
        }
    }

    void GetScreenBounds(out float left, out float right, out float bottom, out float top)
    {
        Camera cam = Camera.main;

        float height = cam.orthographicSize * 2f;
        float width = height * cam.aspect;

        Vector2 center = cam.transform.position;

        left = center.x - width / 2f;
        right = center.x + width / 2f;
        bottom = center.y - height / 2f;
        top = center.y + height / 2f;
    }

    void ScreenCollisions()
    {
        GetScreenBounds(out float left, out float right, out float bottom, out float top);

        float bounce = 0.7f;

        for (int i = 0; i < points.Count; i++)
        {
            Point p = points[i];

            Vector2 pos = p.position;
            Vector2 prev = p.prevPosition;

            Vector2 velocity = pos - prev;

            // LEFT
            if (pos.x < left)
            {
                pos.x = left;
                velocity.x *= -bounce;
            }

            // RIGHT
            if (pos.x > right)
            {
                pos.x = right;
                velocity.x *= -bounce;
            }

            // BOTTOM
            if (pos.y < bottom)
            {
                pos.y = bottom;
                velocity.y *= -bounce;
            }

            // TOP
            if (pos.y > top)
            {
                pos.y = top;
                velocity.y *= -bounce;
            }

            p.position = pos;
            p.prevPosition = pos - velocity;
        }
    }

    void Spawn()
    {
        // Spawn Point If Mouse Clicked
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 spawnPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            points.Add(new Point()
            {
                position = spawnPos,
                prevPosition = spawnPos,
                radius = circleRadius,
                locked = false
            });

            points.Add(p);

            int index = points.Count - 1;

            Vector2Int cell = GetCell(spawnPos);
            p.cell = cell;

            AddToCell(index, cell);
        }
    }

    void SetUp()
    {
        // Set Lengths for sticks based on distance
        for (int i = 0; i < sticks.Count; i++)
        {
            sticks[i].length = Vector2.Distance(
                points[sticks[i].point1].position,
                points[sticks[i].point2].position
            );
        }

        // Set Previous Positions to current position
        for (int i = 0; i < points.Count; i++)
        {
            points[i].prevPosition = points[i].position;
        }

        // Initialize Cells
        for (int i = 0; i < points.Count; i++)
        {
            Vector2Int cell = GetCell(points[i].position);
            points[i].cell = cell;
            AddToCell(i, cell);
        }
    }

    void Draw()
    {
        // Draw Sticks
        if (sIndex < sticks.Count)
        {
            currentPrefab = Instantiate(line, new Vector2(0, 0), Quaternion.Euler(0, 0, 0));
            currentPrefab.GetComponent<Rendering>().sim = this;
            currentPrefab.GetComponent<Rendering>().lineIndex = sIndex; 
            sIndex++;
        }

        // Draw Circles
        if (cIndex < points.Count)
        {
            currentPrefab = Instantiate(circle, new Vector2(0, 0), Quaternion.Euler(0, 0, 0));
            currentPrefab.GetComponent<CircleRendering>().sim = this;
            currentPrefab.GetComponent<CircleRendering>().circleIndex = cIndex;
            cIndex++;
            Debug.Log("Point " + cIndex + " Added");
        }
    }

    [System.Serializable]
    public class Point
    {
        public Vector2 position;
        [HideInInspector] public Vector2 prevPosition;
        public float radius;
        public bool locked;

        public Vector2Int cell;
    }

    [System.Serializable]
    public class Stick
    {
        public int point1, point2;
        [HideInInspector] public float length;
    }
}