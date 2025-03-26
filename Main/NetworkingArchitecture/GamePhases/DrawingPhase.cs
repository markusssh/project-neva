using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public class DrawingPhase : ClosedGamePhase
{
    public DrawingPhase(LobbyManager lobbyManager) : base(lobbyManager)
    {
        _images = Lobby.Players.Keys.ToDictionary(key => key, _ => Array.Empty<byte>());
        _finishedEarly = Lobby.Players.Keys.ToDictionary(key => key, _ => false);
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
        _timer.OneShot = true;
        _timer.Start(Lobby.DrawingTimeSec);
        _timer.Timeout += OnTimerTimeout;
    }

    private void OnPlayerDrawingStateChanged(long playerId, bool drawingOn)
    {
        _finishedEarly[playerId] = drawingOn;
        if (drawingOn && EveryoneFinishedEarly)
        {
            OnTimerTimeout();
        }
    }

    private void OnTimerTimeout()
    {
        _timer.Timeout -= OnTimerTimeout;
        _timer.QueueFree();
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
        Update();
    }

    private void Update()
    {
        if (EveryoneReady) LobbyManager.TransitionTo(LobbyState.LoadingRating);
    }

    public override void Exit()
    {
        base.Exit();
        Lobby.PlayerSentFinalImage -= OnPlayerSentFinalImage;
        Lobby.PlayerDrawingStateChanged -= OnPlayerDrawingStateChanged;
        
        foreach (var kvp in _images)
        {
            Lobby.Images[kvp.Key] = kvp.Value;
        }
    }
}