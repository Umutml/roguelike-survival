using _Scripts.Utilities;
using GameCore.Player;
using UnityEngine;
using VContainer;

namespace _Scripts.UI.Game.InGame.Pause
{
    public class SkillsAcquiredContent : MonoBehaviour
    {
        #region Serializable Fields

        [SerializeField] private Transform content;
        [SerializeField] private GameObject skillSegmentPrefab;
        [SerializeField] private SkillAcquiredDetails skillAcquiredDetails;
        [SerializeField] private RectTransform detailsParent;
        [SerializeField] private GameObject lockedObject;

        #endregion

        #region Private Fields

        private IObjectResolver _resolver;
        private PlayerSkillController _playerSkillController;

        #endregion

        #region Properties

        public RectTransform DetailsParent => detailsParent;

        #endregion

        #region Public Methods

        public void Initialize(IObjectResolver resolver)
        {
            _resolver = resolver;
            _playerSkillController = resolver.Resolve<PlayerSkillController>();
            CreateSkillSegment();
        }

        #endregion

        #region Private Methods

        private void CreateSkillSegment()
        {
            if (_playerSkillController.Skills is not {Count: > 0})
            {
                lockedObject.gameObject.SetActive(true);
                return;
            }

            foreach (var skill in _playerSkillController.Skills)
            {
                var skillSegment = Instantiate(skillSegmentPrefab, content);
                var skillSegmentComponent = skillSegment.GetComponent<SkillsAcquiredSegment>();
                skillSegmentComponent.Initialize(skill, OnClickSkillSegment);
            }

            lockedObject.gameObject.SetActive(false);
        }

        private void OnClickSkillSegment(SkillsAcquiredSegment skillsAcquiredSegment)
        {
            skillAcquiredDetails.Initialize(_resolver, this, skillsAcquiredSegment);
        }

        #endregion
    }
}
