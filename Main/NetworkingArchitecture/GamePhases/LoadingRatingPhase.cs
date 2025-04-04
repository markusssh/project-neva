using ProjectNeva.Main.NetworkingArchitecture.GamePhases.AbstractPhase;

namespace ProjectNeva.Main.NetworkingArchitecture.GamePhases;

public class LoadingRatingPhase : LoadingGamePhase
{
    public LoadingRatingPhase(LobbyManager lobbyManager) : base(
        lobbyManager, MultiplayerController.MethodName.Client_LoadRatingScene)
    {
    }

    protected override void OnEveryoneReady()
    {
        LobbyManager.TransitionTo(LobbyState.Rating);
    }
}