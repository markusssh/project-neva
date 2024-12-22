using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Godot;
using ProjectNeva.Main.LoggerUtils;

namespace ProjectNeva.Main.NetworkingArchitecture;

public partial class MultiplayerController : Node
{
    public static MultiplayerController Instance {get; private set;}
    
//                        
//     _____    _____   ______    _   _    _____   ______            _____    _____   ______    _____ 
//    /  ___|  |  ___|  | ___ \  | | | |  |  ___|  | ___ \          /  ___|  |_   _|  |  _  \  |  ___|
//    \ `--.   | |__    | |_/ /  | | | |  | |__    | |_/ /          \ `--.     | |    | | | |  | |__  
//     `--. \  |  __|   |    /   | | | |  |  __|   |    /            `--. \    | |    | | | |  |  __| 
//    /\__/ /  | |___   | |\ \   | |/ /   | |___   | |\ \           /\__/ /   _| |_   | |/ /   | |___ 
//    \____/   \____/   |_| \_\  |___/    \____/   |_| \_\          \____/   \_____/  |___/    \____/ 
//
    
    private readonly Dictionary<string, Lobby> _lobbyRepo = new();
    private readonly Dictionary<long, string> _playerIdToLobbyId = new();

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

    public void OnPeerConnected(long newPeerId)
    {
        if (!Networking.Instance.PeerAuthData.TryGetValue((int)newPeerId, out var peerAuthData))
        {
            GD.PrintErr($"Peer {newPeerId} AuthData is null");
            return;
        };
        
        var lobby = GetLobbyElseCreate(peerAuthData.LobbyId);
        if (lobby.State != Lobby.LobbyState.WaitingPlayers || lobby.Players.Count >= lobby.LobbySize)
        {
            //TODO: add ability to reconnect
            
            GD.PrintErr($"Cannot connect peer {newPeerId}! Lobby {lobby.LobbyId} is not accepting players.");
            Networking.Instance.Multiplayer.DisconnectPeer((int)newPeerId);
            return;
        }
        
        var player = new Player(newPeerId, peerAuthData.PlayerName);
        Networking.Instance.PeerAuthData.Remove((int)newPeerId);
        _playerIdToLobbyId.Add(newPeerId, lobby.LobbyId);
        lobby.Players.Add(newPeerId, player);
        
        // Synchronize Lobby Settings With Newbie
        RpcId(newPeerId, MethodName.HandleSyncLobbySettingsOnClient, 
            lobby.LobbySize,
            lobby.MaxRounds,
            lobby.RoundLength);
        
        // Handle Peer Connected Func On Clients
        foreach (var playerId in lobby.Players.Keys)
        {
            if (newPeerId != playerId)
            {
                RpcId(newPeerId, MethodName.HandlePlayerConnectedOnClient, playerId, lobby.Players[playerId].PlayerName);
            }
            RpcId(playerId, MethodName.HandlePlayerConnectedOnClient, newPeerId, lobby.Players[newPeerId].PlayerName);
        }
        
        Logger.LogNetwork($"Player {newPeerId} connected to lobby {lobby.LobbyId}");

        if (lobby.Players.Count == lobby.LobbySize)
        {
            Logger.LogNetwork($"Launching game in lobby {lobby.LobbyId}");
            lobby.State = Lobby.LobbyState.Playing;
            OnPlaying(lobby);
        }
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
        GD.PrintErr($"Peer {disconnectingPeerId} disconnected from lobby {lobbyId}");
    }

    private void OnPlaying(Lobby lobby)
    {
        for (var i = 0; i < lobby.MaxRounds; i++)
        {
            OnLobbyRoundStart(lobby);
        }
    }

    private void OnLobbyRoundStart(Lobby lobby)
    {
        var drawer = lobby.SetRandomDrawer();
        var theme = RestServerPlaceholder.GetRandomTheme();
        RpcId(drawer.PlayerId, MethodName.StartDrawerScene);
        RpcId(drawer.PlayerId, MethodName.HandleRoundThemeReceivedOnClient, theme.ThemeName, theme.Author);
        foreach (var player in lobby.Players.Values.Where(player => player.PlayerId != drawer.PlayerId))
        {
            RpcId(player.PlayerId, MethodName.StartGuesserScene);
        }
        foreach (var player in lobby.Players.Values)
        {
            RpcId(player.PlayerId, MethodName.HandleRoundStartOnClient);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void OnImageChangeReceived(byte[] imageBytes)
    {
        var playerId = Multiplayer.GetRemoteSenderId();
        if (!_lobbyRepo.TryGetValue(_playerIdToLobbyId[playerId], out var lobby)) return;
        foreach (var player in lobby.Players.Values.Where(player => player.PlayerId != playerId))
        {
            RpcId(player.PlayerId, MethodName.HandleImageChangeReceivedOnClient, imageBytes);
        }
    }

    public override void _Ready()
    {
        Instance = this;
    }
    
//                        
//     _____    _        _____    _____    _   _    _____            _____    _____   ______    _____ 
//    /  __ \  | |      |_   _|  |  ___|  | \ | |  |_   _|          /  ___|  |_   _|  |  _  \  |  ___|
//    | /  \/  | |        | |    | |__    |  \| |    | |            \ `--.     | |    | | | |  | |__  
//    | |      | |        | |    |  __|   | . ` |    | |             `--. \    | |    | | | |  |  __| 
//    | \__/\  | |____   _| |_   | |___   | |\  |    | |            /\__/ /   _| |_   | |/ /   | |___ 
//     \____/  \_____/  \_____/  \____/   \_| \_/    \_/            \____/   \_____/  |___/    \____/ 
//
    public readonly Godot.Collections.Dictionary<long, Player> CurrentLobbyPlayers = new();
    public int MaxPlayers;
    public int MaxRounds;
    public int RoundLength;
    
    [Signal]
    public delegate void PlayerJoinedLobbyEventHandler(long playerId);
    
    [Signal]
    public delegate void PlayerLeftLobbyEventHandler(long playerId);

    [Signal]
    public delegate void PlayerBecameDrawerEventHandler(long playerId);

    [Signal]
    public delegate void ImageBytesReceivedEventHandler(byte[] bytes);
    
    [Signal]
    public delegate void RoundThemeReceivedEventHandler(RoundTheme roundTheme);
    
    [Signal]
    public delegate void RoundStartedEventHandler();
    
    private readonly Queue<Action> _eventQueue = new();

    
    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void HandlePlayerConnectedOnClient(long playerId, string playerName)
    {
        if (_playerIdToLobbyId.ContainsKey(playerId))
            return;
        CurrentLobbyPlayers[playerId] = new Player(playerId, playerName);
        EmitSignal(SignalName.PlayerJoinedLobby, playerId);
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void HandleSyncLobbySettingsOnClient(
        int maxPlayers, 
        int maxRounds, 
        int roundLength)
    {
        MaxPlayers = maxPlayers;
        MaxRounds = maxRounds;
        RoundLength = roundLength;
    }
    
    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void HandlePlayerDisconnectedOnClient(long playerId)
    {
        if (!CurrentLobbyPlayers.Remove(playerId))
            return;
        EmitSignal(SignalName.PlayerLeftLobby, playerId);
    }
    
    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void StartDrawerScene()
    {
        GetTree().ChangeSceneToFile("res://Main/DrawingGame/Drawer/drawer_scene.tscn");
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void StartGuesserScene()
    {
        GetTree().ChangeSceneToFile("res://Main/DrawingGame/Guesser/guesser_scene.tscn");
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void HandleRoundThemeReceivedOnClient(string name, string author)
    {
        Logger.LogNetwork("HandleRoundThemeReceivedOnClient: " + name + ", author: " + author);
        var roundTheme = new RoundTheme(name, author);
        EmitSignal(SignalName.RoundThemeReceived, roundTheme);
    }
    
    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void HandleRoundStartOnClient()
    {
        _eventQueue.Enqueue(() => EmitSignal(SignalName.RoundStarted));
    }

    private void SendImageChangeByDrawer(Image image)
    {
        var imageBytes = image.GetData();
        imageBytes = Compress(imageBytes);
        RpcId(Networking.ServerPeerId, MethodName.OnImageChangeReceived, imageBytes);
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void HandleImageChangeReceivedOnClient(byte[] imageBytes)
    {
        imageBytes = Decompress(imageBytes);
        EmitSignal(SignalName.ImageBytesReceived, imageBytes);
    }

    private void ProcessEventQueue()
    {
        while (_eventQueue.Count > 0)
        {
            _eventQueue.Dequeue().Invoke();
            Logger.LogNetwork("Round Started!");
        }
    }

    private static byte[] Compress(byte[] data)
    {
        using var compressedStream = new MemoryStream();
        using var brotliStream = new BrotliStream(compressedStream, CompressionLevel.Optimal);
        brotliStream.Write(data, 0, data.Length);
        brotliStream.Close();
        return compressedStream.ToArray();
    }

    private static byte[] Decompress(byte[] data)
    {
        using var compressedStream = new MemoryStream(data);
        using var brotliStream = new BrotliStream(compressedStream, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        brotliStream.CopyTo(resultStream);
        return resultStream.ToArray();
    }
}