using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using Environment = System.Environment;
using HttpClient = System.Net.Http.HttpClient;
using Logger = ProjectNeva.Main.Utils.Logger.Logger;

namespace ProjectNeva.Main.NetworkingArchitecture;

public partial class Networking : Node
{
    private static string _serverManagerUrl = Environment.GetEnvironmentVariable("SERVER_MANAGER_URL") ?? "http://localhost:8080";
    private static string _serverToken = Environment.GetEnvironmentVariable("SERVER_TOKEN") 
                                        ?? "4791da355540b0cd33420b55c066802dd09998d8a2c5a44667ee4ccc5a625e25"; // dev

    private static HttpClient _httpClient = new HttpClient();
    
    public static Networking Instance { get; private set; } = null!;

    public const int GameServerPeerId = 1;
    public const int ServerMaxConnections = 3000;

    private static string _gameServerIp = "127.0.0.1";
    private static int _gameServerPort = 8081;

    public bool IsServer { get; set; }
    public bool IsClient => !IsServer;

    public readonly Dictionary<long, JwtValidationResult> PeerAuthData = new();
    private string _debugAuthData;

    public new SceneMultiplayer Multiplayer { get; private set; }

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
            Multiplayer.ConnectionFailed += () => { Logger.LogNetwork("Connection failed!"); };
            Multiplayer.ConnectedToServer += () => { Logger.LogNetwork("Connection successful."); };
            Multiplayer.ServerDisconnected += () => { Logger.LogNetwork("Disconnected."); };
            Multiplayer.PeerAuthenticating += (peerId) =>
            {
                if (peerId != GameServerPeerId) return;
                Multiplayer.SendAuth(GameServerPeerId, Encoding.UTF8.GetBytes(_debugAuthData));
                Logger.LogNetwork($"Authenticating...");
            };
            Multiplayer.PeerAuthenticationFailed += (peerId) => { GD.PrintErr($"Authentication failed!"); };
            Multiplayer.SetAuthCallback(new Callable(this, MethodName.Client_AuthRequestHandle));
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
            Multiplayer.SetAuthCallback(new Callable(this, MethodName.Server_AuthRequestHandle));
            //Multiplayer.ServerRelay = false;
        }
    }

    private async Task<JwtValidationResult> ValidateJwtWithManger(string jwt)
    {
        try
        {
            var requestData = new JwtValidationRequest(jwt);
            var content = new StringContent(
                JsonSerializer.Serialize(requestData),
                Encoding.UTF8,
                "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_serverToken}");

            var response = await _httpClient.PostAsync(
                $"{_serverManagerUrl}/game-server/auth", content);

            if (!response.IsSuccessStatusCode)
                return JwtValidationResult.CreateInvalid($"Server validation failed: {response.StatusCode}");
            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<JwtValidationResult>(responseBody);
        }
        catch (Exception ex)
        {
            Logger.LogNetwork($"JWT validation error: {ex.Message}");
            return JwtValidationResult.CreateInvalid($"Validation error: {ex.Message}");
        }
    }

    private void Client_AuthRequestHandle(int id, byte[] data)
    {
        Multiplayer.CompleteAuth(id);
    }

    private async void Server_AuthRequestHandle(int id, byte[] data)
    {
        var jwt = Encoding.UTF8.GetString(data);
        Logger.LogNetwork($"Peer {id} authentication data: {jwt}.");

        var validationResult = await ValidateJwtWithManger(jwt);
        
        if (validationResult.Valid)
        {
            PeerAuthData[id] = validationResult;
            if (!MultiplayerController.LobbyExists(validationResult.LobbyId.ToString())) 
                MultiplayerController.CreateLobby(validationResult);
            
            Multiplayer.SendAuth(id, data);
            Multiplayer.CompleteAuth(id);
            Logger.LogNetwork($"Peer {id} authentication complete.");
        }
        else
        {
            Logger.LogNetwork($"Peer {id} authentication failed: {validationResult.ErrorMessage}");
            Multiplayer.DisconnectPeer(id);
        }
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

internal record JwtValidationRequest(string Jwt);

public record JwtValidationResult(
    bool Valid,
    string ErrorMessage,
    long? PlayerId,
    long? LobbyId,
    string PlayerName)
{
    public static JwtValidationResult CreateInvalid(string message)
    {
        return new JwtValidationResult(false, message, null, null, null);
    }
}

internal record ConfirmLobbyRequest(
    long LobbyId,
    long PlayerId);

internal record ConfirmLobbyResult(
    long LobbyId,
    int PlayerId,
    int MaxPlayers);