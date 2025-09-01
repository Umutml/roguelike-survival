using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameCore.Player;
using GameCore.Scriptables;
using Michsky.UI.ModernUIPack;
using UI.Game.Architectural;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VContainer;

namespace _Scripts.UI.Game.InGame.Pause
{
    public class SkillAcquiredDetails : Content
    {
        #region Serializable Fields

        [SerializeField] private List<Image> starImages = new();
        [SerializeField] private Sprite starSprite;
        [SerializeField] private Sprite starEmptySprite;
        [SerializeField] private float disableDelay = 3f;

        #endregion

        #region Private Fields

        private IObjectResolver _resolver;
        private PlayerSkillController _playerSkillController;
        private Coroutine _disableDetailsCoroutine;
        private WaitForSecondsRealtime _waitForSeconds;
        private UIGradient _uiGradient;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            _uiGradient = GetComponent<UIGradient>();
            _waitForSeconds = new WaitForSecondsRealtime(disableDelay);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            TryStopCoroutine();
        }

        #endregion

        #region Public Methods

        public void Initialize(IObjectResolver resolver, SkillsAcquiredContent skillsAcquiredContent,
            SkillsAcquiredSegment segment)
        {
            _resolver = resolver;
            _playerSkillController = _resolver.Resolve<PlayerSkillController>();

            gameObject.SetActive(true);
            UpdateDetails(skillsAcquiredContent, segment);
            RestartDisableCoroutine();
        }

        #endregion

        #region Private Methods

        private void UpdateDetails(SkillsAcquiredContent skillsAcquiredContent, SkillsAcquiredSegment segment)
        {
            _playerSkillController.LoadSkillCollection();
            var details = _playerSkillController.GetSkillDetail(segment.Skill);
            var starUpgrade = segment.Skill.starUpgrades.FirstOrDefault(x => x.starLevel == details.StarLevel);
            SetText("Title", segment.Skill.name);
            SetText("Description", starUpgrade.description);
            SetGradientBySkill(segment.Skill);
            UpdateStarImages(starUpgrade.starLevel);
            SetStartPosition(skillsAcquiredContent, segment);
        }

        private void SetGradientBySkill(Skill skill)
        {
            var skillColorData =
                _playerSkillController.SkillColorData.FirstOrDefault(x => x.upgradeType == skill.upgradeType);
            _uiGradient.EffectGradient = skillColorData.gradient;
        }

        private void UpdateStarImages(int starLevel)
        {
            var currentStarLevel = starLevel > 1 ? starLevel - 1 : 3;

            for (var i = 0; i < starImages.Count; i++)
            {
                starImages[i].sprite = i < currentStarLevel ? starSprite : starEmptySprite;
            }
        }

        private void RestartDisableCoroutine()
        {
            TryStopCoroutine();
            _disableDetailsCoroutine = StartCoroutine(DisableDetailsAfterDelay());
        }

        private void TryStopCoroutine()
        {
            if (_disableDetailsCoroutine == null) return;
            StopCoroutine(_disableDetailsCoroutine);
            _disableDetailsCoroutine = null;
        }

        private IEnumerator DisableDetailsAfterDelay()
        {
            yield return _waitForSeconds;
            gameObject.SetActive(false);
            _disableDetailsCoroutine = null;
        }

        private void SetStartPosition(SkillsAcquiredContent skillsAcquiredContent, SkillsAcquiredSegment segment)
        {
            var rectTransform = GetComponent<RectTransform>();
            transform.parent = segment.transform;
            rectTransform.localPosition = new Vector2(0, -150);
            transform.parent = skillsAcquiredContent.DetailsParent.transform;
        }

        #endregion
    }
}
