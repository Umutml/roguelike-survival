using GameCore.Scriptables;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class BadgeSegment : MonoBehaviour
{
    [SerializeField] private TMP_Text badgeText;
    [SerializeField] private Image badgeImage;
    [SerializeField] private Button badgeButton; 
    
    
    #region Public Methods

    public async void InitializeBadge(Badge badge, UnityAction openPopup)
    {
        badgeButton.onClick.RemoveAllListeners();
        var badgeArt = await badge.BadgeArt();
        
        badgeText.text = badge.BadgeName;
        badgeImage.sprite = badgeArt;
        badgeButton.onClick.AddListener(openPopup);
    }

    #endregion
}
