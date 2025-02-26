using System;
using System.Collections.Generic;
using UnityEngine;
using OpenAI;
using System.IO;
using System.Threading.Tasks;

[Serializable]
public class ItemGeneration
{
    private const string contextPath = "/JSON Info/ItemGenerationContext.json";

    [SerializeField]
    private string[] inspirations, cliches;

    [SerializeField]
    private float minimumPrice, maximumPrice;

    [SerializeField]
    private OpenaiRequestData requestData;

    private string prompt;

    private OpenAIApi openAIApi;

    #region APPENDING
    /// <summary>
    /// Append a context message to the prompt.
    /// </summary>
    /// <param name="stringToAppend"></param>
    private void AppendContext(string stringToAppend)
    {
        prompt += "\n" + stringToAppend;
    }

    /// <summary>
    /// Append a list item to the prompt.
    /// </summary>
    /// <param name="stringToAppend"></param>
    private void AppendListItem(string stringToAppend)
    {
        prompt += "\n" + "- " + stringToAppend + ".";
    }

    /// <summary>
    /// Append a price value to the prompt.
    /// </summary>
    /// <param name="priceDenomination"></param>
    /// <param name="priceValue"></param>
    private void AppendPrice(string priceDenomination, float priceValue)
    {
        prompt += "\n" + priceDenomination + ": " + priceValue.ToString();
    }
    #endregion

    #region ITEM GENERATION
    /// <summary>
    /// Creates the prompt that will be given to the AI for generating an item.
    /// </summary>
    private void GenerateCreationPrompt()
    {
        // Create a class containing the necessary context messages for the prompt.
        // The context messages are saved in a JSON file, which will be serialized into an object where we will retrieve the strings from.
        string absoluteContextPath = Application.dataPath + contextPath;

        string contextJsonString = File.ReadAllText(absoluteContextPath);

        ItemGenerationContext generationContext = JsonUtility.FromJson<ItemGenerationContext>(contextJsonString);

        // Start off prompt with scenario context.
        // Scenario context serves to give the base instruction of creating an item and giving the overall theme behind item's creation.
        prompt = generationContext.Scenario;

        // Add inspiration context to prompt.
        // Inspiration context gives the general inspirations to reference from and sets up the entry of specific inspirations.
        AppendContext(generationContext.Inspiration);

        // Adds the list of specific inspirations to prompt.
        // User can set these in inspector to define references the AI should take inspiration from for creating the item.
        foreach (string inspiration in inspirations)
        {
            AppendListItem(inspiration);
        }

        // Add cliche context to prompt.
        // Tells the AI to avoid horror cliches and sets up the entry of specific cliches.
        AppendContext(generationContext.Cliche);

        // Adds the list of specific cliches to avoid.
        // User can set these in inspector.
        foreach (string cliche in cliches)
        {
            AppendListItem(cliche);
        }

        // Adds the operation order for creating the item. Also sets up entry for creation rules.
        AppendContext(generationContext.OperationOrder);

        // Adds the creation rules.
        foreach (string rule in generationContext.Rules)
        {
            AppendListItem(rule);
        }

        // Adds the context for generating prices.
        // Tells the AI to create a market price and client price for the item, and sets up entry for minimum and maximum price range.
        AppendContext(generationContext.Prices);

        // Adds the minimum and maximum price ranges.
        AppendPrice("Minimum Price", minimumPrice);
        AppendPrice("Maximum Price", maximumPrice);

        // Adds the final command instructing the AI to generate a JSON class based on the combined context.
        AppendContext(generationContext.Command);

        // Tells the AI the fields that should be included in the JSON file.
        foreach (string field in generationContext.ResultFields)
        {
            AppendListItem(field);
        }
    }

    /// <summary>
    /// Generates a random item by giving the AI the prompt to do so and using its JSON formatted response to create an Item class.
    /// </summary>
    /// <returns></returns>
    public async Task<Item> GenerateItem()
    {
        // Initialize API and generate the item creation prompt.
        // Both these things only need to be done once so they are contained in this if statement.
        if (openAIApi == null)
        {
            openAIApi = new OpenAIApi();
            GenerateCreationPrompt();
        }

        Item generatedItem = null;

        // While loop to ensure valid JSON string.
        bool jsonValid = false;
        do
        {
            // Get the JSON formatted item from the AI.
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
                    Content = prompt
                }
            }
            });

            // Convert the AI's JSON formatted response into an Item.
            string itemJson = completionResponse.Choices[0].Message.Content;

            // Only exit loop if we successfuly get a json class from the AI.
            try
            {
                generatedItem = JsonUtility.FromJson<Item>(itemJson);

                jsonValid = true;
            }

            catch(Exception exception)
            {
                Debug.Log("Retrying Json");

                await Task.Delay(500);
            }

        } while (!jsonValid);

        return generatedItem;
    }
    #endregion
}
