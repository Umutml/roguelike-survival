using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class ObjectiveIndicatorBar : MonoBehaviour
{
    [SerializeField] private GameObject progressBarObject,milestoneParent;
    [SerializeField] private Image progressBar;
    public async void Init(AssetReferenceGameObject[] milestonePrefabs,int[] milestoneProgress)
    {            
        progressBar.fillAmount = 0;
        progressBarObject.SetActive(true);
        var milestoneParentWidth = milestoneParent.GetComponent<RectTransform>().rect.width/2;
        var milestoneParentHeight = milestoneParent.GetComponent<RectTransform>().rect.height;
        for (var i = 0; i < milestonePrefabs.Length; i++)
        {
            var milestonePrefab = milestonePrefabs[i];
            var milestoneObject = await ObjectManager.GetObject(milestonePrefab);
            milestoneObject.SetActive(true);
            milestoneObject.transform.SetParent(milestoneParent.transform);
            var milestoneObjectXPosition = _Utilities.Helper.Remap(milestoneProgress[i], 0, 100, -milestoneParentWidth, milestoneParentWidth);
            milestoneObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(milestoneObjectXPosition, -milestoneParentHeight);
        }
    }
    public void UpdateProgress(int progress)
    {
        var targetFill = progress / 100f;
        DOTween.To(() => progressBar.fillAmount, x => progressBar.fillAmount = x, targetFill, 1).SetEase(Ease.OutCubic);
    }
    public void CloseBar()
    {
        foreach (Transform milestoneObject in milestoneParent.transform)
            milestoneObject.gameObject.SetActive(false);
        progressBarObject.SetActive(false);
        progressBar.fillAmount = 0;
    }
}
