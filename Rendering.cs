using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rendering : MonoBehaviour
{
    [HideInInspector] public Simulation sim;
    LineRenderer lineRenderer;
    [HideInInspector] public int lineIndex;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void LateUpdate()
    {
        Render();
    }

    void Render()
    {
        // Line Settings
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        // Draw Line
        lineRenderer.SetPosition(0, sim.points[sim.sticks[lineIndex].point1].position);
        lineRenderer.SetPosition(1, sim.points[sim.sticks[lineIndex].point2].position);
    }
}
