using System;
using System.Collections.Generic;
using Godot;

namespace ProjectNeva.Main.Networking;

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
            GD.Print($"Room {roomId} removed");
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
        RpcId(peerId, MethodName.SyncMaxPlayers, room.RoomSize);
        
        foreach (var existingPeerId in room.Peers.Keys)
        {
            if (peerId != existingPeerId)
            {
                RpcId(peerId, MethodName.OnPeerConnectedRpc, existingPeerId, room.Peers[existingPeerId].PlayerName);
            }
            RpcId(existingPeerId, MethodName.OnPeerConnectedRpc, peerId, room.Peers[peerId].PlayerName);
        }
        
        GD.Print($"Peer {peerId} connected to room {room.RoomId}");

        if (room.Peers.Count == room.RoomSize)
        {
            GD.Print($"Launching game in room {room.RoomId}");
            room.State = Room.RoomState.LoadingPlayers;
            OnRoomLoadingPlayers(room);
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
                RpcId(peerId, MethodName.OnPeerDisconnectedRpc, existingPeerId);
            }
            RpcId(existingPeerId, MethodName.OnPeerDisconnectedRpc, peerId);
        }

        DisconnectPeerFromRoom(peerId, roomId);
        GD.PrintErr($"Peer {peerId} disconnected from room {roomId}");
    }
    
    private void OnRoomLoadingPlayers(Room room)
    {
        var drawer = room.SetRandomDrawer();
        RpcId(drawer.PlayerId, MethodName.StartDrawerScene);
        foreach (var peer in room.Peers.Values)
        {
            if (peer.PlayerId == drawer.PlayerId) continue;
            RpcId(peer.PlayerId, MethodName.StartGuesserScene);
        }
        OnPlaying(room);
    }
    
    public void OnPlaying(Room room)
    {
        throw new NotImplementedException();
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
    public int MaxPlayers = -1;
    
    [Signal]
    public delegate void PeerJoinedRoomEventHandler(long peerId);
    
    [Signal]
    public delegate void PeerLeftRoomEventHandler(long peerId);
    
    [Signal]
    public delegate void PeerBecameDrawerEventHandler(long peerId);

    
    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void OnPeerConnectedRpc(long peerId, string playerName)
    {
        if (_peerIdToRoomId.ContainsKey(peerId))
        {
            GD.PrintErr($"Peer {peerId} is already connected to a room.");
            return;
        }
        CurrentRoomPeers[peerId] = new Peer(peerId, playerName);
        EmitSignal(SignalName.PeerJoinedRoom, peerId);
        GD.Print($"Peer {peerId} joined the room.");
    }

    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncMaxPlayers(int maxPlayers)
    {
        MaxPlayers = maxPlayers;
    }
    
    [Rpc(TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void OnPeerDisconnectedRpc(long peerId)
    {
        if (!CurrentRoomPeers.Remove(peerId))
        {
            GD.PrintErr($"Could not remove peer {peerId} from room.");
            return;
        };
        EmitSignal(SignalName.PeerLeftRoom, peerId);
        GD.Print($"Peer {peerId} left the room.");
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
    
}