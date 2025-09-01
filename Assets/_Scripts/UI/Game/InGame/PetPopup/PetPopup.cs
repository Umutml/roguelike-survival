using System.Collections.Generic;
using System.Linq;
using GameCore.PopupSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PetPopup : Popup
{
    #region Serializable Fields

    [SerializeField] private List<Sprite> dogSprites = new();
    [SerializeField] private Image petImage;
    [SerializeField] private TMP_Text petNameText;

    #endregion


    #region Fields

    private readonly Dictionary<int, string> _petDictionary = new()
    {
        { 0, "Dobby" },
        { 1, "Wolfy" },
        { 2, "Bulldog" },
    };
    private int _currentPetIndex;

    #endregion


    #region Public Methods

    public override void OnOpenPopup()
    {
        
    }
    
    
    public void OnClickNextCharacter(string buttonType)
    {
        _currentPetIndex = (buttonType.Equals("Right"))
            ? (_currentPetIndex + 1) % dogSprites.Count
            : (_currentPetIndex - 1 + dogSprites.Count) %
              dogSprites.Count;
        petImage.sprite = dogSprites[_currentPetIndex];
        petNameText.text = _petDictionary[_currentPetIndex];
    }
    
    
    public void SelectCharacter(string petName)
    {
        var petIndex = _petDictionary.FirstOrDefault(x => x.Value == petName).Key;
            
        if (petIndex == _currentPetIndex) return;

        _currentPetIndex = petIndex;
        petImage.sprite = dogSprites[_currentPetIndex];
        petNameText.text = _petDictionary[_currentPetIndex];
    }

    #endregion
}
