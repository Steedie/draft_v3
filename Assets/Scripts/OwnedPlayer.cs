using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class OwnedPlayer : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text moneySpentText;

    public void SetPlayerName(string playerName)
    {
        playerNameText.text = playerName;
    }

    public void SetMoneySpent(int moneySpent)
    {
        moneySpentText.text = moneySpent.ToString("C0", CultureInfo.CreateSpecificCulture("en-US"));
    }
}
