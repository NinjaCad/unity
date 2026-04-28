using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleRendering : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    [HideInInspector] public Simulation sim;
    [HideInInspector] public int circleIndex;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Start()
    {
        spriteRenderer.color = new Color(Random.Range(0f, 255f) / 255f, Random.Range(0f, 255f) / 255f, Random.Range(0f, 255f) / 255f, 1f);
    }

    void LateUpdate()
    {
        Render();
    }

    void Render()
    {
        // Change Radius
        transform.localScale = new Vector2(sim.points[circleIndex].radius, sim.points[circleIndex].radius);

        // Set Position
        transform.position = sim.points[circleIndex].position;
    }
}
