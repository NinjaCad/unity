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
    public List<Grid> grid = new List<Grid>();
    public List<Vector2> gridPositions = new List<Vector2>();

    [SerializeField] GameObject line;
    [SerializeField] GameObject circle;
    [SerializeField] GameObject sqaure;
    GameObject currentPrefab;

    public void Start()
    {
        numIterations = 8;
        cIndex = 0;
        sIndex = 0;
        
        SetUp();
        CreateGrid();
        
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
            Vector2 stickCenter = (points[sticks[j].point1].position + points[sticks[j].point2].position) / 2;
            Vector2 stickDir = (points[sticks[j].point1].position - points[sticks[j].point2].position).normalized;
            if (!points[sticks[j].point1].locked)
            {
                points[sticks[j].point1].position = stickCenter + stickDir * (sticks[j].length / 2);
            }
            if (!points[sticks[j].point2].locked)
            {
                points[sticks[j].point2].position = stickCenter - stickDir * (sticks[j].length / 2);
            }
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
        Vector2 stickDir = (points[pos1].position - points[pos2].position).normalized;
        if (length < (points[pos1].radius + points[pos2].radius) / 2 && pos1 != pos2)
        {
            if (!points[pos1].locked)
            {
                points[pos1].position += stickDir * ((((points[pos1].radius + points[pos2].radius) / 2) - length) / 2);
            }
            if (!points[pos2].locked)
            {
                points[pos2].position -= stickDir * ((((points[pos1].radius + points[pos2].radius) / 2) - length) / 2);
            }

            if (new Vector2(Mathf.Round(points[pos1].position.x), Mathf.Round(points[pos1].position.y)) != gridPositions[points[pos1].gridIndex])
            {
                grid[points[pos1].gridIndex].circles.RemoveAt(grid[points[pos1].gridIndex].circles.IndexOf(pos1));
                
                points[pos1].gridIndex = gridPositions.IndexOf(new Vector2(Mathf.Round(points[pos1].position.x), Mathf.Round(points[pos1].position.y)));
                grid[points[pos1].gridIndex].circles.Add(pos1);
            }
            if (new Vector2(Mathf.Round(points[pos2].position.x), Mathf.Round(points[pos2].position.y)) != gridPositions[points[pos2].gridIndex])
            {
                grid[points[pos2].gridIndex].circles.RemoveAt(grid[points[pos2].gridIndex].circles.IndexOf(pos2));
                
                points[pos2].gridIndex = gridPositions.IndexOf(new Vector2(Mathf.Round(points[pos2].position.x), Mathf.Round(points[pos2].position.y)));
                grid[points[pos2].gridIndex].circles.Add(pos2);
            }
        }
    }

    void CheckForCircles()
    {
        // Check surrounding squares
        for (int i = 0; i < points.Count; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                for (int k = -1; k < 2; k++)
                {
                    for (int l = 0; l < grid[gridPositions.IndexOf(gridPositions[points[i].gridIndex] + new Vector2(j, k))].circles.Count; l++)
                    {
                        Collisions(i, grid[gridPositions.IndexOf(gridPositions[points[i].gridIndex] + new Vector2(j, k))].circles[l]);
                    }
                }
            }
        }
    }

    void Spawn()
    {
        // Spawn Point If Mouse Clicked
        if (Input.GetMouseButtonDown(0))
        {
            points.Add(new Point() { position = Camera.main.ScreenToWorldPoint(Input.mousePosition), prevPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition), radius = circleRadius, locked = false });

            // Adds point to circles list
            points[points.Count - 1].gridIndex = gridPositions.IndexOf(new Vector2(Mathf.Round(points[points.Count - 1].position.x), Mathf.Round(points[points.Count - 1].position.y)));
            grid[points[points.Count - 1].gridIndex].circles.Add(points.Count - 1);
        }
    }

    void SetUp()
    {
        // Set Lengths for sticks based on distance
        for (int i = 0; i < sticks.Count; i++)
        {
            sticks[i].length = Vector2.Distance(points[sticks[i].point1].position, points[sticks[i].point2].position);
        }

        // Set Previous Positions to current position
        for (int i = 0; i < points.Count; i++)
        {
            points[i].prevPosition = points[i].position;
        }

        // Adds points to circles list in each grid
        for (int i = 0; i < points.Count; i++)
        {
            points[i].gridIndex = gridPositions.IndexOf(new Vector2(Mathf.Round(points[i].position.x), Mathf.Round(points[i].position.y))); // To make life easier
            grid[points[i].gridIndex].circles.Add(i);
        }
    }

    void CreateGrid()
    {
        // Find Screen info
        screenRatio = (float)Screen.width / (float)Screen.height;
        screenHeight = Mathf.Ceil(Camera.main.orthographicSize * 2) + 1;
        screenWidth = Mathf.Ceil(screenRatio * (screenHeight - 1)) + 1;

        // Create a new slot for every square in grid and create a list for grid positions
        for (int i = 0; i < screenWidth; i++)
        {
            for (int j = 0; j > -screenHeight; j--)
            {
                grid.Add(new Grid());
                gridPositions.Add(new Vector2(i - ((screenWidth - 1) / 2), j + Camera.main.orthographicSize));
                //Instantiate(sqaure, gridPositions[gridPositions.Count - 1], Quaternion.Euler(0, 0, 0));
            }
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
        public int gridIndex;
    }

    [System.Serializable]
    public class Stick
    {
        public int point1, point2;
        [HideInInspector] public float length;
    }

    [System.Serializable]
    public class Grid
    {
        public List<int> circles = new List<int>();
    }
}