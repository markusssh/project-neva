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

        PlayerLoadedGameScene += OnPlayerLoadedGameScene;
    }


    #region Server Logic

    private readonly Dictionary<string, LobbyManager> _lobbyManagers = new();
    private readonly Dictionary<long, string> _playerIdToLobbyManagerId = new();

    [Signal]
    public delegate void LobbyIsLoadedEventHandler(string lobbyId);

    public async Task Server_OnPeerConnected(long newPeerId)
    {
        if (!Networking.Instance.PeerAuthData.TryGetValue(newPeerId, out var peerAuthData))
        {
            GD.PrintErr($"Peer {newPeerId} AuthData is null");
            return;
        }

        LobbyManager lobbyManager;
        if (_lobbyManagers.TryGetValue(peerAuthData.LobbyId, out lobbyManager)) {}
        else
        {
            lobbyManager = new LobbyManager(new Lobby(peerAuthData.LobbyId));
            lobbyManager.TransitionTo(LobbyState.WaitingPlayers);
        }
        
        lobbyManager.HandlePlayerConnect(peerAuthData);
        
        
    }

    private void DisconnectPlayerFromLobby(long playerId, string lobbyId)
    {
        if (!_lobbyRepo.TryGetValue(lobbyId, out var lobby)) return;
        lobby.Players.Remove(playerId);
        if (lobby.Players.Count == 0)
        {
            _lobbyRepo.Remove(lobbyId);
            Logger.LogNetwork($"Lobby {lobbyId} removed");
        }
    }

    private Lobby GetLobbyElseCreate(string lobbyId)
    {
        if (_lobbyRepo.TryGetValue(lobbyId, out var lobby))
        {
            return lobby;
        }

        var createLobby = new Lobby(lobbyId);
        _lobbyRepo[lobbyId] = createLobby;
        return createLobby;
    }

    public void OnPeerDisconnected(long disconnectingPeerId)
    {
        if (!_playerIdToLobbyId.TryGetValue(disconnectingPeerId, out var lobbyId)) return;
        Networking.Instance.PeerAuthData.Remove((int)disconnectingPeerId);
        _playerIdToLobbyId.Remove(disconnectingPeerId);

        foreach (var playerId in _lobbyRepo[lobbyId].Players.Keys)
        {
            if (disconnectingPeerId != playerId)
            {
                RpcId(disconnectingPeerId, MethodName.HandlePlayerDisconnectedOnClient, playerId);
            }

            RpcId(playerId, MethodName.HandlePlayerDisconnectedOnClient, disconnectingPeerId);
        }

        DisconnectPlayerFromLobby(disconnectingPeerId, lobbyId);
        Logger.LogMessage($"Disconnected on peer {disconnectingPeerId}");
    }

    private async Task OnPlaying(Lobby lobby)
    {
        await OnLoading(lobby);
        foreach (var player in lobby.Players.Values)
        {
            RpcId(player.PlayerId, MethodName.HandleGameReadyOnClient);
        }
    }

    private async Task OnLoading(Lobby lobby)
    {
        var tcs = new TaskCompletionSource();

        void OnLobbyLoaded(string lobbyId)
        {
            if (lobbyId == lobby.LobbyId)
            {
                tcs.TrySetResult();
                LobbyIsLoaded -= OnLobbyLoaded;
            }
        }

        LobbyIsLoaded += OnLobbyLoaded;

        foreach (var player in lobby.Players.Values)
        {
            RpcId(player.PlayerId, MethodName.StartDrawingRound);
        }

        await tcs.Task;

        CreateLobbyDrawingTimer(lobby.LobbyId);
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

    [Signal]
    public delegate void PlayerJoinedLobbyEventHandler(long playerId);

    [Signal]
    public delegate void PlayerLeftLobbyEventHandler(long playerId);

    [Signal]
    public delegate void PlayerBecameDrawerEventHandler(long playerId);

    [Signal]
    public delegate void ImageBytesReceivedEventHandler(byte[] bytes);

    [Signal]
    public delegate void TopicIdReceivedEventHandler(int topicId);

    [Signal]
    public delegate void PlayerLoadedGameSceneEventHandler();

    private void OnPlayerLoadedGameScene()
    {
        RpcId(Networking.ServerPeerId, MethodName.HandlePlayerLoadedGameSceneOnServer);
    }

    [Signal]
    public delegate void GameReadyEventHandler();

    [Signal]
    public delegate void FinalImageRequestedEventHandler();


    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void HandlePlayerConnectedOnClient(long playerId, string playerName)
    {
        if (_playerIdToLobbyId.ContainsKey(playerId))
            return;
        CurrentLobbyPlayers[playerId] = new Player(playerId, playerName);
        EmitSignal(SignalName.PlayerJoinedLobby, playerId);
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncLobbySettingsOnClient(
        int maxPlayers,
        string topic)
    {
        MaxPlayers = maxPlayers;
        Topic = topic;
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void HandlePlayerDisconnectedOnClient(long playerId)
    {
        if (!CurrentLobbyPlayers.Remove(playerId))
            return;
        EmitSignal(SignalName.PlayerLeftLobby, playerId);
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void StartDrawingRound()
    {
        GetTree().ChangeSceneToFile("res://Main/DrawingGame/Drawing/drawer_scene.tscn");
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void HandleGameReadyOnClient()
    {
        EmitSignal(SignalName.GameReady);
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

    #region RPC Methods

    

    #endregion
}
