using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;
using Logger = ProjectNeva.Main.Utils.Logger.Logger;

namespace ProjectNeva.Main.NetworkingArchitecture;

public partial class Networking : Node
{
    public static Networking Instance { get; private set; } = null!;

    public const int GameServerPeerId = 1;
    public const int ServerMaxConnections = 3000;

    private static string _gameServerIp = "127.0.0.1";
    private static int _gameServerPort = 8081;
    
    private static 

    public bool IsServer { get; set; }
    public bool IsClient => !IsServer;

    public readonly Dictionary<long, AuthResponseDto> PeerAuthData = new();
    private string _debugAuthData;

    public new SceneMultiplayer Multiplayer { get; set; }

    public override void _EnterTree()
    {
        if (OS.HasFeature("dedicated_server") ||
            OS.HasFeature("debug") && OS.GetCmdlineArgs().Contains("--is_server")) IsServer = true;
        if (GetTree().GetMultiplayer() is SceneMultiplayer)
        {
            Multiplayer = GetTree().GetMultiplayer() as SceneMultiplayer;
            ConnectMultiplayerHandlers();
        }
        else
        {
            GD.PrintErr("MultiplayerAPIs implementation is not supported!");
            GetTree().Paused = true;
        }

        if (OS.HasFeature("player1"))
        {
            _debugAuthData = "1";
        }
        else if (OS.HasFeature("player2"))
        {
            _debugAuthData = "2";
        }
        else if (OS.HasFeature("player3"))
        {
            _debugAuthData = "3";
        }
        else if (OS.HasFeature("player4"))
        {
            _debugAuthData = "4";
        }
        else if (OS.HasFeature("player5"))
        {
            _debugAuthData = "5";
        }
        else if (OS.HasFeature("player6"))
        {
            _debugAuthData = "6";
        }
    }
    
    private void ConnectMultiplayerHandlers()
    {
        if (IsClient)
        {
            Multiplayer.PeerConnected += (id) =>
            {
                if (id != 1)
                {
                    Logger.LogNetwork($"Peer {id} connected to server.");
                }
            };
            Multiplayer.PeerDisconnected += (id) => { Logger.LogNetwork($"Peer {id} disconnected from server."); };
            Multiplayer.ConnectionFailed += () => { Logger.LogNetwork($"Connection failed!"); };
            Multiplayer.ConnectedToServer += () => { Logger.LogNetwork("Connection successful."); };
            Multiplayer.ServerDisconnected += () => { Logger.LogNetwork("Disconnected."); };
            Multiplayer.PeerAuthenticating += (peerId) =>
            {
                if (peerId != GameServerPeerId) return;
                Multiplayer.SendAuth(GameServerPeerId, Encoding.UTF8.GetBytes(_debugAuthData));
                Logger.LogNetwork($"Authenticating...");
            };
            Multiplayer.PeerAuthenticationFailed += (peerId) => { GD.PrintErr($"Authentication failed!"); };
            Multiplayer.SetAuthCallback(new Callable(this, nameof(ClientAuthRequestHandle)));
        }
        else
        {
            Multiplayer.PeerConnected += (id) =>
            {
                Logger.LogNetwork($"Peer {id} connected to server.");
                MultiplayerController.Instance.Server_OnPeerConnected(id);
            };
            Multiplayer.PeerDisconnected += (id) =>
            {
                Logger.LogNetwork($"Peer {id} disconnected from server.");
                MultiplayerController.Instance.Server_OnPeerDisconnected(id);
            };
            Multiplayer.PeerAuthenticating += (id) => { Logger.LogNetwork($"Peer {id} authenticating..."); };
            Multiplayer.PeerAuthenticationFailed += (id) => { GD.PrintErr($"Peer {id} authentication failed!"); };
            Multiplayer.SetAuthCallback(new Callable(this, nameof(ServerAuthRequestHandle)));
        }
    }

    private void ClientAuthRequestHandle(int id, byte[] data)
    {
        Logger.LogNetwork($"Authentication data: {Encoding.UTF8.GetString(data)}.");
        Multiplayer.CompleteAuth(id);
        Logger.LogNetwork($"Authentication complete.");
    }

    private void ServerAuthRequestHandle(int id, byte[] data)
    {
        var s = Encoding.UTF8.GetString(data);
        Logger.LogNetwork($"Peer {id} authentication data: {s}.");

        if (AuthIsValid(s, id))
        {
            Multiplayer.SendAuth(id, data);
            Multiplayer.CompleteAuth(id);
            Logger.LogNetwork($"Peer {id} authentication complete.");
        }
        else
        {
            Multiplayer.DisconnectPeer(id);
        }
    }

    //TODO: add logic
    //server remembers peer id if auth data is valid
    private bool AuthIsValid(string data, int id)
    {
        var authData = RestServerPlaceholder.Authenticate(data);
        if (!authData.AuthSuccess) return false;
        PeerAuthData[id] = authData;
        return true;
    }

    public override void _Ready()
    {
        Instance = this;
        if (IsClient) return;
        
        Logger.LogNetwork("Starting server...");
        if (StartServer() != Error.Ok)
        {
            GD.PrintErr("Server start failed!");
            return;
        }

        GetTree().GetRoot().Ready += () =>
        {
            Logger.LogNetwork("Server is ready!");
        };
    }

    private Error StartServer()
    {
        if (!IsServer) return Error.Failed;
        var peer = new ENetMultiplayerPeer();
        var error = peer.CreateServer(_gameServerPort, ServerMaxConnections);

        if (error != Error.Ok)
        {
            return error;
        }

        Multiplayer.MultiplayerPeer = peer;
        Multiplayer.ServerRelay = false;
        return Error.Ok;
    }

    public Error JoinGame()
    {
        if (IsServer) return Error.Failed;
        var peer = new ENetMultiplayerPeer();
        var error = peer.CreateClient(_gameServerIp, _gameServerPort);
        if (error != Error.Ok)
        {
            GD.PrintErr("Failed to create client!");
            return error;
        }
        
        Multiplayer.MultiplayerPeer = peer;
        Logger.LogNetwork($"Client started on peer {Multiplayer.GetUniqueId()}.");

        var args = OS.GetCmdlineArgs();
        foreach (var arg in args)
        {
            if (!arg.StartsWith("player_auth=")) continue;
            var jwt = arg["player_auth=".Length..];
            Multiplayer.SendAuth(GameServerPeerId, Encoding.UTF8.GetBytes(jwt));
            break;
        }

        return Error.Ok;
    }

    public bool IsMyPeer(int peerId)
    {
        var thisPeer = Multiplayer.MultiplayerPeer.GetUniqueId();
        return thisPeer == peerId;
    }
}

//TODO: make a dedicated server
#region REST Server Simulation

public static class RestServerPlaceholder
{
    public static AuthResponseDto Authenticate(string jwt)
    {
        return jwt switch
        {
            "1" => new AuthResponseDto(true, "0", "Ваня"),
            "2" => new AuthResponseDto(true, "0", "Галя"),
            "3" => new AuthResponseDto(true, "0", "Рита"),
            "4" => new AuthResponseDto(true, "1", "Таня"),
            "5" => new AuthResponseDto(true, "1", "Саня"),
            "6" => new AuthResponseDto(true, "1", "Галя"),
            _ => throw new ArgumentOutOfRangeException(nameof(jwt), jwt, null)
        };
    }
    
}

public record AuthResponseDto(bool AuthSuccess, string LobbyId, string PlayerName);

#endregion