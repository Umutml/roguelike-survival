using System.Diagnostics;
using System.IO;
using GameCore.Spawner;
using TMPro;
using UnityEngine;
using VContainer;

namespace UI.Game
{
    public class PerformanceIndicator : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI fpsText;
        [SerializeField] private TextMeshProUGUI cpuMsText;
        [SerializeField] private TextMeshProUGUI enemiesText;
        [SerializeField] private TextMeshProUGUI memoryUsageText;
        
        private int frameCount;
        private float deltaTime;
        private float cpuTime;

        [Inject]
        private void Init(MobManager mobManager)
        {
            MobManager.MobCountChanged += MobCountChanged;
        }

        private void OnDestroy()
        {
            MobManager.MobCountChanged -= MobCountChanged;
        }

        private void MobCountChanged(int obj)
        {
            enemiesText.text = $"Active Enemy Count: {obj}";
        }


        private void Update()
        {
            frameCount++;
            deltaTime += Time.deltaTime;
            cpuTime += Time.unscaledDeltaTime;

            if (deltaTime > 1.0f)
            {
                float fps = frameCount / deltaTime;
                float cpuMs = (cpuTime / frameCount) * 1000.0f;

                fpsText.text = $"FPS: {fps:F2}";
                cpuMsText.text = $"CPU: {cpuMs:F2} ms";

                frameCount = 0;
                deltaTime = 0;
                cpuTime = 0;
            }

            if (frameCount % 300 == 0)
            {
                CheckMemoryUsage();
            }
        }

        private void CheckMemoryUsage()
        {
            long totalMemory = GetTotalSystemMemory();
            long usedMemory = GetUsedMemory();
            long availableMemory = totalMemory - usedMemory;

            float totalMemoryGB = totalMemory / (1024f * 1024f * 1024f);
            float usedMemoryGB = usedMemory / (1024f * 1024f * 1024f);
            float availableMemoryGB = availableMemory / (1024f * 1024f * 1024f);

            float usagePercentage = (float) usedMemory / totalMemory * 100f;

            memoryUsageText.text =
                $"Total Memory: {totalMemoryGB:F2} GB\nUsed Memory: {usedMemoryGB:F2} GB\nAvailable Memory: {availableMemoryGB:F2} GB\nMemory Usage: {usagePercentage:F2}%";
        }

        private long GetTotalSystemMemory()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        return GetTotalMemoryAndroid();
#elif UNITY_IOS && !UNITY_EDITOR
        return GetTotalMemoryIOS();
#else
            return SystemInfo.systemMemorySize * 1024L * 1024L;
#endif
        }

        private long GetUsedMemory()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        return GetUsedMemoryAndroid();
#elif UNITY_IOS && !UNITY_EDITOR
        return GetUsedMemoryIOS();
#else
            Process currentProcess = Process.GetCurrentProcess();
            currentProcess.Refresh();
            return currentProcess.WorkingSet64;
#endif
        }

#if UNITY_IOS && !UNITY_EDITOR
private long GetUsedMemoryIOS()
{
    // iOS doesn't provide direct access to memory usage
    // You might need to use Unity's profiling tools or estimate based on total memory
    return SystemInfo.systemMemorySize * 1024L * 1024L; // This is total memory, not used memory
}
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
private long GetUsedMemoryAndroid()
{
    string[] memInfo = File.ReadAllLines("/proc/meminfo");
    long totalMem = 0;
    long freeMem = 0;
    foreach (string line in memInfo)
    {
        if (line.StartsWith("MemTotal:"))
            totalMem = long.Parse(line.Split()[1]) * 1024; // Convert to bytes
        else if (line.StartsWith("MemFree:"))
            freeMem = long.Parse(line.Split()[1]) * 1024; // Convert to bytes
    }
    return totalMem - freeMem;
}
#endif


#if UNITY_ANDROID && !UNITY_EDITOR
    private long GetTotalMemoryAndroid()
    {
        string[] memInfo = File.ReadAllLines("/proc/meminfo");
        string memTotalLine = memInfo[0];
        string[] memTotalParts = memTotalLine.Split(':');
        string memTotalValue = memTotalParts[1].Trim().Split(' ')[0];
        return long.Parse(memTotalValue) * 1024; // Convert KB to bytes
    }
#endif

#if UNITY_IOS && !UNITY_EDITOR
    private long GetTotalMemoryIOS()
    {
        return SystemInfo.systemMemorySize * 1024L * 1024L; // Convert MB to bytes
    }
#endif
    }
}
