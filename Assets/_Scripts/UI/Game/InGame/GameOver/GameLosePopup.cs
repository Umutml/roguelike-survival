using System.Collections.Generic;
using GameCore.PopupSystem;
using Interfaces;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class GameLosePopup : Popup
{
    #region Serializable Fields

    [SerializeField] private List<Sprite> loseSprites = new();
    [SerializeField] private Image loseImage;

    private IGameService _gameService;
    private CarManager _carManager;

    #endregion


    #region Public Methods

    public override void OnOpenPopup()
    {
        _gameService = Resolver.Resolve<IGameService>();
        _carManager = Resolver.Resolve<CarManager>();
        SetRandomLoseImage();
    }

    #endregion


    #region Private Methods

    private void SetRandomLoseImage()
    {
        loseImage.sprite = loseSprites[Random.Range(0, loseSprites.Count)];
    }


    public void OnClickRestartButton()
    {
        _carManager.Restart();
        _gameService.RestartLevel();
    }

    #endregion
}
