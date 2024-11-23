using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;

namespace ProjectNeva.Main.Networking;

public partial class Networking : Node
{
    public static Networking Instance { get; private set; }
    
    public new SceneMultiplayer Multiplayer { get; set; }

    private bool IsServer { get; set; }
    private const string ServerIp = "127.0.0.1";
    private const int ServerPort = 8081;
    public const int ServerPeerId = 1;
    public const int ServerMaxConnections = 100;
    public readonly Dictionary<int, AuthResponseDto> PeerAuthData = new();

    private string DebugAuthData;

    public override void _EnterTree()
    {
        //грязь
        if (OS.HasFeature("player1"))
        {
            DebugAuthData = "1";
        }
        else if (OS.HasFeature("player2"))
        {
            DebugAuthData = "2";
        }
        else if (OS.HasFeature("player3"))
        {
            DebugAuthData = "3";
        }
        else if (OS.HasFeature("player4"))
        {
            DebugAuthData = "4";
        }
        else if (OS.HasFeature("player5"))
        {
            DebugAuthData = "5";
        }
        else if (OS.HasFeature("player6"))
        {
            DebugAuthData = "6";
        }
        
        
        //TODO: make --is_server only for debug!!!
        if (OS.HasFeature("dedicated_server") || OS.GetCmdlineArgs().Contains("--is_server")) IsServer = true;
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
    }

    private void ConnectMultiplayerHandlers()
    {
        if (!IsServer)
        {
            Multiplayer.PeerConnected += (id) =>
            {
                if (id != 1)
                {
                    GD.Print($"Peer {id} connected to server.");
                }
            };
            Multiplayer.PeerDisconnected += (id) =>
            {
                GD.Print($"Peer {id} disconnected from server.");
            };
            Multiplayer.ConnectionFailed += () =>
            {
                GD.Print($"Connection failed!");
            };
            Multiplayer.ConnectedToServer += () =>
            {
                GD.Print("Connection successful.");
            };
            Multiplayer.ServerDisconnected += () =>
            {
                GD.Print("Disconnected.");
            };
            Multiplayer.PeerAuthenticating += (peerId) =>
            {
                if (peerId != ServerPeerId) return;
                Multiplayer.SendAuth(ServerPeerId, Encoding.UTF8.GetBytes(DebugAuthData));
                GD.Print($"Authenticating...");
            };
            Multiplayer.PeerAuthenticationFailed += (peerId) =>
            {
                GD.PrintErr($"Authentication failed!");
            };
            Multiplayer.SetAuthCallback(new Callable(this, nameof(ClientAuthRequestHandle)));
        }
        else
        {
            Multiplayer.PeerConnected += (id) =>
            { 
                GD.Print($"Peer {id} connected to server.");
                MultiplayerController.Instance.OnPeerConnected(id);
            };
            Multiplayer.PeerDisconnected += (id) =>
            { 
                GD.Print($"Peer {id} disconnected from server.");
                MultiplayerController.Instance.OnPeerDisconnected(id);
            };
            Multiplayer.PeerAuthenticating += (id) =>
            {
                GD.Print($"Peer {id} authenticating...");
            };
            Multiplayer.PeerAuthenticationFailed += (id) =>
            {
                GD.PrintErr($"Peer {id} authentication failed!");
            };
            Multiplayer.SetAuthCallback(new Callable(this, nameof(ServerAuthRequestHandle)));
        }
    }

    private void ClientAuthRequestHandle(int id, byte[] data)
    {
        GD.Print($"Authentication data: {Encoding.UTF8.GetString(data)}.");
        Multiplayer.CompleteAuth(id);
        GD.Print($"Authentication complete.");
    }
    
    private void ServerAuthRequestHandle(int id, byte[] data)
    {
        var s = Encoding.UTF8.GetString(data);
        GD.Print($"Peer {id} authentication data: {s}.");

        if (AuthIsValid(s, id))
        {
            Multiplayer.SendAuth(id, data);
            Multiplayer.CompleteAuth(id);
            GD.Print($"Peer {id} authentication complete.");
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
        if (IsServer)
        {
            GD.Print("Starting server...");
            if (StartServer() != Error.Ok)
            {
                GD.PrintErr("Server start failed!");
                return;
            }
            GetTree().GetRoot().Ready += () =>
            {
                //CreateNewEmptyRoom();
                GD.Print("Server is ready!");
            };
        }
    }
    
    private Error StartServer()
    {
        if (!IsServer) return Error.Failed;
        var peer = new ENetMultiplayerPeer();
        var error = peer.CreateServer(ServerPort, ServerMaxConnections);

        if (error != Error.Ok)
        {
            return error;
        }

        Multiplayer.MultiplayerPeer = peer;
        return Error.Ok;
    }

    public Error JoinGame()
    {
        if (IsServer) return Error.Failed;
        var peer = new ENetMultiplayerPeer();
        var error = peer.CreateClient(ServerIp, ServerPort);
        if (error != Error.Ok)
        {
            GD.PrintErr("Failed to create client!");
            return error;
        }
        GD.Print($"Client started on peer {Multiplayer.GetUniqueId()}!");
        Multiplayer.MultiplayerPeer = peer;
        
        var args = OS.GetCmdlineArgs();
        foreach (var arg in args)
        {
            if (!arg.StartsWith("player_auth=")) continue;
            var jwt = arg["player_auth=".Length..];
            Multiplayer.SendAuth(ServerPeerId, Encoding.UTF8.GetBytes(jwt));
            break;
        }
        return Error.Ok;
    }
}

//TODO: make a dedicated server
//                                                
//    ______    _____    _____    _____            _____    _____   ______    _   _    _____   ______ 
//    | ___ \  |  ___|  /  ___|  |_   _|          /  ___|  |  ___|  | ___ \  | | | |  |  ___|  | ___ \
//    | |_/ /  | |__    \ `--.     | |            \ `--.   | |__    | |_/ /  | | | |  | |__    | |_/ /
//    |    /   |  __|    `--. \    | |             `--. \  |  __|   |    /   | | | |  |  __|   |    / 
//    | |\ \   | |___   /\__/ /    | |            /\__/ /  | |___   | |\ \   | |/ /   | |___   | |\ \ 
//    |_| \_\  \____/   \____/     \_/            \____/   \____/   |_| \_\  |___/    \____/   |_| \_\
//
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
    
    public static GameThemeDto GetRandomGameThemeDto()
    {
        Random rnd = new();
        var rndInt = rnd.Next(0, 4);
        return rndInt switch
        {
            0 => new GameThemeDto("Илья Ефимович Репин", "Бурлаки на Волге"),
            1 => new GameThemeDto("Леонардо да Винчи", "Мона Лиза"),
            2 => new GameThemeDto("Каспар Давид Фридрих", "Странник над морем тумана"),
            3 => new GameThemeDto("Иван Константинович Айвазовский", "Девятый вал"),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
}

public record AuthResponseDto(bool AuthSuccess, string RoomId, string PlayerName);
public record GameThemeDto(string AuthorName, string PaintingName);