using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class CaptainCard : MonoBehaviour
{
    [SerializeField] private TMP_Text captainName;
    [SerializeField] private TMP_Text captainMoney;

    [SerializeField] private OwnedPlayer ownedPlayerPrefab;
    [SerializeField] private Transform ownedPlayersParent;

    public void UpdateCaptainName(string value)
    {
        captainName.text = value;
    }

    public void UpdateCaptainMoney(int value)
    {
        captainMoney.text = value.ToString("C0", CultureInfo.CreateSpecificCulture("en-US"));
    }

    public OwnedPlayer AddOwnedPlayer(string playerName, int moneySpent)
    {
        OwnedPlayer ownedPlayer = Instantiate(ownedPlayerPrefab, ownedPlayersParent);
        ownedPlayer.SetPlayerName(playerName);
        ownedPlayer.SetMoneySpent(moneySpent);
        return ownedPlayer;
    }

    public bool HasNoDraftPlayers()
    {
        return ownedPlayersParent.childCount == 0;
    }
}
