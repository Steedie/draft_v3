using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class BidManager : NetworkBehaviour
{
    public NetworkVariable<bool> m_BiddingActive = new NetworkVariable<bool>(false);
    public NetworkVariable<ulong> m_BiddingOnId = new NetworkVariable<ulong>();

    public NetworkVariable<int> m_CurrentBid = new NetworkVariable<int>(0);
    public NetworkVariable<ulong> m_BidLeaderId = new NetworkVariable<ulong>(0);

    [SerializeField] private GameObject biddingOverlay;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text tierText;
    [SerializeField] private TMP_Text highestBidderText;

    [SerializeField] private TMP_Text playerMoneyText;
    [SerializeField] private TMP_InputField bidInputField;

    [SerializeField] private GameObject captainControls;
    [SerializeField] private GameObject hostControls;

    [SerializeField] private Color canBidColor = Color.green;
    [SerializeField] private Color cantBidColor = Color.red;

    [SerializeField] private Button confirmWinnerButton;

    private bool isInitialized = false;

    private void Awake()
    {
        biddingOverlay.SetActive(false);
        SetControlsActive(false);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        m_BiddingActive.OnValueChanged += OnBiddingActiveChanged;

        m_CurrentBid.OnValueChanged += OnCurrentBidChanged;
        m_BidLeaderId.OnValueChanged += OnBidLeaderChanged;

        if (IsClient)
        {
            StartCoroutine(InitializeClient());
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        m_BiddingActive.OnValueChanged -= OnBiddingActiveChanged;
        m_CurrentBid.OnValueChanged -= OnCurrentBidChanged;
        m_BidLeaderId.OnValueChanged -= OnBidLeaderChanged;
    }

    private void OnCurrentBidChanged(int oldValue, int newValue)
    {
        if (!m_BiddingActive.Value)
            return;

        StartCoroutine(WaitForBidChangeDirty());
    }

    private void OnBidLeaderChanged(ulong oldValue, ulong newValue)
    {
        if (!m_BiddingActive.Value)
            return;

        StartCoroutine(WaitForBidChangeDirty());
    }

    IEnumerator WaitForBidChangeDirty()
    {
        yield return new WaitUntil(() => !m_BidLeaderId.IsDirty() && !m_CurrentBid.IsDirty());
        UpdateHighestBidderUi();
    }

    private IEnumerator InitializeClient()
    {
        yield return new WaitUntil(() => !m_BiddingActive.IsDirty() && !m_BiddingOnId.IsDirty() && !m_BidLeaderId.IsDirty() && !m_CurrentBid.IsDirty());
        if (m_BiddingActive.Value)
        {
            SetupBiddingCard(m_BiddingOnId.Value);
        }
        isInitialized = true;
    }

    private void OnBiddingActiveChanged(bool oldValue, bool newValue)
    {
        if (newValue)
            StartCoroutine(WaitForDirty());
        else
        {
            biddingOverlay.SetActive(false);
        }
    }

    IEnumerator WaitForDirty()
    {
        yield return new WaitUntil(() => !m_BiddingOnId.IsDirty());
        SetupBiddingCard(m_BiddingOnId.Value);
    }

    [Rpc(SendTo.Everyone)]
    public void StartBidOnDraftPlayerRpc(ulong draftPlayerId)
    {
        if (IsHost)
        {
            m_BiddingOnId.Value = draftPlayerId;
            m_BiddingActive.Value = true;
        }

        SetupBiddingCard(draftPlayerId);
    }

    private void SetupBiddingCard(ulong draftPlayerId)
    {
        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(draftPlayerId, out NetworkObject netDraftPlayerObject);
        NetDraftPlayer netDraftPlayer = netDraftPlayerObject.GetComponent<NetDraftPlayer>();

        playerNameText.text = netDraftPlayer.m_DraftPlayerName.Value.ToString();

        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netDraftPlayer.m_NetTierId.Value, out NetworkObject netTierObject);
        NetTier netTier = netTierObject.GetComponent<NetTier>();
        tierText.text = $"{netTier.m_TierName.Value} Tier";

        // !! If there's already an active bidder it should say who and what the highest bid is !!
        if (m_BidLeaderId.Value == 0)
        {
            int minBid = netTier.m_MinBid.Value;
            highestBidderText.text = $"minimum bid: {minBid.ToString("C0", CultureInfo.CreateSpecificCulture("en-US"))}";

            int myMoney = GameManager.NetPlayer.m_Money.Value;
            if (myMoney < minBid)
                playerMoneyText.color = cantBidColor;
            else
                playerMoneyText.color = canBidColor;
        }
        else
        {
            UpdateHighestBidderUi();
        }
        
        if (ColorUtility.TryParseHtmlString(netTier.m_TierColor.Value.ToString(), out Color targetColor))
        {
            tierText.color = targetColor;
        }

        if (!IsHost)
        {
            playerMoneyText.text = GameManager.NetPlayer.m_Money.Value.ToString("C0", CultureInfo.CreateSpecificCulture("en-US"));
        }

        biddingOverlay.SetActive(true);
        SetControlsActive(true);

        SoundManager.Instance.PlaySound("Ding");
    }

    private void SetControlsActive(bool active)
    {
        if (active)
        {
            if (IsHost)
                hostControls.SetActive(true);
            else
                captainControls.SetActive(true);
        }
        else
        {
            captainControls.SetActive(false);
            hostControls.SetActive(false);
        }
    }

    public void BidButton()
    {
        int bidAmount = 0;
        int.TryParse(bidInputField.text, out bidAmount);
        int playerMoney = GameManager.NetPlayer.m_Money.Value;

        if (playerMoney < bidAmount)
        {
            Debug.LogWarning($"Don't have enough money to make this bid. Money: {playerMoney}, bid amount: {bidAmount}");
            SoundManager.Instance.PlaySound("Error");
            return;
        }

        if (bidAmount <= m_CurrentBid.Value)
        {
            Debug.LogWarning($"Bid amount ({bidAmount}) must be larger than the current highest bid ({m_CurrentBid.Value})");
            SoundManager.Instance.PlaySound("Error");
            return;
        }

        if (m_BidLeaderId.Value == 0)
        {
            // Use tier min bid
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(m_BiddingOnId.Value, out NetworkObject netDraftPlayerObject);
            NetDraftPlayer netDraftPlayer = netDraftPlayerObject.GetComponent<NetDraftPlayer>();
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netDraftPlayer.m_NetTierId.Value, out NetworkObject netTierObject);
            NetTier netTier = netTierObject.GetComponent<NetTier>();
            if (bidAmount < netTier.m_MinBid.Value)
            {
                Debug.LogWarning($"Bid amount ({bidAmount}) must be at least the minimum bid for this tier");
                SoundManager.Instance.PlaySound("Error");
                return;
            }
        }

        MakeBidRpc(GameManager.NetPlayer.NetworkObjectId, bidAmount);
        bidInputField.text = "";
    }

    private Coroutine cooldownCoroutine;

    [Rpc(SendTo.Server)]
    private void MakeBidRpc(ulong bidderId, int bidAmount)
    {
        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(bidderId, out NetworkObject netPlayerObject);
        NetPlayer netPlayer = netPlayerObject.GetComponent<NetPlayer>();
        int playerMoney = netPlayer.m_Money.Value;

        // CHECK FOULPLAY
        if (playerMoney < bidAmount)
        {
            Debug.LogWarning($"Captain [{netPlayer.m_PlayerName.Value}] tried to make a bid of {bidAmount}, but they only have {playerMoney}");
            return;
        }
        if (bidAmount <= m_CurrentBid.Value)
        {
            Debug.LogWarning($"Captain [{netPlayer.m_PlayerName.Value}] tried to make a bid of {bidAmount} which is <= the current bid value ({m_CurrentBid.Value})");
            return;
        }

        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
        }
        cooldownCoroutine = StartCoroutine(ConfirmCooldownAfterNewBid());

        // WE GOOD, CONTINUE
        m_BidLeaderId.Value = bidderId;
        m_CurrentBid.Value = bidAmount;

        UpdateHighestBidderUiRpc(bidderId, bidAmount);
    }

    IEnumerator ConfirmCooldownAfterNewBid()
    {
        confirmWinnerButton.interactable = false;
        yield return new WaitForSeconds(.5f);
        confirmWinnerButton.interactable = true;
    }

    [Rpc(SendTo.Everyone)]
    private void UpdateHighestBidderUiRpc(ulong bidderId, int bidAmount)
    {
        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(bidderId, out NetworkObject netPlayerObject);
        NetPlayer netPlayer = netPlayerObject.GetComponent<NetPlayer>();
        string playerName = netPlayer.m_PlayerName.Value.ToString();
        string highestBid = bidAmount.ToString("C0", CultureInfo.CreateSpecificCulture("en-US"));
        highestBidderText.text = $"highest bidder: {playerName} {highestBid}";
        SoundManager.Instance.PlaySound("MakeBid");
    }

    private void UpdateHighestBidderUi()
    {
        if (m_BidLeaderId.Value == 0)
            return;

        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(m_BidLeaderId.Value, out NetworkObject netPlayerObject);
        NetPlayer netPlayer = netPlayerObject.GetComponent<NetPlayer>();
        string playerName = netPlayer.m_PlayerName.Value.ToString();
        string highestBid = m_CurrentBid.Value.ToString("C0", CultureInfo.CreateSpecificCulture("en-US"));
        highestBidderText.text = $"highest bidder: {playerName} {highestBid}";

        int myMoney = GameManager.NetPlayer.m_Money.Value;
        if (myMoney <= m_CurrentBid.Value)
            playerMoneyText.color = cantBidColor;
        else
            playerMoneyText.color = canBidColor;
    }

    public void EndAndConfirmWinnerButton()
    {
        if (IsHost)
        {
            if (m_BidLeaderId.Value == 0)
            {
                Debug.LogWarning("There are currently no bidders, can't declare a winner.");
                SoundManager.Instance.PlaySound("Error");
                return;
            }

            EndAndConfirmWinnerRpc();
        }
    }

    public void CancelButton()
    {
        if (IsHost)
        {
            m_BiddingOnId.Value = 0;
            m_CurrentBid.Value = 0;
            m_BidLeaderId.Value = 0;
            m_BiddingActive.Value = false;
        }
    }

    [Rpc(SendTo.Server)]
    private void EndAndConfirmWinnerRpc()
    {
        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(m_BidLeaderId.Value, out NetworkObject netPlayerObject);
        NetPlayer netPlayer = netPlayerObject.GetComponent<NetPlayer>();
        netPlayer.m_Money.Value -= m_CurrentBid.Value;

        NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(m_BiddingOnId.Value, out NetworkObject netDraftPlayerObject);
        NetDraftPlayer netDraftPlayer = netDraftPlayerObject.GetComponent<NetDraftPlayer>();
        netDraftPlayer.m_NetPlayerOwnerId.Value = netPlayer.NetworkObjectId;
        netDraftPlayer.m_FinalPrice.Value = m_CurrentBid.Value;
        netDraftPlayer.m_HasBeenSold.Value = true;

        m_BiddingOnId.Value = 0;
        m_CurrentBid.Value = 0;
        m_BidLeaderId.Value = 0;
        m_BiddingActive.Value = false;

        BidWinnerSoundRpc(netPlayer.NetworkObjectId);
    }

    [Rpc(SendTo.Everyone)]
    private void BidWinnerSoundRpc(ulong winnerId)
    {
        SoundManager.Instance.PlaySound("ConfirmWinner");
        if (winnerId == GameManager.NetPlayer.NetworkObjectId)
        {
            SoundManager.Instance.PlaySound("IsBidWinner");
        }
    }
}
