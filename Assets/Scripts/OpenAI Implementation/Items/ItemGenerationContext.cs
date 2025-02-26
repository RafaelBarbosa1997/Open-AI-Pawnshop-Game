using System;
using UnityEngine;

/// <summary>
/// This class is used to generate a prompt for the AI to create an item.
/// The fields here contain messages to be added to the prompt that intructs the AI how to generate an item.
/// The content of these messages is stored in a JSON file.
/// </summary>
[Serializable]
public class ItemGenerationContext
{
    [SerializeField]
    private string scenario, inspiration, cliche, operationOrder, prices, command;

    [SerializeField]
    private string[] rules, resultFields;

    public string Scenario { get => scenario; private set => scenario = value; }
    public string Inspiration { get => inspiration; private set => inspiration = value; }
    public string Cliche { get => cliche; private set => cliche = value; }
    public string OperationOrder { get => operationOrder; private set => operationOrder = value; }
    public string Prices { get => prices; private set => prices = value; }
    public string Command { get =>  command; private set => command = value; }
    public string[] Rules { get => rules; private set => rules = value; }
    public string[] ResultFields { get => resultFields; private set => resultFields = value; }
}
