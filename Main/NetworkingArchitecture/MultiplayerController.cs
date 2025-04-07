using System.Collections.Generic;
using Godot;
using ProjectNeva.Main.Utils;
using Logger = ProjectNeva.Main.Utils.Logger.Logger;

namespace ProjectNeva.Main.NetworkingArchitecture;

public partial class MultiplayerController : Node
{
    public static MultiplayerController Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    #region Server Logic

    private readonly Dictionary<string, LobbyManager> _lobbyManagers = new();
    private readonly Dictionary<long, string> _playerIdToLobbyManagerId = new();

    public static bool LobbyExists(string lobbyId) => Instance._lobbyManagers.ContainsKey(lobbyId);
    public static void CreateLobby(
            string lobbyId,
            long creatorId,
            int playTime,
            int maxPlayers
        )
    {
        var lobby = new Lobby(lobbyId, creatorId, playTime, maxPlayers);
        Instance._lobbyManagers.Add(lobbyId, new LobbyManager(lobby));
    }

    #region Broadcasting
    
    public void Server_BroadcastLobby(Lobby lobby, StringName methodName, params Variant[] args)
    {
        foreach (var playerId in lobby.Players.Keys)
        {
            RpcId(playerId, methodName, args);
        }
    }

    public void Server_BroadcastLobbyNewPlayer(long joinerId, Lobby lobby)
    {
        foreach (var playerId in lobby.Players.Keys)
        {
            RpcId(joinerId, MethodName.Client_ReceiveNewPlayer, playerId, lobby.Players[joinerId]);
            RpcId(playerId, MethodName.Client_ReceiveNewPlayer, joinerId, lobby.Players[playerId]);
        }
    }
    
    #endregion
    
    public void Server_OnPeerConnected(long newPeerId)
    {
        if (!Networking.Instance.PeerAuthData.Remove(newPeerId, out var peerAuthData))
        {
            GD.PrintErr($"Peer {newPeerId} AuthData is null");
            return;
        }

        if (!_lobbyManagers.TryGetValue(peerAuthData.LobbyId.ToString(), out var lobbyManager))
        {
            Networking.Instance.Multiplayer.DisconnectPeer((int) newPeerId);
        }

        if (lobbyManager == null) return;
        
        var lobby = lobbyManager.Lobby;
        lobby.OnPlayerConnected(newPeerId, peerAuthData);
        _playerIdToLobbyManagerId.Add(newPeerId, lobby.LobbyId);
    }

    private Lobby Server_GetLobbyByPlayerId(long playerId)
    {
        return _playerIdToLobbyManagerId.TryGetValue(playerId, out var lobbyId)
            ? _lobbyManagers[lobbyId].Lobby : null;
    }

    public void Server_SendLobbySettings(long playerId, Lobby lobby)
    {
        RpcId(playerId, MethodName.Client_ReceiveLobbySettings,
            lobby.LobbySize, 
            lobby.Topic, 
            lobby.PlayTime);
    }

    public void Server_OnPeerDisconnected(long peerId)
    {
        if (!_playerIdToLobbyManagerId.TryGetValue(peerId, out var lobbyId)) return;
        var lobby = _lobbyManagers[lobbyId].Lobby;
        lobby.OnPlayerDisconnected(peerId);

        if (lobby.Players.Count == 0)
        {
            _lobbyManagers.Remove(lobbyId);
            Logger.LogNetwork($"Lobby {lobbyId} closed");
        }
        _playerIdToLobbyManagerId.Remove(peerId);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Server_HandlePlayerLoadNewScene()
    {
        var playerId = Multiplayer.GetRemoteSenderId();
        Server_GetLobbyByPlayerId(playerId)?.OnPlayerLoadedNewScene(playerId);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Server_ReceiveFinalImage(byte[] imageData)
    {
        var playerId = Multiplayer.GetRemoteSenderId();
        Server_GetLobbyByPlayerId(playerId)?.OnPlayerSentFinalImage(playerId, imageData);

        Logger.LogNetwork($"Received final image from {playerId} with {imageData.Length} bytes");
    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Server_HandlePlayerDrawingStateChange(bool drawingOn)
    {
        var playerId = Multiplayer.GetRemoteSenderId();
        Server_GetLobbyByPlayerId(playerId)?.OnPlayerDrawingStateChanged(playerId, drawingOn);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Server_HandleNewScore(long toPlayerId, int score)
    {
        var playerId = Multiplayer.GetRemoteSenderId();
        Server_GetLobbyByPlayerId(playerId)?.OnNewScoreReceived(playerId, toPlayerId, score);
    }

    #endregion

    #region Client Logic

    public readonly Godot.Collections.Dictionary<long, Player> Client_Players = new();
    public readonly Godot.Collections.Dictionary<long, Image> Client_FinalImages = new();
    public Godot.Collections.Dictionary<long, int> Client_Scores;

    public int Client_MaxPlayers;
    public int Client_MaxRounds;
    public float Client_DrawingTimeSec;
    public string Client_Topic;

    [Signal] public delegate void PlayerJoinedLobbyEventHandler(long playerId);
    
    [Signal] public delegate void PlayerLeftLobbyEventHandler(long playerId);

    [Signal] public delegate void DrawingGameStartedEventHandler();

    [Signal] public delegate void FinalImageRequestedEventHandler();
    
    [Signal] public delegate void ImageToRateReceivedEventHandler(long playerId, Image image);

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Client_ReceiveLobbySettings(
        int lobbySize, 
        string topic,
        float drawingTimeSec)
    {
        Client_MaxPlayers = lobbySize;
        Client_Topic = topic;
        Client_DrawingTimeSec = drawingTimeSec;
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Client_ReceiveNewPlayer(long playerId, string name)
    {
        Client_Players[playerId] = new Player(playerId, name);
        EmitSignal(SignalName.PlayerJoinedLobby, playerId);
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Client_ClearLeavingPlayer(long playerId)
    {
        Client_Players.Remove(playerId);
        EmitSignal(SignalName.PlayerLeftLobby, playerId);
    }

    public void Client_NotifyNewSceneReady()
    {
        RpcId(Networking.GameServerPeerId, MethodName.Server_HandlePlayerLoadNewScene);
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Client_ShootDrawingScene()
    {
        EmitSignal(SignalName.DrawingGameStarted);
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Client_CollectFinalImage()
    {
        EmitSignal(SignalName.FinalImageRequested);
    }

    private void Client_SendFinalImage(byte[] imageData)
    {
        imageData = ImageHelper.Compress(imageData);
        RpcId(Networking.GameServerPeerId, MethodName.Server_ReceiveFinalImage, imageData);
        Logger.LogNetwork($"Sent final image with {imageData.Length} bytes");
    }

    private void Client_NotifyDrawingStateChanged(bool drawingOn)
    {
        RpcId(Networking.GameServerPeerId, MethodName.Server_HandlePlayerDrawingStateChange, drawingOn);
    }
    
    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Client_ReceiveFinalImages(Godot.Collections.Dictionary<long, byte[]> images)
    {
        foreach (var playerId in images.Keys)
        {
            Client_FinalImages[playerId] = ImageHelper.CreateImageFromCompressed(images[playerId]);
        }
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Client_ShootRatingNextPlayer(long playerId)
    {
        var image = Client_FinalImages[playerId];
        EmitSignal(SignalName.ImageToRateReceived, playerId, image);
    }
    
    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Client_ReceiveScores(Godot.Collections.Dictionary<long, int> scores)
    {
        Client_Scores = scores;
    }

    private void Client_SendScore(long playerId, int score)
    {
        RpcId(Networking.GameServerPeerId, MethodName.Server_HandleNewScore, playerId, score);
    }

    #region Loading Scenes

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Client_LoadDrawingScene()
    {
        GetTree().ChangeSceneToFile("res://Main/DrawingGame/Drawing/drawing_scene.tscn");
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Client_LoadRatingScene()
    {
        GetTree().ChangeSceneToFile("res://Main/DrawingGame/Rating/RatingScene.tscn");
    }
    
    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Client_LoadResultsScene()
    {
        GetTree().ChangeSceneToFile("res://Main/DrawingGame/Results/ResultsScene.tscn");
    }


    #endregion

    #endregion
    
}
