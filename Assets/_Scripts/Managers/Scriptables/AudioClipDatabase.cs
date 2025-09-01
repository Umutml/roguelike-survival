using System.Collections.Generic;
using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using Utilities;


[CreateAssetMenu(fileName = "AudioClipDatabase", menuName = "ScriptableObjects/AudioClipDatabase", order = 0)]
public class AudioClipDatabase : ScriptableObject
{
    #region Serializable Fields

    [Header("Musics")] [SerializeField] private GameMusics gameMusics;

    #endregion


    #region Properties

    public GameMusics GameMusics => gameMusics;

    #endregion


    [Serializable]
    public class AudioClipData
    {
        public string clipName;
        public AudioClip clip;
    }

    public List<AudioClipData> audioClips;
}


[Serializable]
public struct GameMusics
{
    [SerializeField] private AssetReference tutorialMusic;
    [SerializeField] private AssetReference freeRoamMusic;
    [SerializeField] private AssetReference waveMusic;


    public async UniTask<AudioClip> GetMusic(string musicType)
    {
        return musicType switch
        {
            "Tutorial" => await AssetManager<AudioClip>.LoadObject(tutorialMusic),
            "FreeRoam" => await AssetManager<AudioClip>.LoadObject(freeRoamMusic),
            "Wave" => await AssetManager<AudioClip>.LoadObject(waveMusic),
            _ => null
        };
    }
}