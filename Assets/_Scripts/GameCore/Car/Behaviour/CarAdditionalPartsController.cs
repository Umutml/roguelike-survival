using System;
using System.Collections.Generic;
using _Scripts.GameCore.Player;
using GameCore.Player;
using GameCore.Tutorial;
using Interfaces;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;

public class CarAdditionalPartsController : MonoBehaviour
{
    [SerializeField] private List<AdditionalPart> additionalParts = new List<AdditionalPart>();

    private PlayerCarController _playerCarController;
    private ITutorialService _tutorialService;

    public enum AdditionalPartType
    {
        None,
        Npc
    }

 

    public void SetPlayerCarController(PlayerCarController playerCarController, ITutorialService tutorialService)
    {
        _tutorialService = tutorialService;
        _playerCarController = playerCarController;
        _tutorialService.TutorialCompleted += OnTutorialComplete;
        
        if (_tutorialService.IsTutorialCompleted)
        {
            OnTutorialComplete();
        }
        else
        {
            OnTutorialStart();
        }
    }

    private void OnDestroy()
    {
        if (_tutorialService != null)
        {
            _tutorialService.TutorialCompleted -= OnTutorialComplete;
        }
    }

    public void OnTutorialComplete()
    {
        foreach (var part in additionalParts)
        {
            if (part.EnableOnTutorialComplete)
            {
                part.Part.SetActive(true);
            }
            else if (part.DisableOnTutorialComplete)
            {
                part.Part.SetActive(false);
            }
        }
    }

    public void OnTutorialStart()
    {
        foreach (var part in additionalParts)
        {
            if (part.EnableOnTutorialStart)
            {
                part.Part.SetActive(true);
            }
            else if (part.DisableOnTutorialStart)
            {
                part.Part.SetActive(false);
            }
        }
    }

    public bool IsPartActive(AdditionalPartType type)
    {
        var part = additionalParts.Find(x => x.Type == type);
        return part != null && part.IsPartActive;
    }

    public void EnableAdditionalPart(AdditionalPartType type)
    {
        var part = additionalParts.Find(x => x.Type == type);
        if (part != null)
        {
            part.Part.SetActive(true);
        }
    }

    public void DisableAdditionalPart(AdditionalPartType type)
    {
        var part = additionalParts.Find(x => x.Type == type);
        if (part != null)
        {
            part.Part.SetActive(false);
        }
    }

    public void EnableAllAdditionalParts()
    {
        foreach (var part in additionalParts)
        {
            part.Part.SetActive(true);
        }
    }

    public void DisableAllAdditionalParts()
    {
        foreach (var part in additionalParts)
        {
            part.Part.SetActive(false);
        }
    }
}

[Serializable]
public class AdditionalPart
{
    public CarAdditionalPartsController.AdditionalPartType Type;
    public GameObject Part;
    public bool IsPartActive => Part != null && Part.activeSelf;
    public bool EnableOnTutorialStart = false;
    public bool EnableOnTutorialComplete = false;
    public bool DisableOnTutorialStart = false;
    public bool DisableOnTutorialComplete = false;
}
