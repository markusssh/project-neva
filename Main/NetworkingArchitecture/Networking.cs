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
    
    private static readonly string ServerManagerUrl = Environment.GetEnvironmentVariable("SERVER_MANAGER_URL") ?? "http://localhost:8080";
    private static readonly string ServerToken = Environment.GetEnvironmentVariable("SERVER_TOKEN") 
                                                  ?? "4791da355540b0cd33420b55c066802dd09998d8a2c5a44667ee4ccc5a625e25"; // dev

    private static string _authToken;
    public static long ClientId { get; private set; }

    private static readonly HttpClient HttpClient = new();
    public static Networking Instance { get; private set; } = null!;

    public const int GameServerPeerId = 1;
    public const int ServerMaxConnections = 3000;

    private static string _gameServerIp;
    private static int _gameServerPort = 8081;

    public static void SetGameServerUrl(string gameServerIp, int gameServerPort)
    {
        _gameServerIp = gameServerIp;
        _gameServerPort = gameServerPort;
    }

    public bool IsServer { get; set; }
    public bool IsClient => !IsServer;

    public readonly Dictionary<long, JwtValidationResult> PeerAuthData = new();

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
        // Подключение обработчиков для клиентской сборки
        if (IsClient)
        {
            Multiplayer.PeerConnected += (id) =>
            {
                if (id != 1)
                {
                    Logger.LogNetwork($"Peer {id} connected to server.");
                }
            };
            Multiplayer.PeerDisconnected += id => { Logger.LogNetwork($"Peer {id} disconnected from server."); };
            Multiplayer.ConnectionFailed += () => { Logger.LogNetwork("Connection failed!"); };
            Multiplayer.ConnectedToServer += () =>
            {
                Logger.LogNetwork("Connection successful.");
            };
            Multiplayer.ServerDisconnected += () =>
            {
                Logger.LogNetwork("Disconnected from game server.");
                ClientId = 0;
            };
            Multiplayer.PeerAuthenticating += peerId =>
            {
                if (peerId != GameServerPeerId) return;
                Multiplayer.SendAuth(GameServerPeerId, Encoding.UTF8.GetBytes(_authToken));
                Logger.LogNetwork("Authenticating...");
                ClientId = peerId;
            };
            Multiplayer.PeerAuthenticationFailed += (id) =>
            {
                Logger.LogNetwork("Authentication failed!");
            };
            Multiplayer.SetAuthCallback(new Callable(this, MethodName.Client_AuthRequestHandle));
        }
        // Подключение обработчиков для серверной сборки
        else
        {
            Multiplayer.PeerConnected += (id) =>
            {
                Logger.LogNetwork($"Peer {id} connected to server.");
                MultiplayerController.Instance.Server_OnPeerConnected(id);
            };
            Multiplayer.PeerDisconnected += Server_OnPeerDisconnected;
            Multiplayer.PeerAuthenticating += (id) => { Logger.LogNetwork($"Peer {id} authenticating..."); };
            Multiplayer.PeerAuthenticationFailed += (_) => {}; // неудачная ав-ция лоигруется ниже
            Multiplayer.SetAuthCallback(new Callable(this, MethodName.Server_AuthRequestHandle));
            //Multiplayer.ServerRelay = false;
        }
    }

    private static async Task<JwtValidationResult> ValidateJwtWithManger(string jwt)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            
            var requestData = new JwtValidationRequest(jwt);
            var content = new StringContent(
                JsonSerializer.Serialize(requestData, options),
                Encoding.UTF8,
                "application/json");
            
            HttpClient.DefaultRequestHeaders.Clear();
            HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ServerToken}");

            var response = await HttpClient.PostAsync(
                $"{ServerManagerUrl}/game-server/auth", content);
            
            if (!response.IsSuccessStatusCode)
                return JwtValidationResult.CreateInvalid($"Server validation failed: {response.StatusCode}");
            
            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<JwtValidationResult>(responseBody, options);
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
        try
        {
            var jwt = Encoding.UTF8.GetString(data);
            Logger.LogNetwork($"Peer {id} authentication data: {jwt.Left(3)}***");

            var validationResult = await ValidateJwtWithManger(jwt);
            if (!validationResult.Valid) throw new Exception("Jwt not valid!");
            
            if (!MultiplayerController.LobbyExists(validationResult.LobbyId.ToString()))
            {
                if (validationResult.LobbyId == null || validationResult.PlayerId == null)
                    throw new Exception($"Lobby {validationResult.LobbyId} not found.");

                var requestData = new ConfirmLobbyRequest(
                    validationResult.LobbyId.Value, 
                    validationResult.PlayerId.Value);
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                };
                var content = new StringContent(
                    JsonSerializer.Serialize(requestData, options),
                    Encoding.UTF8,
                    "application/json");
                
                HttpClient.DefaultRequestHeaders.Clear();
                HttpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ServerToken}");
                
                var response = await HttpClient.PostAsync(
                    $"{ServerManagerUrl}/game-server/confirm-lobby", content);
                
                var responseBody = await response.Content.ReadAsStringAsync();
                var confirmResult = JsonSerializer.Deserialize<ConfirmLobbyResult>(responseBody, options);

                MultiplayerController.CreateLobby(
                    confirmResult.LobbyId.ToString(), 
                    confirmResult.CreatorId,
                    confirmResult.PlayTime,
                    confirmResult.MaxPlayers);
            }

            Multiplayer.SendAuth(id, data);
            Multiplayer.CompleteAuth(id);
            Logger.LogNetwork($"Peer {id} authentication complete.");

            PeerAuthData[id] = validationResult;
        }
        catch (Exception ex)
        {
            Logger.LogNetwork($"Peer {id} authentication failed: {ex.Message}");
            Multiplayer.DisconnectPeer(id);
        }
    }

    private void Server_OnPeerDisconnected(long id)
    {
        Logger.LogNetwork($"Peer {id} disconnected from server.");
        MultiplayerController.Instance.Server_OnPeerDisconnected(id);
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

    public Error JoinGame(string authToken)
    {
        _authToken = authToken;
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
        //Multiplayer.SendAuth(GameServerPeerId, Encoding.UTF8.GetBytes(authToken));

        return Error.Ok;
    }

    public void LeaveGame()
    {
        Logger.LogNetwork("Leaving lobby...");
        MultiplayerController.Instance.Client_Clear();
        Multiplayer.MultiplayerPeer.Close();
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
    long CreatorId,
    int PlayTime,
    int MaxPlayers);