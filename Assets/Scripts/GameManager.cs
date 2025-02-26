using System.Collections;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField chatInput;
    [SerializeField]
    private GameObject newClientButton, endGameObject;

    [SerializeField]
    private int maxClients;
    [SerializeField]
    private float neededGains;

    [SerializeField]
    private ItemGeneration itemGeneration;
    [SerializeField]
    private ClientSetup clientSetup;
    [SerializeField]
    private UIHandler uiHandler;

    private int clientCount, madeDeals;
    private float currentMarketValue, currentClientOffer, currentDealValue, currentGains;
    private bool inputLocked, dealEnded;

    private Item currentItem;
    private Client currentClient;

    private async void Start()
    {
        inputLocked = true;

        uiHandler.SetMaxClients(maxClients);
        uiHandler.SetNeededGains(neededGains);

        await OnNewClient();
    }

    private async void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            await SendPlayerMessage();
        }

        if (dealEnded)
        {
            dealEnded = false;

            await OnNewClient();
        }
    }

    /// <summary>
    /// Adds to the current gains by incrementing it by the current deal value.
    /// </summary>
    private void AddGains()
    {
        currentGains += currentDealValue;

        uiHandler.SetEarnedGains(currentGains, neededGains);
    }

    /// <summary>
    /// Gets the current deal's value by calculating the difference between item's market value and the client's offer for it.
    /// </summary>
    /// <returns></returns>
    private float DefineDealValue()
    {
        float dealValue = currentMarketValue - currentClientOffer;

        return dealValue;
    }

    /// <summary>
    /// Displays the button to send in a new client, called when current client leaves.
    /// </summary>
    private void DisplayNewClientButton() 
    {
        newClientButton.SetActive(true);
    }

    /// <summary>
    /// Enables the end game screen and sets the text to inform whether the game ended in a victory or a loss.
    /// </summary>
    private void EndGame()
    {
        endGameObject.SetActive(true);

        // Status text indicates to the player whether they won or lost.
        // Set it according to whether the player reached the necessary amount of gold.
        string status;

        if(currentGains < neededGains)
        {
            status = "You failed. It's time to close shop.";
        }

        else
        {
            status = "Looks like you'll live to deal another day.";
        }

        uiHandler.SetEndGameScreen(status, madeDeals, maxClients, currentGains, neededGains);
    }

    #region NEW CLIENT
    /// <summary>
    /// Uses item generation class to generate a new item and returns it.
    /// </summary>
    /// <returns></returns>
    private async Task<Item> GetNewItem()
    {
        Item newItem;

        // Generate item.
        // While loop exists incase AI responds with an improperly formatted JSON class causing item generation to fail.
        do
        {
            newItem = await itemGeneration.GenerateItem();

        } while (newItem == null);

        return newItem;
    }

    /// <summary>
    /// Procedure for when a new client arrives at the shop.
    /// </summary>
    /// <returns></returns>
    private async Task OnNewClient()
    {
        // Get a new item and set it as current item.
        currentItem = await GetNewItem();

        // Set the market value and client offer from item's information.
        currentMarketValue = currentItem.MarketValue;
        currentClientOffer = currentItem.ClientOffer;

        // Get the current deal's value.
        currentDealValue = DefineDealValue();

        // Initialize new client and get its first dialogue.
        currentClient = new Client(clientSetup, this);
        string initialDialogue = await currentClient.CreateInitialDialogue(currentItem);

        // Use UIHandler class to fill out UI elements.
        uiHandler.ClearChat();

        uiHandler.SetItemInfo(currentItem);

        uiHandler.SetDealValue(currentDealValue);

        uiHandler.AppendMessageToChat("Client", initialDialogue, true);

        inputLocked = false;
    }
    #endregion

    /// <summary>
    /// Handles the behaviour for the player sending a new message to talk with client.
    /// </summary>
    /// <returns></returns>
    private async Task SendPlayerMessage()
    {
        // Can't send message during specific times.
        if (inputLocked || chatInput.text == "")
        {
            return;
        }

        // Once sent input becomes locked.
        inputLocked = true;

        // Get string from player's message and clear chatbox.
        string playerMessage = chatInput.text;
        chatInput.text = "";

        // Display player's message in chat history.
        uiHandler.AppendMessageToChat("You", playerMessage);

        // Get the response from the client.
        string clientResponse = await currentClient.RespondToPlayer(playerMessage);

        // Display client's response in chat history.
        uiHandler.AppendMessageToChat("Client", clientResponse);

        // Unlock input.
        inputLocked = false;
    }

    public void ChangeCurrentClientOffer(float value)
    {
        currentClientOffer = value;

        currentDealValue = DefineDealValue();

        uiHandler.SetNewClientOffer(currentClientOffer);
        uiHandler.SetDealValue(currentDealValue);
    }

    public void CloseDeal()
    {
        inputLocked = true;

        AddGains();

        madeDeals++;

        clientCount++;

        uiHandler.SetClientCount(clientCount);

        if(clientCount < maxClients)
        {
            DisplayNewClientButton();
        }

        else
        {
            EndGame();
        }
    }

    public void CancelDeal()
    {
        inputLocked = true;

        clientCount++;

        uiHandler.SetClientCount(clientCount);

        if (clientCount < maxClients)
        {
            DisplayNewClientButton();
        }

        else
        {
            EndGame();
        }
    }

    public void SendInNewClient()
    {
        uiHandler.ClearChat();
        uiHandler.ClearItemInfo();

        dealEnded = true;

        newClientButton.SetActive(false);
    }

    public void RestartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        SceneManager.LoadScene(currentScene.name);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
