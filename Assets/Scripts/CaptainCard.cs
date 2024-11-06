using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class CaptainCard : MonoBehaviour
{
    ulong captainNetPlayerId;

    [SerializeField] private TMP_Text captainName;
    [SerializeField] private TMP_Text captainMoney;

    [SerializeField] private OwnedPlayer ownedPlayerPrefab;
    [SerializeField] private Transform ownedPlayersParent;

    [SerializeField] private GameObject editMoneyButton;
    [SerializeField] private GameObject editMoney;
    [SerializeField] private TMP_InputField moneyInputField;

    private void Update()
    {
        if (!NetworkManager.Singleton.IsHost)
            return;

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            editMoneyButton.SetActive(true);
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            editMoneyButton.SetActive(false);
        }
    }

    public void SetCaptainNetPlayerId(ulong id)
    {
        captainNetPlayerId = id;
    }

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

    public void EditMoneyButton()
    {
        editMoney.SetActive(true);
        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(captainNetPlayerId, out NetworkObject netPlayerObject);
        int money = netPlayerObject.GetComponent<NetPlayer>().m_Money.Value;
        moneyInputField.text = money.ToString();
    }

    public void SetMoneyButton()
    {
        if (!NetworkManager.Singleton.IsHost)
            return;

        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(captainNetPlayerId, out NetworkObject netPlayerObject);
        int.TryParse(moneyInputField.text, out int newMoney);
        netPlayerObject.GetComponent<NetPlayer>().m_Money.Value = newMoney;

        editMoney.SetActive(false);
        SoundManager.Instance.PlaySound("Edit");
    }
}
