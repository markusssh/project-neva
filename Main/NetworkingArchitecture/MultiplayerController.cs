using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using ProjectNeva.Main.LoggerUtils;
using Array = Godot.Collections.Array;
using FileAccess = Godot.FileAccess;

namespace ProjectNeva.Main.NetworkingArchitecture;

public partial class MultiplayerController : Node
{
    public static MultiplayerController Instance {get; private set;}

    public override void _Ready()
    {
        Instance = this;

        if (Networking.Instance.IsServer)
        {
            if (FileAccess.FileExists(NetExportFile))
            {
                var file = FileAccess.Open(NetExportFile, FileAccess.ModeFlags.Read);
                var json = "";
                while (!file.EofReached())
                {
                    json += file.GetLine();
                }

                var topicIds = (Array)Json.ParseString(json);
                foreach (int topicId in topicIds)
                {
                    TopicIds.Add(topicId);
                }
            }
            else
            {
                throw new FileNotFoundException("NetExportFile not found");
            }
        }
        
        PlayerLoadedGameScene += OnPlayerLoadedGameScene;
        PlayerMadeAGuess += OnPlayerMadeAGuess;
    }


    //                        
    //     _____    _____   ______    _   _    _____   ______            _____    _____   ______    _____ 
    //    /  ___|  |  ___|  | ___ \  | | | |  |  ___|  | ___ \          /  ___|  |_   _|  |  _  \  |  ___|
    //    \ `--.   | |__    | |_/ /  | | | |  | |__    | |_/ /          \ `--.     | |    | | | |  | |__  
    //     `--. \  |  __|   |    /   | | | |  |  __|   |    /            `--. \    | |    | | | |  |  __| 
    //    /\__/ /  | |___   | |\ \   | |/ /   | |___   | |\ \           /\__/ /   _| |_   | |/ /   | |___ 
    //    \____/   \____/   |_| \_\  |___/    \____/   |_| \_\          \____/   \_____/  |___/    \____/ 
    //

    private const string NetExportFile = "res://Main/NetworkingArchitecture/NetExportData.json";

    private readonly Dictionary<string, Lobby> _lobbyRepo = new();

    private readonly Dictionary<long, string> _playerIdToLobbyId = new();

    [Signal]
    public delegate void LobbyIsLoadedEventHandler(string lobbyId);

    public HashSet<int> TopicIds = new();

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

    public async Task OnPeerConnected(long newPeerId)
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
            await OnPlaying(lobby);
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
        Logger.LogMessage($"Disconnected on peer {disconnectingPeerId}");
    }

    private async Task OnPlaying(Lobby lobby)
    {
        for (var i = 0; i < lobby.MaxRounds; i++)
        {
            await OnLoading(lobby);
            OnRoundStart(lobby);
            foreach (var player in lobby.Players.Values)
            {
                RpcId(player.PlayerId, MethodName.HandleGameReadyOnClient);
            }
        }
    }

    private async Task OnLoading(Lobby lobby)
    {
        var tcs = new TaskCompletionSource();

        // Подписываемся на событие
        void OnLobbyLoaded(string lobbyId)
        {
            if (lobbyId == lobby.LobbyId)
            {
                tcs.TrySetResult();
                LobbyIsLoaded -= OnLobbyLoaded;
            }
        }
        
        LobbyIsLoaded += OnLobbyLoaded;

        // Выполняем основную логику метода
        lobby.SetRandomDrawerId();
        foreach (var player in lobby.Players.Values.Where(player => player.PlayerId != lobby.DrawerId))
        {
            RpcId(player.PlayerId, MethodName.StartGuesserScene);
        }
        RpcId(lobby.DrawerId, MethodName.StartDrawerScene);

        // Ждем, пока сигнал будет вызван
        await tcs.Task;
    }

    private void OnRoundStart(Lobby lobby)
    {
        lobby.SetRandomTopicId();
        RpcId(lobby.DrawerId, MethodName.HandleTopicIdReceivedOnClient, lobby.CurrentTopicId);
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
    private void HandlePlayerGuessOnServer(int topicId)
    {
        var playerId = Multiplayer.GetRemoteSenderId();
        if (!_lobbyRepo.TryGetValue(_playerIdToLobbyId[playerId], out var lobby)) return;
        var correctGuess = lobby.CurrentTopicId == topicId;
        Logger.LogNetwork($"Player {playerId} from lobby {lobby.LobbyId} has made a guess: {topicId}, {correctGuess}");
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
    public delegate void PlayerMadeAGuessEventHandler(int topicId);

    private void OnPlayerMadeAGuess(int topicId)
    {
        RpcId(Networking.ServerPeerId, MethodName.HandlePlayerGuessOnServer, topicId);
    }


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
    private void HandleTopicIdReceivedOnClient(int id)
    {
        Logger.LogNetwork("Round topic id is: " + id);
        EmitSignal(SignalName.TopicIdReceived, id);
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void HandleGameReadyOnClient()
    {
        EmitSignal(SignalName.GameReady);
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