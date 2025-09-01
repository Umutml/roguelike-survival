using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using UnityEngine.ResourceManagement.AsyncOperations;
using Utilities;

namespace UI
{
    public class RandomImageSetter : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private AssetReferenceSprite[] spriteReferences;
        
        private AsyncOperationHandle<Sprite> _currentSpriteHandle;

        private void OnEnable()
        {
            LoadSprite();
        }

        private void OnDisable()
        {
            ReleaseSprite();
        }
        
        private void LoadSprite() 
        {
            _currentSpriteHandle = Addressables.LoadAssetAsync<Sprite>(spriteReferences.PickRandom());
            _currentSpriteHandle.Completed += handle =>
            {
                image.sprite = handle.Result;
            };
        }

        private void ReleaseSprite() 
        {
            if (_currentSpriteHandle.IsValid())
            {
                image.sprite = null;
                Addressables.Release(_currentSpriteHandle);
            }
        }
        
    }
}
