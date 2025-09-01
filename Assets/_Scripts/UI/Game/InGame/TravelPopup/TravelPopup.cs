using GameCore.Player;
using GameCore.PopupSystem;
using Interfaces;
using VContainer;

public class TravelPopup : Popup
{
    private IGameService _gameService;
    public override void OnOpenPopup()
    {
        _gameService = Resolver.Resolve<IGameService>();
    }


    public void TravelToBase()
    {
        _gameService.RestartLevel();
    }
}
