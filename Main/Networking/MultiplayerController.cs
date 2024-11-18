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
        if (room.State != Room.RoomState.WaitingPlayers)
        {
            //TODO: add ability to reconnect
            
            GD.PrintErr($"Cannot connect peer {peerId}! Room {room.RoomId} is not accepting players.");
            Networking.Instance.Multiplayer.DisconnectPeer((int)peerId);
            return;
        }
        
        var peer = new Peer(peerAuthData.PlayerName);
        Networking.Instance.PeerAuthData.Remove((int)peerId);
        _peerIdToRoomId.Add(peerId, room.RoomId);
        room.Peers.Add(peerId, peer);
        
        foreach (var existingPeerId in room.Peers.Keys)
        {
            if (peerId != existingPeerId)
            {
                RpcId(peerId, MethodName.OnPeerConnectedRpc, existingPeerId, room.Peers[existingPeerId].PlayerName);
            }
            RpcId(existingPeerId, MethodName.OnPeerConnectedRpc, peerId, room.Peers[peerId].PlayerName);
        }
        
        GD.Print($"Peer {peerId} connected to room {room.RoomId}");
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
    
    public void OnRoomLoadingPlayers(string roomId)
    {
        throw new NotImplementedException();
    }
    
    public void OnRoomPendingRoundStart(string roomId)
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
        CurrentRoomPeers[peerId] = new Peer(playerName);
        EmitSignal(SignalName.PeerJoinedRoom, peerId);
        GD.Print($"Peer {peerId} joined the room.");
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
    private void OnPeerIsDrawerChange(long peerId, bool isDrawer)
    {
        if (!CurrentRoomPeers.TryGetValue(peerId, out var peer)) return;
        if (peer.IsDrawer == isDrawer) return;
        peer.IsDrawer = isDrawer;
        EmitSignal(SignalName.PeerBecameDrawer, peerId);
    }
    
    
}

public class Room
{
    public enum RoomState
    {
        WaitingPlayers,
        LoadingPlayers,
        PendingRoundStart,
        Playing,
        Closing
    }

    public Room(string roomId)
    {
        RoomId = roomId;
    }

    public string RoomId { get; set; }

    public Dictionary<long, Peer> Peers { get; set; } = new();
    
    private RoomState _state = RoomState.WaitingPlayers;
    public RoomState State
    {
        get => _state;
        set
        {
            switch (value)
            {
                case RoomState.LoadingPlayers:
                    MultiplayerController.Instance.OnRoomLoadingPlayers(RoomId);
                    break;
                case RoomState.PendingRoundStart:
                    MultiplayerController.Instance.OnRoomPendingRoundStart(RoomId);
                    break;
                case RoomState.WaitingPlayers:
                    break;
                case RoomState.Playing:
                    break;
                case RoomState.Closing:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }

            _state = value;
        }
    }
    
    
}

public partial class Peer : GodotObject
{
    public Peer()
    {
    }

    public Peer(string playerName)
    {
        PlayerName = playerName;
    }

    public string PlayerName { get; set; }
    public bool WinState { get; set; } = false;
    public int Score { get; set; } = 0;
    public bool IsDrawer { get; set; } = false;
}

public struct PlayerGameAction
{
    public bool WinState { get; set; }
    public int Score { get; set; }
    
}