using System;
using System.Collections.Generic;
using _Scripts.Interfaces;
using _Scripts.UI.Architectural;
using _Scripts.Utilities;
using _Utilities;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using VContainer;

namespace UI.Game.Architectural
{
    public class Content : MonoBehaviour, IContent
    {
        public bool validate;
        [HideInInspector][SerializeField] private protected GameObject[] gameObjects;
        [HideInInspector][SerializeField] private protected Image[] uiImages;
        [HideInInspector][SerializeField] private protected CanvasGroup[] canvasGroups;
        [HideInInspector][SerializeField] private protected TMP_Text[] texts;
        [HideInInspector][SerializeField] private protected Slider[] sliders;
        [HideInInspector][SerializeField] private protected Text[] legacyTexts;
        [HideInInspector][SerializeField] private Button[] buttons;
        [HideInInspector][SerializeField] private Animator[] animators;
        [HideInInspector][SerializeField] private Toggle[] toggles;
        [HideInInspector][SerializeField] private TMP_InputField[] tmpInputFields;

        private IGeneralOnClickManager _generalOnClickManager;
        private bool dependenciesInjected = false;
#if UNITY_EDITOR
        private void OnValidate()
        {
            gameObjects = CollectAllChildObjects(gameObject).ToArray();
            var newUIImages = new List<Image>();
            var newCanvasGroups = new List<CanvasGroup>();
            var newTexts = new List<TMP_Text>();
            var newsSliders = new List<Slider>();
            var newLegacyTexts = new List<Text>();
            var newButtons = new List<Button>();
            var newAnimators = new List<Animator>();
            var newToggles = new List<Toggle>();
            var newSliders = new List<Slider>();
            var newTmpInputFields = new List<TMP_InputField>();
            foreach (var go in gameObjects)
            {
                if (go.TryGetComponent(out Image newImage))
                {
                    newUIImages.Add(newImage);
                }

                if (go.TryGetComponent(out CanvasGroup newCanvasGroup))
                {
                    newCanvasGroups.Add(newCanvasGroup);
                }

                if (go.TryGetComponent(out TMP_Text newText))
                {
                    newTexts.Add(newText);
                }

                if (go.TryGetComponent(out Slider newSlider))
                {
                    newsSliders.Add(newSlider);
                }

                if (go.TryGetComponent(out Button newButton))
                {
                    newButtons.Add(newButton);
                }

                if (go.TryGetComponent(out Toggle newToggle))
                {
                    newToggles.Add(newToggle);
                }

                if (go.TryGetComponent(out Animator newAnimator))
                {
                    newAnimators.Add(newAnimator);
                }


                if (go.TryGetComponent(out Text newLegacyText))
                {
                    newLegacyTexts.Add(newLegacyText);
                }

                if (go.TryGetComponent(out Slider slider))
                {
                    newSliders.Add(slider);
                }

                if (go.TryGetComponent(out TMP_InputField inputField))
                {
                    newTmpInputFields.Add(inputField);
                }
            }

            uiImages = newUIImages.ToArray();
            canvasGroups = newCanvasGroups.ToArray();
            texts = newTexts.ToArray();
            sliders = newsSliders.ToArray();
            legacyTexts = newLegacyTexts.ToArray();
            buttons = newButtons.ToArray();
            animators = newAnimators.ToArray();
            toggles = newToggles.ToArray();
            tmpInputFields = newTmpInputFields.ToArray();
        }

        List<GameObject> CollectAllChildObjects(GameObject parent)
        {
            List<GameObject> newList = new List<GameObject>();
            foreach (Transform childTransform in parent.transform)
            {
                GameObject childObject = childTransform.gameObject;
                StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(childObject);
                if (flags == StaticEditorFlags.BatchingStatic) continue;
                newList.Add(childObject);
                if (childObject.transform.childCount > 0)
                    newList.AddRange(CollectAllChildObjects(childObject));
            }

            return newList;
        }
#endif

        [Inject]
        public void Init(IGeneralOnClickManager generalOnClickManager)
        {
            _generalOnClickManager = generalOnClickManager;

            dependenciesInjected = true;
        }

        public void ForceCulling()
        {
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(true);
        }

        public void SetCulling()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(CheckRendering());
            }
        }

        private bool CheckRendering()
        {
            var yPos = transform.position.y;
            return yPos > 0 && yPos < Screen.height;
        }

        public void OnClickListen(Action callBack)
        {
            if (!TryGetComponent(out Button mainButton)) return;
            //mainButton.onClick.AddListener(() => { callBack?.Invoke(); });
            //_generalOnClickManager.RegisterButton(mainButton, callBack);
        }

        public void OnClickListen(string buttonName, Action callBack, IObjectResolver resolver = null)
        {
            var findButton = GetButton(buttonName);
            if (findButton == null) return;
            if (resolver != null)
            {
                _generalOnClickManager = resolver.Resolve<IGeneralOnClickManager>();
                ButtonEvent result;
                var buttonEvent = Enum.TryParse(buttonName, out result) ? result : ButtonEvent.None;

                if (_generalOnClickManager != null)
                {
                    _generalOnClickManager.RegisterButton(findButton, callBack, buttonEvent);
                }
            }
            else
            {
                findButton.onClick.AddListener(() => { callBack?.Invoke(); });
            }
        }

        public void OnClickListen(ref Action<string> callBack, string contentId)
        {
            if (!TryGetComponent(out Button mainButton)) return;
            var onClickAction = callBack;
            //mainButton.onClick.AddListener(() => { onClickAction?.Invoke(contentId); });
            //_generalOnClickManager.RegisterButton(mainButton, () => onClickAction?.Invoke(contentId));
        }

        public void OnClickListen(string buttonName, ref Action<string> callBack, string contentId)
        {
            var onClickAction = callBack;
            var findButton = GetButton(buttonName);
            if (findButton == null) return;
            //GetButton(buttonName).onClick.AddListener(() => { onClickAction?.Invoke(contentId); });
            //_generalOnClickManager.RegisterButton(findButton, () => onClickAction?.Invoke(contentId));
        }

        public void OnValueChangedListen(string sliderName, UnityAction<float> callBack)
        {
            var findSlider = GetSlider(sliderName);
            if (findSlider == null) return;
            findSlider.onValueChanged.AddListener(callBack);
        }

        protected void RemoveAllListener()
        {
            foreach (var button in buttons)
            {
                button.onClick.RemoveAllListeners();
            }

            if (!TryGetComponent(out Button mainButton)) return;
            mainButton.onClick.RemoveAllListeners();
        }

        protected virtual void OnDestroy()
        {
            RemoveAllListener();
        }

        #region Set References

        protected void SetMaterial(string targetName, Material targetMaterial)
        {
            GetImage(targetName).material = targetMaterial;
        }

        public void SetImage(string targetName, Sprite targetSprite)
        {
            if (targetSprite == null) return;
            Image targetImage = GetImage(targetName);
            if (targetImage == null) return;
            targetImage.sprite = targetSprite;
        }


        protected void SetColor(string targetName, Color targetColor)
        {
            foreach (var gameObject in gameObjects)
            {
                if (gameObject.name != targetName) continue;
                if (gameObject.TryGetComponent(out TMP_Text textMeshPro))
                {
                    textMeshPro.color = targetColor;
                }

                if (gameObject.TryGetComponent(out Image image))
                {
                    image.color = targetColor;
                }

                return;
            }
        }

        protected void SetGameObject(string targetName, bool active)
        {
            var findObject = GetGameObject(targetName);
            if (findObject)
                findObject.SetActive(active);
        }

        public void SetText(string targetName, string targetText)
        {
            TMP_Text findText = GetText(targetName);
            if (findText == null)
            {
                Text findLegacyText = GetLegacyText(targetName);
                if (findLegacyText == null) return;
                findLegacyText.text = targetText;
                return;
            }

            findText.text = targetText;
        }

        public void AppendText(string targetName, string additionalText)
        {
            var findText = GetText(targetName);
            if (findText != null)
            {
                findText.text += additionalText;
                return;
            }

            var findLegacyText = GetLegacyText(targetName);
            if (findLegacyText != null)
            {
                findLegacyText.text += additionalText;
            }
        }

        public void SetSlider(string targetName, float value)
        {
            var findSlider = GetSlider(targetName);
            if (findSlider != null)
            {
                findSlider.value = value;
            }
        }
        public void SetFill(string targetName, float value, float time = 0)
        {
            var findSlider = GetImage(targetName);
            if (findSlider == null) return;
            if (time > 0.1f)
                DOTween.To(() => findSlider.fillAmount, x => findSlider.fillAmount = x, value, time).SetEase(Ease.OutCubic);
            else
                findSlider.fillAmount = value;
        }

        public void SetSlider(string targetName, float value, float maxValue)
        {
            var findSlider = GetSlider(targetName);
            if (findSlider == null) return;
            findSlider.value = Helper.Remap(value, 0, maxValue, 0, 1);
        }


        public void SetGameObjectScale(string targetName, Vector3 targetSize)
        {
            var findObject = GetGameObject(targetName);
            if (findObject) findObject.GetComponent<RectTransform>().localScale = targetSize;
        }


        // protected void SetLocalizeKey(string targetName, string targetText)
        // {
        //     var targetGameObject = GetLocalizeStringEvent(targetName);
        //     if (targetGameObject == null)
        //     {
        //         SetText(targetName, targetText);
        //         return;
        //     }
        //
        //     targetGameObject.SetEntry(targetText);
        // }

        /*
        protected void SetLocalizeVariable(string targetName, string targetText)
        {
            var targetGameObject = GetLocalizeStringEvent(targetName);
            if (targetGameObject == null)
                return;
            var variableString = targetText.Split('-');
            ((StringVariable)targetGameObject.StringReference[variableString[0]]).Value = variableString[1];
        }
        */

        protected void SetAnimator(int parameterName, bool isActive)
        {
            Animator animator = GetAnimator();
            if (!AnimatorIsReady(animator)) return;
            animator.SetBool(parameterName, isActive);
        }

        protected void SetAnimator(int parameterName, float parameterFloat)
        {
            Animator animator = GetAnimator();
            if (!AnimatorIsReady(animator)) return;
            animator.SetFloat(parameterName, parameterFloat);
        }

        protected void SetAnimator(int parameterName, int parameterInt)
        {
            Animator animator = GetAnimator();
            if (!AnimatorIsReady(animator)) return;
            animator.SetInteger(parameterName, parameterInt);
        }

        /*
        protected void SetAnimator(int parameterName)
        {
            Animator animator = GetAnimator();
            if (!AnimatorIsReady(animator) || !animator.IsPlayingAnimatorController()) return;
            animator.SetTrigger(parameterName);
        }
        */

        protected void SetAnimator(string targetName, int parameterName, bool isActive)
        {
            GetAnimator(targetName).SetBool(parameterName, isActive);
        }

        protected void SetAnimator(string targetName, int parameterName, float parameterFloat)
        {
            GetAnimator(targetName).SetFloat(parameterName, parameterFloat);
        }

        protected void SetAnimator(string targetName, int parameterName, int parameterInt)
        {
            GetAnimator(targetName).SetInteger(parameterName, parameterInt);
        }

        protected void SetAnimator(string targetName, int parameterName)
        {
            GetAnimator(targetName).SetTrigger(parameterName);
        }

        protected void SetAnimatorActive(string targetName, bool isActive)
        {
            GetAnimator(targetName).enabled = isActive;
        }

        protected void StopAnimator(string targetName)
        {
            Animator currentAnimator = GetAnimator(targetName);
            foreach (AnimatorControllerParameter parameter in currentAnimator.parameters)
            {
                switch (parameter.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        currentAnimator.SetBool(parameter.name, false);
                        break;
                    case AnimatorControllerParameterType.Int:
                        currentAnimator.SetInteger(parameter.name, 0);
                        break;
                    case AnimatorControllerParameterType.Float:
                        currentAnimator.SetFloat(parameter.name, 0.0f);
                        break;
                    case AnimatorControllerParameterType.Trigger:
                        currentAnimator.ResetTrigger(parameter.name);
                        break;
                }
            }

            var defaultTransform = currentAnimator.transform;
            defaultTransform.rotation = Quaternion.identity;
            defaultTransform.localScale = Vector3.one;
            currentAnimator.enabled = false;
        }

        /*protected void StopAnimator()
        {
            if (GetAnimator() == null) return;
            Animator currentAnimator = GetAnimator();
            if (currentAnimator.IsPlayingAnimatorController())
            {
                foreach (AnimatorControllerParameter parameter in currentAnimator.parameters)
                {
                    switch (parameter.type)
                    {
                        case AnimatorControllerParameterType.Bool:
                            currentAnimator.SetBool(parameter.name, false);
                            break;
                        case AnimatorControllerParameterType.Int:
                            currentAnimator.SetInteger(parameter.name, 0);
                            break;
                        case AnimatorControllerParameterType.Float:
                            currentAnimator.SetFloat(parameter.name, 0.0f);
                            break;
                        case AnimatorControllerParameterType.Trigger:
                            currentAnimator.ResetTrigger(parameter.name);
                            break;
                    }
                }
            }

            var defaultTransform = currentAnimator.transform;
            defaultTransform.rotation = Quaternion.identity;
            defaultTransform.localScale = Vector3.one;
            currentAnimator.enabled = false;
        }*/

        #endregion

        #region Get References

        // protected LocalizeStringEvent GetLocalizeStringEvent(string targetName)
        // {
        //     foreach (var localizeString in localizeStrings)
        //     {
        //         if (localizeString.name == targetName)
        //             return localizeString;
        //     }
        //
        //     return null;
        // }

        protected GameObject GetGameObject(string targetName)
        {
            foreach (var item in gameObjects)
            {
                if (item.name == targetName)
                {
                    return item;
                }
            }

            return null;
        }

        public Button GetButton(string targetName)
        {
            foreach (var item in buttons)
            {
                if (item.name == targetName)
                {
                    return item;
                }
            }

            return null;
        }

        protected Image[] GetAllImages => uiImages;

        protected TMP_Text GetText(string targetName)
        {
            foreach (var item in texts)
            {
                if (item.name == targetName)
                {
                    return item;
                }
            }

            return null;
        }

        protected string GetInputFieldValue(string targetName)
        {
            var inputField = GetInputField(targetName);
            return inputField == null ? string.Empty : inputField.text;
        }


        protected TMP_InputField GetInputField(string targetName)
        {
            foreach (var item in tmpInputFields)
            {
                if (item.name == targetName)
                {
                    return item;
                }
            }

            return null;
        }

        protected Slider GetSlider(string targetName)
        {
            foreach (var item in sliders)
            {
                if (item.name == targetName)
                {
                    return item;
                }
            }

            return null;
        }

        protected Text GetLegacyText(string targetName)
        {
            foreach (var item in legacyTexts)
            {
                if (item.name == targetName)
                {
                    return item;
                }
            }

            return null;
        }


        protected Image GetImage(string targetName)
        {
            foreach (var item in uiImages)
            {
                if (item.name == targetName)
                {
                    return item;
                }
            }

            return null;
        }

        protected CanvasGroup GetCanvasGroup(string targetName)
        {
            foreach (var item in canvasGroups)
            {
                if (item.name == targetName)
                {
                    return item;
                }
            }

            return null;
        }

        private Animator GetAnimator(string targetName)
        {
            foreach (var item in animators)
            {
                if (item.name == targetName)
                {
                    item.enabled = true;
                    return item;
                }
            }

            return null;
        }

        protected Animator GetAnimator()
        {
            if (!TryGetComponent(out Animator animator)) return null;
            animator.enabled = true;
            return animator;
        }

        protected AnimatorStateInfo GetAnimatorInfo()
        {
            return GetAnimator().GetCurrentAnimatorStateInfo(0);
        }

        private bool AnimatorIsReady(Animator animator)
        {
            if (animator == null) return false;
            if (animator.gameObject.activeSelf == false) return false;
            return animator.enabled != false;
        }

        #endregion

        #region Tween Actions

        public void AnimateSlider(string sliderName, float value, float maxValue, float animationDuration = 1f)
        {
            var targetSlider = GetSlider(sliderName);
            if (targetSlider == null)
            {
                return;
            }

            var endValue = Helper.Remap(value, 0, maxValue, 0, 1);

            DOTween.To(() => targetSlider.value, x => targetSlider.value = x, endValue, animationDuration).SetUpdate(true);
        }

        public void AnimateNumericText(string textObjectName, float endValue, float animationDuration = 1f)
        {
            var targetText = GetText(textObjectName);
            if (targetText == null)
            {
                return;
            }

            DOTween.To(() => float.TryParse(targetText.text, out var value) ? value : 0,
                x => targetText.text = x.ToString("F0"),
                endValue,
                animationDuration);
        }

        /// <summary>
        ///  {VALUE} will be replaced with the current value
        /// </summary>
        /// <param name="textObjectName"></param>
        /// <param name="startValue"></param>
        /// <param name="endValue"></param>
        /// <param name="textTemplate"></param>
        public Tween AnimateNumericText(string textObjectName, string textTemplate, long startValue, long endValue,
            float animationDuration = 1f, float delay = 0f)
        {
            var targetText = GetText(textObjectName);
            if (targetText == null) return null;

            targetText.text = textTemplate.Replace("{VALUE}", startValue.ToString());

            return DOTween.To(() => startValue,
                value => targetText.text = textTemplate.Replace("{VALUE}", value.ToString()),
                endValue,
                animationDuration).SetDelay(delay);
        }

        public void DoPunchScale(Transform targetTransform)
        {
            GetComponent<RectTransform>().localScale = Vector3.one;
            targetTransform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.5f, 2)
                .OnComplete(() => targetTransform.DOScale(Vector3.one, 0.2f));
        }

        public void DoPunchScale()
        {
            GetComponent<RectTransform>().localScale = Vector3.one;
            transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.5f, 2)
                .OnComplete(() => transform.DOScale(Vector3.one, 0.2f));
        }

        public void ShakeLoop()
        {
            transform.DOShakeRotation(10, 5, 10, 90, false).OnComplete(ShakeLoop);
        }

        public void StopShake()
        {
            transform.DOKill();
        }

        public void StopShake(Transform targetTransform)
        {
            targetTransform.DOKill();
        }

        #endregion
    }
}