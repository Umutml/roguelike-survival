using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Utilities;
using System;
using MyBox;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "CharacterResources", menuName = "ScriptableObjects/CharacterResources", order = 1)]
    public class CharacterResources : ScriptableObject
    {
        [SerializeField] private Character character;
        [SerializeField] private string characterName;
        [SerializeField] private string modelAddressableKey;
        [SerializeField] private string characterSpacialSkill;
        [SerializeField] private string unlockDescription;
        [SerializeField] private int unlockWaveCount;
        [SerializeField] private int waveIndex;
        [SerializeField] private AssetReference characterArt;
        [SerializeField] private AssetReference characterGrayArt;
        [SerializeField] private AssetReference characterModel;
        [SerializeField] private AssetReference characterTopBarImage;
        [SerializeField] private Gradient characterGradient;
        [SerializeField] private Gradient characterCardGradient;
        [SerializeField] private CharacterSpawnTransform characterSpawnTransform;
        [SerializeField] private bool isLocked;
        [SerializeField] private bool usesCustomAnimator;
        [ConditionalField(nameof(usesCustomAnimator), false, true)][SerializeField] private RuntimeAnimatorController animatorController;
        
        public Character Character => character;
        public string CharacterName => characterName;
        public string CharacterSpacialSkill => characterSpacialSkill;
        public string CharacterUnlockDescription => unlockDescription;
        public int CharacterUnlockWaveCount => unlockWaveCount;
        public int WaveIndex => waveIndex;
        public Gradient CharacterGradient => characterGradient;
        public Gradient CharacterCardGradient => characterCardGradient;
        public CharacterSpawnTransform CharacterSpawnTransform => characterSpawnTransform;
        public async UniTask<Sprite> GetCharacterArt() => await AssetManager<Sprite>.LoadObject(characterArt);
        public async UniTask<Sprite> GetCharacterGrayArt() => await AssetManager<Sprite>.LoadObject(characterGrayArt);
        public async UniTask<GameObject> GetCharacterModel() => await AssetManager<GameObject>.LoadObject(characterModel);
        public async UniTask<Sprite> GetCharacterTopBar() => await AssetManager<Sprite>.LoadObject(characterTopBarImage);
        public string CharacterModelAddressableKey => modelAddressableKey;
        public bool IsLocked => isLocked;

        public AssetReference CharacterModel
        {
            get => characterModel;
            set => characterModel = value;
        }

        public AssetReference CharacterArt
        {
            get => characterArt;
            set => characterArt = value;
        }

        public AssetReference CharacterGrayArt
        {
            get => characterGrayArt;
            set => characterGrayArt = value;
        }

        public AssetReference CharacterTopBarImage
        {
            get => characterTopBarImage;
            set => characterTopBarImage = value;
        }

        public bool UsesCustomAnimator
        {
            get => usesCustomAnimator;
            set => usesCustomAnimator = value;
        }

        public RuntimeAnimatorController AnimatorController
        {
            get => animatorController;
            set => animatorController = value;
        }
    }
}


[Serializable]
public struct CharacterSpawnTransform
{
    [SerializeField] private Vector3 position;
    [SerializeField] private Vector3 scale;

    public Vector3 Position => position;
    public Vector3 Scale => scale;
}

