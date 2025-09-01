using UnityEngine;
using UnityEngine.AddressableAssets;
using static ObjectiveStructure.UIIndicator;

public class ObjectiveIndicatorManager : MonoBehaviour
{
    [SerializeField] private ObjectiveIndicatorText timerIndicator,textIndicator;
    [SerializeField] private ObjectiveIndicatorBar progressBar;
    [SerializeField] private ObjectiveIndicatorWave waveIndicator;
    public void ObjectiveStart(AssetReferenceGameObject[] milestoneObjects, int[] milestoneProgress)
    {
        progressBar?.Init(milestoneObjects, milestoneProgress);
    }
    public void UpdateObjectiveStatus(string status,Color backgroundColor, float delay, IndicatorType indicatorType)
    {
        switch (indicatorType)
        {
            case IndicatorType.Time:
                timerIndicator.ShowText(status,backgroundColor, delay);
                break;
            case IndicatorType.Text:
                textIndicator.ShowText(status,backgroundColor, delay);
                break;
            default:
                goto case IndicatorType.Text;
        }
    }
    public void UpdateObjectiveProgress(int progress)=> progressBar?.UpdateProgress(progress);
    public void ObjectiveFinish()
    {
        waveIndicator?.SetWareAreaActive(false);
        timerIndicator?.CloseText();
        progressBar?.CloseBar();
    }
    public void WaveStarted(bool waveIndicatorActive) => waveIndicator?.SetWareAreaActive(waveIndicatorActive);
    public void WaveUpdate(int? activeWaveCount, int? totalWaveCount, int? remainingWaveCount) => waveIndicator?.UpdateWaveProgress(activeWaveCount, totalWaveCount, remainingWaveCount);
}
