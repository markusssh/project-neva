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
    
    private readonly Dictionary<string, Room> _roomRepo = new();
    private readonly Dictionary<long, string> _peerIdToRoomId = new();

    private Room GetRoomElseCreate(string roomId)
    {
        if (_roomRepo.TryGetValue(roomId, out var room))
        {
            return room;
        }

        var createRoom = new Room(roomId);
        _roomRepo[roomId] = createRoom;
        return createRoom;
    }

    private void DisconnectPeerFromRoom(long peerId, string roomId)
    {
        if (!_roomRepo.TryGetValue(roomId, out var room)) return;
        room.Peers.Remove(peerId);
        if (room.Peers.Count == 0)
        {
            _roomRepo.Remove(roomId);
            Logger.LogNetwork($"Room {roomId} removed");
        }
    }

    public void OnPeerConnected(long peerId)
    {
        if (!Networking.Instance.PeerAuthData.TryGetValue((int)peerId, out var peerAuthData))
        {
            GD.PrintErr($"Peer {peerId} AuthData is null");
            return;
        };
        
        var room = GetRoomElseCreate(peerAuthData.RoomId);
        if (room.State != Room.RoomState.WaitingPlayers || room.Peers.Count >= room.RoomSize)
        {
            //TODO: add ability to reconnect
            
            GD.PrintErr($"Cannot connect peer {peerId}! Room {room.RoomId} is not accepting players.");
            Networking.Instance.Multiplayer.DisconnectPeer((int)peerId);
            return;
        }
        
        var peer = new Peer(peerId, peerAuthData.PlayerName);
        Networking.Instance.PeerAuthData.Remove((int)peerId);
        _peerIdToRoomId.Add(peerId, room.RoomId);
        room.Peers.Add(peerId, peer);
        
        // Synchronize Room Settings With Newbie
        RpcId(peerId, MethodName.HandleSyncRoomSettingsOnClient, 
            room.RoomSize,
            room.MaxRounds,
            room.RoundLength);
        
        // Handle Peer Connected Func On Clients
        foreach (var existingPeerId in room.Peers.Keys)
        {
            if (peerId != existingPeerId)
            {
                RpcId(peerId, MethodName.HandlePeerConnectedOnClient, existingPeerId, room.Peers[existingPeerId].PlayerName);
            }
            RpcId(existingPeerId, MethodName.HandlePeerConnectedOnClient, peerId, room.Peers[peerId].PlayerName);
        }
        
        Logger.LogNetwork($"Peer {peerId} connected to room {room.RoomId}");

        if (room.Peers.Count == room.RoomSize)
        {
            Logger.LogNetwork($"Launching game in room {room.RoomId}");
            room.State = Room.RoomState.Playing;
            OnPlaying(room);
        }
    }

    public void OnPeerDisconnected(long peerId)
    {
        if (!_peerIdToRoomId.TryGetValue(peerId, out var roomId)) return;
        Networking.Instance.PeerAuthData.Remove((int)peerId);
        _peerIdToRoomId.Remove(peerId);

        foreach (var existingPeerId in _roomRepo[roomId].Peers.Keys)
        {
            if (peerId != existingPeerId)
            {
                RpcId(peerId, MethodName.HandlePeerDisconnectedOnClient, existingPeerId);
            }
            RpcId(existingPeerId, MethodName.HandlePeerDisconnectedOnClient, peerId);
        }

        DisconnectPeerFromRoom(peerId, roomId);
        GD.PrintErr($"Peer {peerId} disconnected from room {roomId}");
    }

    private void OnPlaying(Room room)
    {
        for (var i = 0; i < room.MaxRounds; i++)
        {
            OnRoomRoundStart(room);
        }
    }

    private void OnRoomRoundStart(Room room)
    {
        var drawer = room.SetRandomDrawer();
        var theme = RestServerPlaceholder.GetRandomTheme();
        RpcId(drawer.PlayerId, MethodName.StartDrawerScene);
        RpcId(drawer.PlayerId, MethodName.HandleRoundThemeReceivedOnClient, theme.ThemeName, theme.Author);
        foreach (var peer in room.Peers.Values.Where(peer => peer.PlayerId != drawer.PlayerId))
        {
            RpcId(peer.PlayerId, MethodName.StartGuesserScene);
        }
        foreach (var peer in room.Peers.Values)
        {
            RpcId(peer.PlayerId, MethodName.HandleRoundStartOnClient);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void OnImageChangeReceived(byte[] imageBytes)
    {
        var peerId = Multiplayer.GetRemoteSenderId();
        if (!_roomRepo.TryGetValue(_peerIdToRoomId[peerId], out var room)) return;
        foreach (var peer in room.Peers.Values.Where(peer => peer.PlayerId != peerId))
        {
            RpcId(peer.PlayerId, MethodName.HandleImageChangeReceivedOnClient, imageBytes);
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
    public readonly Godot.Collections.Dictionary<long, Peer> CurrentRoomPeers = new();
    public int MaxPlayers;
    public int MaxRounds;
    public int RoundLength;
    
    [Signal]
    public delegate void PeerJoinedRoomEventHandler(long peerId);
    
    [Signal]
    public delegate void PeerLeftRoomEventHandler(long peerId);

    [Signal]
    public delegate void PeerBecameDrawerEventHandler(long peerId);

    [Signal]
    public delegate void ImageBytesReceivedEventHandler(byte[] bytes);
    
    [Signal]
    public delegate void RoundThemeReceivedEventHandler(RoundTheme roundTheme);
    
    [Signal]
    public delegate void RoundStartedEventHandler();
    
    private readonly Queue<Action> _eventQueue = new();

    
    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void HandlePeerConnectedOnClient(long peerId, string playerName)
    {
        if (_peerIdToRoomId.ContainsKey(peerId))
            return;
        CurrentRoomPeers[peerId] = new Peer(peerId, playerName);
        EmitSignal(SignalName.PeerJoinedRoom, peerId);
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void HandleSyncRoomSettingsOnClient(
        int maxPlayers, 
        int maxRounds, 
        int roundLength)
    {
        MaxPlayers = maxPlayers;
        MaxRounds = maxRounds;
        RoundLength = roundLength;
    }
    
    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void HandlePeerDisconnectedOnClient(long peerId)
    {
        if (!CurrentRoomPeers.Remove(peerId))
            return;
        EmitSignal(SignalName.PeerLeftRoom, peerId);
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