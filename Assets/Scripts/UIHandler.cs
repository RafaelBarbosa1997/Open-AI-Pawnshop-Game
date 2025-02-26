using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class handles filling out the UI elements.
/// </summary>
public class UIHandler : MonoBehaviour
{
    [SerializeField]
    private TMP_Text itemName, itemInformation, marketValue, clientOffer, dealValue, chatBox, earnedGains, maxClients, clientCount, neededGains;
    [SerializeField]
    private TMP_Text endStatus, endDeals, endEarnings;
    [SerializeField]
    private Color underValueColor, overValueColor;
    [SerializeField]
    private ScrollRect scrollRect;

    #region HELPER METHODS
    /// <summary>
    /// Scrolls the chat to the bottom.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForSeconds(0.1f);

        scrollRect.normalizedPosition = new Vector2(0, 0);
    }

    /// <summary>
    /// Takes in a string and makes the numbers in it bold and gives them an accent color.
    /// Used to highlight prices for analyzing the negotiation more easily.
    /// </summary>
    /// <param name="clientMessage"></param>
    /// <returns></returns>
    private string ColorPrice(string clientMessage)
    {
        string pattern = @"\d+";

        string result = Regex.Replace(clientMessage, pattern, match => $"<color=#fcb103><b>{match.Value}</b></color>");

        return result;
    }
    #endregion

    #region ITEM INFO
    /// <summary>
    /// Sets the UI elements for item information.
    /// </summary>
    /// <param name="item"></param>
    public void SetItemInfo(Item item)
    {
        // Item name.
        itemName.text = item.Name;

        // Item visual and effect descriptions.
        itemInformation.text = item.Description;
        itemInformation.text += "\n" + item.Effect;

        // Item's market value.
        marketValue.text = item.MarketValue.ToString();

        // Client's offer for the item.
        clientOffer.text = item.ClientOffer.ToString();
    }

    /// <summary>
    /// Sets the UI element for the value player will get out of the deal.
    /// </summary>
    /// <param name="value"></param>
    public void SetDealValue(float value)
    {
        // If player is making no money from the deal set the text to red.
        if(value <= 0)
        {
            dealValue.color = underValueColor;
        }

        // If player is making money from the deal set the text to green.
        else
        {
            dealValue.color = overValueColor;
        }

        // Set text to display value.
        dealValue.text = value.ToString();
    }

    /// <summary>
    /// Change the client offer text.
    /// </summary>
    /// <param name="value"></param>
    public void SetNewClientOffer(float value)
    {
        clientOffer.text = value.ToString();
    }

    public void ClearItemInfo()
    {
        itemName.text = "";

        itemInformation.text = "";

        marketValue.text = "";

        clientOffer.text = "";
    }
    #endregion

    #region CHAT
    /// <summary>
    /// Appends a message to the chat box.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="message"></param>
    /// <param name="firstMessage"></param>
    public void AppendMessageToChat(string sender, string message, bool firstMessage = false)
    {
        // If its not the first message create a new line before appending.
        if (!firstMessage)
        {
            chatBox.text += "\n";
        }

        // Highlight the numbers in the message.
        message = ColorPrice(message);

        // Append the new message.
        chatBox.text += "<color=#FF7423>" + sender + ": " + "</color>" + message;

        // Scroll chat to the bottom to display latest message.
        // Needs to be in a coroutine so scroll rect has time to update its value.
        StartCoroutine(ScrollToBottom());
    }

    /// <summary>
    /// Clear the chat box.
    /// </summary>
    public void ClearChat()
    {
        chatBox.text = "";

        StartCoroutine(ScrollToBottom());
    }
    #endregion

    #region GAME VALUES
    public void SetNeededGains(float value)
    {
        neededGains.text = value.ToString();
    }

    public void SetMaxClients(int value)
    {
        maxClients.text = value.ToString();
    }

    public void SetClientCount(int value)
    {
        clientCount.text = value.ToString();
    }

    public void SetEarnedGains(float value, float needed)
    {
        earnedGains.text = value.ToString();

        if(value < needed)
        {
            earnedGains.color = underValueColor;
        }

        else
        {
            earnedGains.color = overValueColor;
        }
    }
    #endregion

    public void SetEndGameScreen(string status, int madeDeals, int clientMax, float earnedGains, float neededGains)
    {
        endStatus.text = status;

        endDeals.text = madeDeals.ToString() + " / " + clientMax.ToString();

        endEarnings.text = earnedGains.ToString() + " / " + neededGains.ToString();

        if(earnedGains < neededGains)
        {
            endEarnings.color = underValueColor;
        }

        else
        {
            endEarnings.color = overValueColor;
        }
    }
}
