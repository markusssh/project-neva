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

    public override void _Ready()
    {
        Instance = this;
    }

    #region Server Logic

    private readonly Dictionary<string, LobbyManager> _lobbyManagers = new();
    private readonly Dictionary<long, string> _playerIdToLobbyManagerId = new();
    
    #region Broadcasting
    
    public void Server_BroadcastLobby(Lobby lobby, string methodName, params Variant[] args)
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
            lobby.DrawingTimeSec);
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
    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Server_HandlePlayerDrawingStateChange(bool drawingOn)
    {
        var playerId = Multiplayer.GetRemoteSenderId();
        Server_GetLobbyByPlayerId(playerId)?.OnPlayerDrawingStateChanged(playerId, drawingOn);
    }

    #endregion

    #region Client Logic

    public readonly Godot.Collections.Dictionary<long, Player> Client_Players = new();

    public int Client_MaxPlayers;
    public int Client_MaxRounds;
    public float Client_DrawingTimeSec;
    public string Client_Topic;

    [Signal] public delegate void PlayerJoinedLobbyEventHandler(long playerId);
    
    [Signal] public delegate void PlayerLeftLobbyEventHandler(long playerId);

    [Signal] public delegate void DrawingGameStartedEventHandler();

    [Signal] public delegate void FinalImageRequestedEventHandler();

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

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Client_LoadDrawingScene()
    {
        GetTree().ChangeSceneToFile("res://Main/DrawingGame/Drawing/drawing_scene.tscn");
    }

    public void Client_NotifyNewSceneReady()
    {
        RpcId(Networking.ServerPeerId, MethodName.Server_HandlePlayerLoadNewScene);
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
        RpcId(Networking.ServerPeerId, MethodName.Server_ReceiveFinalImage, imageData);
    }

    private void Client_NotifyDrawingStateChanged(bool drawingOn)
    {
        RpcId(Networking.ServerPeerId, MethodName.Server_HandlePlayerDrawingStateChange, drawingOn);
    }
    
    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Client_ReceiveFinalImages(Godot.Collections.Dictionary<long, byte[]> images)
    {
        foreach (var player in Client_Players.Values)
        {
            player.FinalImageData = images[player.PlayerId];
        }
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Client_LoadRatingScene()
    {
        GetTree().ChangeSceneToFile("res://Main/DrawingGame/Rating/RatingScene.tscn");
    }

    #endregion
    
}
