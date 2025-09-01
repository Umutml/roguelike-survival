using System.Collections.Generic;
using GameCore.Scriptables;
using UnityEngine;

public class AllAreaNpcController : MonoBehaviour
{
    #region Serializable Fields

    [SerializeField] private AreaResources areaResources;
    [SerializeField] private List<AreaBaseNpc> areaNpcList = new();

    #endregion


    #region Unity Methods

    private void OnEnable()
    {
        InitializeAreaNpc();
    }

    #endregion


    #region Private Methods

    private void InitializeAreaNpc()
    {
        foreach (var areaNpc in areaNpcList)
        {
            var area = areaResources.GetArea(areaNpc.AreaNpcType);
            areaNpc.InitializeArea(area);
        }
    }

    #endregion
}
