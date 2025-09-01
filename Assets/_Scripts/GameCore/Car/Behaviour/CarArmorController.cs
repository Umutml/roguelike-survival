using System;
using System.Collections.Generic;
using GameCore.Player;
using UnityEngine;

public class CarArmorController : MonoBehaviour
{
    [SerializeField] private List<GameObject> _armorObjects = new();
    [SerializeField] private bool isTutorialCar;


    public void OpenArmorObjects()
    {
        if (_armorObjects is not { Count: > 0 }) { return; }

        if (isTutorialCar)
        {
            for (var i = 0; i < _armorObjects.Count; i++)
            {
                _armorObjects[i].SetActive(true);
            }
        }
    }
}