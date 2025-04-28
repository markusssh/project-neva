using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ProjectNeva.Main.NetworkingArchitecture.GamePhases.AbstractPhase;
using ProjectNeva.Main.Utils.Logger;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public class DrawingPhase : ClosedGamePhase
{
    public DrawingPhase(LobbyManager lobbyManager) : base(lobbyManager)
    {
        _images = Lobby.Players.Keys.ToDictionary(key => key, _ => Array.Empty<byte>());
        _finishedEarly = Lobby.Players.Keys.ToDictionary(key => key, _ => false);
        _timer.OneShot = true;
        _timer.Timeout += OnTimerTimeout;
    }

    private readonly Dictionary<long, bool> _finishedEarly;
    private readonly Dictionary<long, byte[]> _images;
    private bool EveryoneFinishedEarly => _finishedEarly.All(kv => kv.Value);
    private bool EveryoneReady => _images.Values.All(image => image.Length > 0);
    private readonly Timer _timer = new();

    public override void Enter()
    {
        base.Enter();
        Lobby.PlayerDrawingStateChanged += OnPlayerDrawingStateChanged;
        
        MultiplayerController.Instance.Server_BroadcastLobby(
            Lobby,
            MultiplayerController.MethodName.Client_ShootDrawingScene);

        MultiplayerController.Instance.AddChild(_timer);
        _timer.Start(Lobby.PlayTime);
        
        Logger.LogNetwork($"Lobby: {Lobby.LobbyId}. Drawing scene started.");
    }

    private void OnPlayerDrawingStateChanged(long playerId, bool drawingOn)
    {
        _finishedEarly[playerId] = !drawingOn;
        /*const string pref = "un-";
        var message = $"Lobby: {Lobby.LobbyId}. Player {playerId} has ";
        message = drawingOn ? message + pref : message;
        message += "finished drawing.";
        Logger.LogNetwork(message);*/

        if (!EveryoneFinishedEarly) return;
        Logger.LogNetwork($"Lobby {Lobby.LobbyId} has finished drawing. Collecting data.");
        OnTimerTimeout();
    }

    private void OnTimerTimeout()
    {
        _timer.Timeout -= OnTimerTimeout;
        Lobby.PlayerSentFinalImage += OnPlayerSentFinalImage;
        MultiplayerController.Instance.Server_BroadcastLobby(
            Lobby,
            MultiplayerController.MethodName.Client_CollectFinalImage);
    }

    private void OnPlayerSentFinalImage(long playerId, byte[] data)
    {
        _images[playerId] = data;
        Update();
    }

    protected override void HandlePlayerDisconnect(long playerId)
    {
        base.HandlePlayerDisconnect(playerId);
        _images.Remove(playerId);
        _finishedEarly.Remove(playerId);
        Update();
    }

    private void Update()
    {
        if (EveryoneReady) LobbyManager.TransitionTo(LobbyState.LoadingRating);
    }

    public override void Exit()
    {
        foreach (var kvp in _images)
        {
            Lobby.Images[kvp.Key] = kvp.Value;
        }
        
        MultiplayerController.Instance.Server_BroadcastLobby(
            Lobby,
            MultiplayerController.MethodName.Client_ReceiveFinalImages,
            Lobby.Images
        );
        
        base.Exit();
        Lobby.PlayerSentFinalImage -= OnPlayerSentFinalImage;
        Lobby.PlayerDrawingStateChanged -= OnPlayerDrawingStateChanged;
        
        _timer.QueueFree();
    }
}