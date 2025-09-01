using TMPro;
using UnityEngine;

public class ObjectiveIndicatorWave : MonoBehaviour
{
    [SerializeField] private TMP_Text waveText, waveCountText;
    [SerializeField] private GameObject waveArea;
    public void SetWareAreaActive(bool active)
    {
        waveArea.SetActive(active);
    }
    public void UpdateWaveProgress(int? activeWaveCount, int? totalWaveCount, int? remainingWaveCount)
    {
        waveCountText.text = remainingWaveCount ==null ? "-" : $"{remainingWaveCount}";
        if(activeWaveCount == null || totalWaveCount == null) return;
        waveText.text = $"WAVE {activeWaveCount}/{totalWaveCount}";
    }
}
