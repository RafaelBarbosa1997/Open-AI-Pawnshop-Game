using System;
using UnityEngine;

/// <summary>
/// This class stores the results of the client's reasoning in regards to the player's response.
/// It will use these results to formulate the next response and to interact with the game mechanics as needed.
/// </summary>
[Serializable]
public class ReasoningResultInfo
{
    [SerializeField]
    private bool outraged, changedPrice, dealMade;
    [SerializeField]
    private float newPrice;

    public bool Outraged { get => outraged; private set => outraged = value; }
    public float NewPrice { get => newPrice; private set => newPrice = value; }
    public bool ChangedPrice { get => changedPrice; private set => changedPrice = value; }
    public bool DealMade { get => dealMade; private set => dealMade = value; }
}
