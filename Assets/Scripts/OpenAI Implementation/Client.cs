using System.Collections.Generic;
using OpenAI;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using System;

public class Client
{
    private string initialPrompt, rules, personalityPrompt, responseReasoning, reasoningRules;

    private OpenAIApi openAIApi;
    private OpenaiRequestData requestData;
    private List<ChatMessage> chatLog;

    private GameManager gameManager;

    #region SETUP
    /// <summary>
    /// Constructor takes in client setup to be able to generate a client.
    /// </summary>
    /// <param name="setup"></param>
    public Client(ClientSetup setup, GameManager gameManager)
    {
        initialPrompt = setup.InitialPrompt;
        rules = setup.Rules;
        personalityPrompt = setup.PersonalityPrompt;
        responseReasoning = setup.ResponseReasoning;
        reasoningRules = setup.ReasoningRules;

        requestData = setup.RequestData;

        this.gameManager = gameManager;
    }

    /// <summary>
    /// Creates a personality for a client.
    /// Returns a string with 5 personality traits and an explanation for each one.
    /// </summary>
    /// <returns></returns>
    private async Task<string> CreatePersonality()
    {
        var completionResponse = await openAIApi.CreateChatCompletion(new CreateChatCompletionRequest
        {
            Model = requestData.Model,
            MaxTokens = requestData.MaxTokens,
            Temperature = requestData.Temperature,

            Messages = new List<ChatMessage>
            {
                new ChatMessage
                {
                    Role = "system",
                    Content = personalityPrompt
                }
            }
        });

        return completionResponse.Choices[0].Message.Content;
    }
    #endregion

    #region REASONING
    /// <summary>
    /// Goes through the current chat log and does reasoning to determine the client's next response.
    /// Returns a class with information on the results of said reasoning.
    /// </summary>
    /// <returns></returns>
    private async Task<ReasoningResultInfo> ReasonResponse()
    {
        // Create temporary message list so reasoning results are not added to main chat log.
        List<ChatMessage> reasoningLog = new List<ChatMessage>();
        reasoningLog.AddRange(chatLog);

        // Add reasoning prompt.
        reasoningLog.Add(new ChatMessage
        {
            Role = "system",
            Content = responseReasoning
        });

        // Add reasoning rules.
        reasoningLog.Add(new ChatMessage
        {
            Role = "system",
            Content = reasoningRules
        });

        // Initialize reasoning results class.
        ReasoningResultInfo reasoningResultInfo = null;

        // While loop to make sure JSON string is valid.
        bool jsonValid = false;
        do
        {
            // Chat completion to return AI's reasoning.
            var reasoningResponse = await openAIApi.CreateChatCompletion(new CreateChatCompletionRequest
            {
                Model = requestData.Model,
                MaxTokens = requestData.MaxTokens,
                Temperature = requestData.Temperature,


                Messages = reasoningLog
            });

            Debug.Log(reasoningResponse.Choices[0].Message.Content);

            // Get AI's JSON response and convert it to reasoning result class.
            string jsonString = reasoningResponse.Choices[0].Message.Content;

            // Only exit loop if we successfly get a json class from the AI.
            try
            {
                reasoningResultInfo = JsonUtility.FromJson<ReasoningResultInfo>(jsonString);

                jsonValid = true;
            }

            catch(Exception exception)
            {
                Debug.Log("Retrying JSON");

                await Task.Delay(500);
            }

        } while (!jsonValid);

        return reasoningResultInfo;
    }

    /// <summary>
    /// Adds context to instructions for AI's next dialogue before they respond, based on their reasoning results.
    /// Returns a list of actions to perform also based on that reasoning.
    /// </summary>
    /// <param name="reasoningResult"></param>
    /// <returns></returns>
    private List<UnityAction> ReasonActions(ReasoningResultInfo reasoningResult)
    {
        List<UnityAction> actionList = new List<UnityAction>();

        // If client has been upset enough to cancel the deal and leave.
        if (reasoningResult.Outraged)
        {
            // Add context to reflect them leaving without selling to the chat log.
            chatLog.Add(new ChatMessage
            {
                Role = "system",
                Content = "You are outraged with the client's response and decide to leave without selling. Respond showing your dissatisfaction before leaving."
            });

            // Action list cancels the deal.
            actionList.Add(() => gameManager.CancelDeal());

            return actionList;
        }

        // If client determines a deal has been made.
        if (reasoningResult.DealMade)
        {
            // Add context to reflect a new deal being made with the chosen price.
            chatLog.Add(new ChatMessage
            {
                Role = "system",
                Content = "A deal has been made with the price of " + reasoningResult.NewPrice + "Your response must reflect the deal being made with that price."
            });

            // Action list changes offer to agreed price and closes deal.
            actionList.Add(() => gameManager.ChangeCurrentClientOffer(reasoningResult.NewPrice));
            actionList.Add(() => gameManager.CloseDeal());

            return actionList;
        }

        // If client decided to change their offer.
        if(reasoningResult.ChangedPrice)
        {
            // Add context to reflect them changing to their new offering price.
            chatLog.Add(new ChatMessage
            {
                Role = "system",
                Content = "Change the price you're offering to " + reasoningResult.NewPrice + "Your response must reflect the fact that you changed the offering price."
            });

            // Action list changes the offering price.
            actionList.Add(() => gameManager.ChangeCurrentClientOffer(reasoningResult.NewPrice));

            return actionList;
        }

        // If client did not change their offer.
        else
        {
            // Add context to reflect them keeping the same offering price.
            chatLog.Add(new ChatMessage
            {
                Role = "system",
                Content = "Do not lower your offering price, keep it at " + reasoningResult.NewPrice + "Your response must reflect the fact that you did not change the offering price."
            });

            // No actions for this scenario.
            return null;
        }
    }
    #endregion

    #region DIALOGUE
    /// <summary>
    /// Creates the client's initial dialogue.
    /// Setups up the initial prompt for the AI to know how to play its part and returns its first response.
    /// </summary>
    /// <param name="itemToSell"></param>
    /// <returns></returns>
    public async Task<string> CreateInitialDialogue(Item itemToSell)
    {
        openAIApi = new OpenAIApi();

        // First get the string explaining the client's personality.
        string personality = await CreatePersonality();

        // Add the personality to the initial prompt.
        initialPrompt += "\n The client has a set personality and you must keep the dialogue coherent with it. Here are the client's personality traits:\n" + personality;

        // Add information about the item client is trying to sell.
        initialPrompt += "\n" + "Here is the information about the item you're selling:\n" + "Item name: " + itemToSell.Name + "\nItem description: " + itemToSell.Description + "\n" + itemToSell.Effect + "\nThe price you're offering: " + itemToSell.ClientOffer;

        // Add rules AI should follow.
        initialPrompt += "\n" + rules;

        // Initialize chat log that AI uses to remember conversation and add the created initial prompt to it.
        chatLog = new List<ChatMessage>()
        {
            new ChatMessage
            {
                Role = "system",
                Content = initialPrompt
            }
        };

        // Have the AI create the first dialogue.
        var completionResponse = await openAIApi.CreateChatCompletion(new CreateChatCompletionRequest
        {
            Model = requestData.Model,
            MaxTokens = requestData.MaxTokens,
            Temperature = requestData.Temperature,

            Messages = chatLog
        });

        // Add the first dialogue to the chat log.
        chatLog.Add(completionResponse.Choices[0].Message);

        return completionResponse.Choices[0].Message.Content;
    }

    /// <summary>
    /// Receives player's message and returns the client's response.
    /// AI's response will be based on parameters chosen by reasoning which will also call methods directly affecting game mechanics.
    /// </summary>
    /// <param name="playerResponse"></param>
    /// <returns></returns>
    public async Task<string> RespondToPlayer(string playerResponse)
    {
        // Add player's message to the message list.
        chatLog.Add(new ChatMessage
        {
            Role = "user",
            Content = playerResponse
        });

        // Use reasoning to get a class representing how the client should act according to player's message.
        ReasoningResultInfo reasoning = await ReasonResponse();

        // Use reasoning results to alter client's response and get a list of actions that should be performed.
        List<UnityAction> reasonActions = ReasonActions(reasoning);

        // Have AI create a dialogue response to player.
        var completionResponse = await openAIApi.CreateChatCompletion(new CreateChatCompletionRequest
        {
            Model = requestData.Model,
            MaxTokens = requestData.MaxTokens,
            Temperature = requestData.Temperature,

            Messages = chatLog
        });

        // Add AI's response to chat log.
        chatLog.Add(completionResponse.Choices[0].Message);

        // If there are actions to be performed, perform them.
        if(reasonActions != null)
        {
            foreach(UnityAction action in reasonActions)
            {
                action.Invoke();
            }
        }

        return completionResponse.Choices[0].Message.Content;
    }
    #endregion
}
