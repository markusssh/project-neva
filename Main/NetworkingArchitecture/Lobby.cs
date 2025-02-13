using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectNeva.Main.NetworkingArchitecture;

public class Lobby
{
    private const int LobbyDefaultSize = 2;
    private const int LobbyDefaultRounds = 1;
    private const int LobbyDefaultRoundLength = 30;
    
    public enum LobbyState
    {
        WaitingPlayers,
        Playing
    }

    public Lobby(string lobbyId)
    {
        LobbyId = lobbyId;
        TopicIdSet = MultiplayerController.Instance.TopicIds;
    }

    public string LobbyId { get; set; }
    public Dictionary<long, Player> Players { get; set; } = new();
    public LobbyState State = LobbyState.WaitingPlayers;

    public int LobbySize { get; set; } = LobbyDefaultSize;
    public int MaxRounds { get; set; } = LobbyDefaultRounds;
    public int RoundLength { get; set; } = LobbyDefaultRoundLength;
   
    public long DrawerId = -1;
    private HashSet<int> TopicIdSet { get; set; }
    public int CurrentTopicId { get; set; } = -1;

    public void SetRandomTopicId()
    {
        if (TopicIdSet.Count == 0) 
            throw new InvalidOperationException("No topics available to choose from.");

        Random random = new();
        int randomIndex = random.Next(0, TopicIdSet.Count);
        CurrentTopicId = TopicIdSet.ElementAt(randomIndex);
        TopicIdSet.Remove(CurrentTopicId);
    }

    public void SetRandomDrawerId()
    {
        if (Players.Count == 0) throw new InvalidOperationException("No peer to choose random drawer.");
        Random random = new();
        var randomIndex = random.Next(0, Players.Count);
        var chosen = Players.ElementAt(randomIndex).Key;
        DrawerId = chosen;
    }
}