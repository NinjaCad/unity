/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork : MonoBehaviour
{
    private int[] setLayers = new int[] { 1, 10, 1 };
    private List<float> layers;
    private List<List<float>> neurons;
    private List<List<List<float>>> weights;
    private float fitness;

    public float[] Compute(float[] inputs)
    {
        Initialize();
        
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }

        for (int i = 1; i < layers.Count; i++)
        {
            for (int j = 0; j < neurons[i].Count; j++)
            {
                float value = 0f;

                for (int k = 0; k < weights[i][j].Count; k++)
                {
                    value += weights[i][j][k] * neurons[i - 1][j];
                }
                
                neurons[i][j] = (float)Math.Tanh(value);
            }
        }
    }

    public void Initialize()
    {
        // Layers
        for (int i = 0; i < setLayers.Length; i++)
        {
            layers.Add(setLayers[i]);
        }

        // Neurons
        for (int i = 0; i < layers.Count; i++)
        {
            for (int j = 0; j < layers[i]; j++)
            {
                neurons[i].Add();
            }
        }

        // Weights
        for (int i = 0; i < layers.Count; i++)
        {
            for (int j = 0; j < neurons[i].Count; j++)
            {
                for (int k = 0; k < neurons[i + 1].Count; k++)
                {
                    weights[i][j].Add(Random.Range(-0.5f, 0.5f));
                }
            }
        }
    }
}
*/