using System;
using UnityEngine;

/// <summary>
/// This class contains the required variables for making a chat completion request with AI.
/// There are JSON files representing this class for each type of needed request.
/// </summary>
[Serializable]
public class OpenaiRequestData
{
    [SerializeField]
    private int maxTokens;
    [SerializeField]
    private float temperature;
    [SerializeField]
    private string model;

    public int MaxTokens { get => maxTokens; private set => maxTokens = value; }
    public float Temperature { get => temperature; private set => temperature = value; }
    public string Model { get => model; private set => model = value; }
}
