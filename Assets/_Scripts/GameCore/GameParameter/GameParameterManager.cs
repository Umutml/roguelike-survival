using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using VContainer;

public class GameParameterManager : MonoBehaviour
{
    private const string DevRemoteConfigUrl = "https://storage.googleapis.com/nosurrender-assets/zombie-drift-survival-racing/settings/GameParameters.json";
    private const string LiveRemoteConfigUrl = "https://cdn.nosurrenderheroes.xyz/zombie-drift-survival-racing/settings/GameParameters.json";

    private GameParameters GameParameters { set; get; }

    [Inject]
    private void Initialize() => InitializeRemoteConfig();

    private async void InitializeRemoteConfig()
    {
        var gameParametersTask = Addressables.LoadAssetAsync<GameParameters>("GameParameters").Task;
        await gameParametersTask;
        GameParameters = gameParametersTask.Result;
        var gameConfig = JsonUtility.FromJson<GameParameterConfig>(GameParameters.GetDefaultGameConfig());
        var baseUrl = GetBaseUrl();
        var versionedUrl = AddVersionToUrl(baseUrl);
        Debug.Log($"Loading GameParameters from: {versionedUrl}");
        var jsonData = await TryDownloadJson(versionedUrl);
        if (jsonData == null)
        {
            Debug.LogWarning("Falling back to base URL for GameParameters...");
            jsonData = await TryDownloadJson(baseUrl);
        }
        if (!string.IsNullOrEmpty(jsonData))
        {
            gameConfig = JsonUtility.FromJson<GameParameterConfig>(jsonData);
        }
        GameParameters.LoadParameter(gameConfig);
    }

    public ObjectiveParameters GetObjectiveParameters(string objectiveName)
    {
        return GameParameters.GameParameterConfig.objectives.FirstOrDefault(o => o.objectiveName == objectiveName);
    }
    private static string AddVersionToUrl(string url)
    {
        var newUrl = url[..^5];
        newUrl +=$"_v{Application.version}.json";
        return newUrl;
    }
    private static string GetBaseUrl()
    {
#if UNITY_EDITOR
        return DevRemoteConfigUrl;
#elif UNITY_IOS
        if (Application.isEditor)
            return DevRemoteConfigUrl;
        return LiveRemoteConfigUrl;
#elif UNITY_ANDROID
        if (Debug.isDebugBuild || string.IsNullOrEmpty(Application.installerName) || Application.installerName != "com.android.vending")
        {
            return DevRemoteConfigUrl;
        }
        return LiveRemoteConfigUrl;
#else
        return LiveRemoteConfigUrl;
#endif
    }
    private static async Task<string> TryDownloadJson(string url)
    {
        using var request = UnityWebRequest.Get(url);
        var operation = request.SendWebRequest();
        while (!operation.isDone)
            await Task.Yield();

        if (request.result == UnityWebRequest.Result.Success)
        {
            return request.downloadHandler.text;
        }
        Debug.LogWarning($"Request failed: {url}\nError: {request.error}");
        return null;
    }
}
