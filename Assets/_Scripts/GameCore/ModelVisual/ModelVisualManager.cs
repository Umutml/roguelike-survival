using System.Linq;
using Cysharp.Threading.Tasks;
using GameCore.Scriptables;
using UnityEngine;
using Utilities;


public class ModelVisualManager : MonoBehaviour
{
    #region Private Methods

    [SerializeField] private Camera modelRenderCamera;
    [SerializeField] private Transform modelVisualContent;

    #endregion


    #region Fields

    private RenderTexture _renderTexture;
    private GameObject _currentModel;

    #endregion


    #region Properties

    public Transform ModelVisualContent
    {
        get => modelVisualContent;
        set => modelVisualContent = value;
    }

    public RenderTexture RenderTexture => _renderTexture;
    public GameObject CurrentModel => _currentModel;

    #endregion


    #region Public Methods

    public void SetupModelVisual(GameObject model)
    {
        if (_currentModel != null)
        {
            Destroy(_currentModel);
        }


        _currentModel = Instantiate(model, modelVisualContent);
        _currentModel.transform.rotation = Quaternion.Euler(0, 180, 0);
        SetupRenderTexture();
    }


    public void SetupModelVisual(GameObject model, Vector3 position, Vector3 localScale)
    {
        if (_currentModel != null)
        {
            Destroy(_currentModel);
        }


        _currentModel = Instantiate(model, modelVisualContent);
        _currentModel.transform.localPosition = position;
        _currentModel.transform.rotation = Quaternion.Euler(0, 180, 0);
        _currentModel.transform.localScale = localScale;
        SetupRenderTexture();
    }


    public async void SetupModelVisual(GameObject model, CharacterResources character)
    {
        if (_currentModel != null)
        {
            Destroy(_currentModel);
        }

        _currentModel = Instantiate(model, modelVisualContent);
        var animator = _currentModel.GetComponent<Animator>();

        if (character.UsesCustomAnimator)
        {
            animator.runtimeAnimatorController = character.AnimatorController;
            
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
    
            animator.Rebind();
            animator.Update(0f);
    
            animator.enabled = true;
            animator.speed = 1f;
            
            animator.Play(0, 0, 0f);
        }
        
        SetTrigger(animator, character.CharacterName);
        
        _currentModel.transform.localPosition = character.CharacterSpawnTransform.Position;
        _currentModel.transform.localScale = character.CharacterSpawnTransform.Scale;
        _currentModel.transform.rotation = Quaternion.Euler(0, 180, 0);
        SetupRenderTexture();
    }
    
   


    public void ReleaseCarRenderTexture()
    {
        if (_renderTexture != null)
        {
            _renderTexture.Release();
            _renderTexture = null;
        }

        modelRenderCamera.targetTexture = null;
        modelRenderCamera.gameObject.SetActive(false);
    }

    #endregion


    #region Private Methods

    private void SetupRenderTexture()
    {
        if (modelRenderCamera == null)
        {
            Debug.LogError("Model Render Camera is null");
            return;
        }

        if (_renderTexture == null)
        {
            _renderTexture = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
            _renderTexture.useMipMap = false;
            _renderTexture.filterMode = FilterMode.Bilinear;
        }

        modelRenderCamera.targetTexture = _renderTexture;
        modelRenderCamera.gameObject.SetActive(true);
    }
    
    private void SetTrigger(Animator animator, string triggerName)
    {
        animator.enabled = true;
        animator.speed = 1f;
        animator.Update(0f);
        
        if (!animator || animator.runtimeAnimatorController == null)
            return;
        
        if (animator.parameters.Any(p => p.name == triggerName && p.type == AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger(triggerName);
        }
        else
        {
            Debug.LogWarning($"Trigger parameter '{triggerName}' not found in animator controller.");
        }
    }

    #endregion
}
