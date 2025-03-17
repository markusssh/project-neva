using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using ProjectNeva.Main.NetworkingArchitecture.GamePhases;
using ProjectNeva.Main.NetworkingArchitecture.LobbyLogic;
using ProjectNeva.Main.Utils;
using Logger = ProjectNeva.Main.Utils.Logger.Logger;

namespace ProjectNeva.Main.NetworkingArchitecture;

public partial class MultiplayerController : Node
{
    public static MultiplayerController Instance { get; private set; }
    public float DrawingRoundTimeSec = 3.0f;

    public float TimeToRateOneSec = 20.0f;

    public override void _Ready()
    {
        Instance = this;
    }


    #region Server Logic

    private readonly Dictionary<string, LobbyManager> _lobbyManagers = new();
    private readonly Dictionary<long, string> _playerIdToLobbyManagerId = new();
    
    #region Broadcasting
    public void Server_Broadcast(Lobby lobby, string methodName, params Variant[] args)
    {
        foreach (var playerId in lobby.Players.Keys)
        {
            RpcId(playerId, methodName, args);
        }
    }

    public void Server_BroadcastNewPlayer(long joinerId, Lobby lobby)
    {
        foreach (var playerId in lobby.Players.Keys)
        {
            RpcId(joinerId, MethodName.Client_GetNewPlayer, playerId, lobby.Players[joinerId]);
            RpcId(playerId, MethodName.Client_GetNewPlayer, joinerId, lobby.Players[playerId]);
        }
    }
    #endregion

    public async Task Server_OnPeerConnected(long newPeerId)
    {
        if (!Networking.Instance.PeerAuthData.Remove(newPeerId, out var peerAuthData))
        {
            GD.PrintErr($"Peer {newPeerId} AuthData is null");
            return;
        }

        if (!_lobbyManagers.TryGetValue(peerAuthData.LobbyId, out var lobbyManager))
        {
            lobbyManager = new LobbyManager(new Lobby(peerAuthData.LobbyId));
            _lobbyManagers.Add(lobbyManager.Lobby.LobbyId, lobbyManager);
            lobbyManager.TransitionTo(LobbyState.WaitingPlayers);
        }

        var lobby = lobbyManager.Lobby;
        lobby.OnPlayerConnected(newPeerId, peerAuthData);
        _playerIdToLobbyManagerId.Add(newPeerId, lobby.LobbyId);
    }

    public void Server_SendLobbySettings(long playerId, Lobby lobby)
    {
        RpcId(playerId, MethodName.Client_GetLobbySettings, lobby.LobbySize, lobby.Topic);
    }

    public void Server_OnPeerDisconnected(long peerId)
    {
        if (!_playerIdToLobbyManagerId.TryGetValue(peerId, out var lobbyId)) return;
        var lobby = _lobbyManagers[lobbyId].Lobby;
        lobby.OnPlayerDisconnected(peerId);

        if (lobby.Players.Count == 0)
        {
            _lobbyManagers.Remove(lobbyId);
            Logger.LogMessage($"Lobby {lobbyId} closed");
        }
        _playerIdToLobbyManagerId.Remove(peerId);
        
        Logger.LogMessage($"Peer {peerId} disconnected from this server");
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Server_HandlePlayerLoadDrawingScene()
    {
        var playerId = Multiplayer.GetRemoteSenderId();
        if (!_playerIdToLobbyManagerId.TryGetValue(playerId, out var lobbyId)) return;
        var lobby = _lobbyManagers[lobbyId].Lobby;
        lobby.OnPlayerLoadedDrawingScene(playerId);
    }

    private void CreateLobbyDrawingTimer(string lobbyId)
    {
        Timer timer = new();
        AddChild(timer);
        timer.OneShot = true;
        timer.Start(DrawingRoundTimeSec);
        timer.Timeout += OnTimerTimeout;

        void OnTimerTimeout()
        {
            timer.QueueFree();
            OnDrawingRoundEnd(lobbyId);
        }
    }

    private void OnDrawingRoundEnd(string lobbyId)
    {
        if (!_lobbyRepo.TryGetValue(lobbyId, out var lobby)) return;
        foreach (var player in lobby.Players.Values)
        {
            RpcId(player.PlayerId, MethodName.HandleFinalImageRequestOnClient);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void HandlePlayerLoadedGameSceneOnServer()
    {
        var playerId = Multiplayer.GetRemoteSenderId();
        if (!_lobbyRepo.TryGetValue(_playerIdToLobbyId[playerId], out var lobby)) return;
        lobby.Players[playerId].State = Player.PlayerState.Playing;
        Logger.LogNetwork($"Player {playerId} from lobby {lobby.LobbyId} has loaded game scene");

        var everyoneIsLoaded = true;
        foreach (var player in lobby.Players.Values)
        {
            if (player.State != Player.PlayerState.Playing) everyoneIsLoaded = false;
        }

        if (everyoneIsLoaded)
        {
            EmitSignal(SignalName.LobbyIsLoaded, lobby.LobbyId);
            Logger.LogNetwork($"Everyone on lobby {lobby.LobbyId} has been loaded");
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void HandleFinishedDrawingOnServer()
    {
        var playerId = Multiplayer.GetRemoteSenderId();
        if (!_lobbyRepo.TryGetValue(_playerIdToLobbyId[playerId], out var lobby)) return;
        lobby.PlayersFinishedDrawingCounter++;
        if (lobby.PlayersFinishedDrawingCounter == lobby.Players.Count)
        {
            Logger.LogNetwork($"Everyone on lobby {lobby.LobbyId} has finished drawing");
            OnDrawingRoundEnd(lobby.LobbyId);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void HandleResumedDrawingOnServer()
    {
        var playerId = Multiplayer.GetRemoteSenderId();
        if (!_lobbyRepo.TryGetValue(_playerIdToLobbyId[playerId], out var lobby)) return;
        lobby.PlayersFinishedDrawingCounter--;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void HandleFinalImageResponseOnServer(byte[] imageData)
    {
        var playerId = Multiplayer.GetRemoteSenderId();
        if (!_lobbyRepo.TryGetValue(_playerIdToLobbyId[playerId], out var lobby)) return;
        lobby.ReceivedFinalDrawingCounter++;
        lobby.Players[playerId].FinalImageData = imageData;
        if (lobby.ReceivedFinalDrawingCounter == lobby.Players.Count)
        {
            Logger.LogNetwork($"Received every final drawing on lobby {lobby.LobbyId}");
        }

        Godot.Collections.Dictionary<long, byte[]> playersImageData = new();

        foreach (var player in lobby.Players.Values)
        {
            playersImageData[player.PlayerId] = player.FinalImageData;
        }

        foreach (var player in lobby.Players.Values)
        {
            RpcId(player.PlayerId, MethodName.SyncPlayersImagesOnClient, playersImageData);
        }

        foreach (var player in lobby.Players.Values)
        {
            RpcId(player.PlayerId, MethodName.StartRatingRound);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void HandleNewScoreOnServer(int playerToRateId, int score)
    {
        var playerId = Multiplayer.GetRemoteSenderId();
        if (!_lobbyRepo.TryGetValue(_playerIdToLobbyId[playerId], out var lobby)) return;
        lobby.Players[playerToRateId].AddScore(score);
    }

    #endregion

    #region Client Logic

    public readonly Godot.Collections.Dictionary<long, Player> CurrentLobbyPlayers = new();

    public int MaxPlayers;
    public int MaxRounds;
    public int RoundLength;
    public string Topic;

    [Signal] public delegate void PlayerJoinedLobbyEventHandler(long playerId);
    [Signal] public delegate void PlayerLeftLobbyEventHandler(long playerId);
    [Signal] public delegate void PlayerBecameDrawerEventHandler(long playerId);
    [Signal] public delegate void ImageBytesReceivedEventHandler(byte[] bytes);
    [Signal] public delegate void TopicIdReceivedEventHandler(int topicId);
    [Signal] public delegate void DrawingGameStartedEventHandler();
    [Signal] public delegate void FinalImageRequestedEventHandler();

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Client_GetLobbySettings(int lobbySize, string topic)
    {
        MaxPlayers = lobbySize;
        Topic = topic;
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Client_GetNewPlayer(long playerId, string name)
    {
        CurrentLobbyPlayers[playerId] = new Player(playerId, name);
        EmitSignal(SignalName.PlayerJoinedLobby, playerId);
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Client_ClearLeavingPlayer(long playerId)
    {
        CurrentLobbyPlayers.Remove(playerId);
        EmitSignal(SignalName.PlayerLeftLobby, playerId);
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Client_LoadDrawingScene()
    {
        GetTree().ChangeSceneToFile("res://Main/DrawingGame/Drawing/drawing_scene.tscn");
    }

    public void Client_NotifyDrawingSceneReady()
    {
        RpcId(Networking.ServerPeerId, MethodName.Server_HandlePlayerLoadDrawingScene);
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Client_ShootDrawingScene()
    {
        EmitSignal(SignalName.DrawingGameStarted);
    }

    private void HandleDrawingStateChangeOnClient(bool drawing)
    {
        RpcId(Networking.ServerPeerId,
            drawing ? MethodName.HandleResumedDrawingOnServer : MethodName.HandleFinishedDrawingOnServer);
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void HandleFinalImageRequestOnClient()
    {
        EmitSignal(SignalName.FinalImageRequested);
    }

    private void HandleFinalImageResponseOnClient(Image image)
    {
        RpcId(Networking.ServerPeerId,
            MethodName.HandleFinalImageResponseOnServer, ImageHelper.Compress(image.GetData())
        );
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncPlayersImagesOnClient(Godot.Collections.Dictionary<long, byte[]> playersImages)
    {
        foreach (var playerId in playersImages.Keys)
        {
            CurrentLobbyPlayers[playerId].FinalImageData = playersImages[playerId];
        }
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SendScoreFromClient(int player, int score)
    {
        if (score > 0) RpcId(Networking.ServerPeerId, MethodName.HandleNewScoreOnServer, player, score);
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void StartRatingRound()
    {
        GetTree().ChangeSceneToFile("res://Main/DrawingGame/Rating/RatingScene.tscn");
    }

    #endregion
    
}
