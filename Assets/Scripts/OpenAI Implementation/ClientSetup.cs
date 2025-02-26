using System;
using UnityEngine;

/// <summary>
/// This class contains the information needed to initialize a new client.
/// It is fed to the client class by the game manager.
/// </summary>
[Serializable]
public class ClientSetup
{
    [SerializeField, TextArea(1, 100)]
    private string initialPrompt, rules, personalityPrompt, responseReasoning, reasoningRules;

    [SerializeField]
    private OpenaiRequestData requestData;

    public string InitialPrompt { get => initialPrompt; set => initialPrompt = value; }
    public string Rules { get => rules; set => rules = value; }
    public string PersonalityPrompt { get => personalityPrompt; set => personalityPrompt = value; }
    public OpenaiRequestData RequestData { get => requestData; private set => requestData = value; }
    public string ResponseReasoning { get => responseReasoning; private set => responseReasoning = value; }
    public string ReasoningRules { get => reasoningRules; private set => reasoningRules = value; }
}
