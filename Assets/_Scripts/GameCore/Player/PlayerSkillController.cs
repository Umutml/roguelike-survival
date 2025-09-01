using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.Utilities;
using _Utilities;
using Cathei.LinqGen;
using GameCore.AbilitySystem;
using GameCore.Car;
using GameCore.Health;
using GameCore.Scriptables;
using GameCore.Wave;
using Interfaces;
using MyBox;
using UnityEngine;
using VContainer;

namespace GameCore.Player
{
    public class PlayerSkillController : MonoBehaviour, IAbilityService
    {
        #region Events

        public event Action<UpgradeDetail> OnSkillUpgrade;
        public event Action OnResetSkill;

        #endregion

        #region Serialized Fields

        [SerializeField] private CharacterUpgradeResources characterUpgradeResources;
        [SerializeField] private CarMetaUpgradeResources carMetaUpgradeResources;
        [SerializeField] private SkillData skillData;
        [SerializeField] private List<Ability> abilities;

        #endregion

        #region Constants

        private const float MaxStarLevel = 3f;
        private const int MaxRandomSkills = 3;

        #endregion

        #region Private Fields

        private static readonly List<(string id, float calculateValue)> SkillValues = new();
        private readonly HashSet<Skill> _skills = new();
        private static WaveManager _waveManager;
        private ObjectiveManager _objectiveManager;
        private SkillCollection _skillCollection;
        private PlayerStatusController _playerStatusController;
        private readonly HashSet<(Func<bool>, EventBasedTrigger)> _eventConditions = new();
        private readonly HashSet<(DateTime, string)> _cooldownTuples = new();

        #endregion

        #region Properties

        public HashSet<Skill> Skills => _skills;
        public CharacterUpgradeResources CharacterUpgradeResources => characterUpgradeResources;
        public CarMetaUpgradeResources CarMetaUpgradeResources => carMetaUpgradeResources;
        public List<SkillColorData> SkillColorData => skillData.skillColorData;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            SetComponentValues();
            SetEventConditions();
            skillData.CreateNewSkillData();
        }

        private void OnEnable()
        {
            _objectiveManager.OnObjectiveComplete += OnResetSkillInvoke;
        }

        private void OnDestroy()
        {
            _objectiveManager.OnObjectiveComplete -= OnResetSkillInvoke;
        }

        private void Update()
        {
            foreach (var ability in abilities) ability.UpdateCooldown();
        }

        #endregion

        #region Private Methods

        [Inject]
        private void Initialize(IObjectResolver resolver)
        {
            foreach (var ability in abilities) ability.Setup(resolver, gameObject);
            _waveManager = resolver.Resolve<WaveManager>();
            _objectiveManager = resolver.Resolve<ObjectiveManager>();
        }

        private void SetEventConditions()
        {
            _eventConditions.Add((
                () => _playerStatusController != null && _playerStatusController.MaxHealth > 0 &&
                    _playerStatusController.Health / _playerStatusController.MaxHealth < 0.25f,
                EventBasedTrigger.LowHealth));
        }

        private void SetComponentValues()
        {
            _playerStatusController = GetComponent<PlayerStatusController>();
        }

        private void UpgradeSkillDetails(Skill skill)
        {
            SaveLoadHelper.UpdateData<SkillCollection>(skillCollection =>
            {
                var skillDetail = skillCollection.Skills.Gen().Where(s => s.Name.Equals(skill.name)).FirstOrDefault();
                if (skillDetail == null) return;

                skillDetail.StarLevel = skillDetail.StarLevel + 1 > MaxStarLevel ? 1 : skillDetail.StarLevel + 1;
            });
        }

        private void ApplyStatUpgrade(Skill skill)
        {
            if (skill?.starUpgrades == null)
                return;

            var skillDetail = GetSkillDetail(skill);
            if (skillDetail == null)
                return;

            var starLevel = Math.Clamp(skillDetail.StarLevel - 1, 0, skill.starUpgrades.Length - 1);

            var upgradeDetails = skill.starUpgrades[starLevel].upgradeDetails.ToList();
            upgradeDetails.ForEach(x => x.skill = skill);
            if (upgradeDetails.Count > 0)
            {
                ApplyStatUpgrade(upgradeDetails);
            }
        }

        private void RemoveExpiredCooldowns()
        {
            _cooldownTuples.RemoveWhere(cd =>
                (DateTime.UtcNow - cd.Item1).TotalSeconds >
                GetCooldownTime(skillData.Skills.Gen().Where(x => x.name == cd.Item2).FirstOrDefault()));
        }

        private List<(Skill, int)> GetEventBasedSkills()
        {
            var selectedSkills = new List<(Skill, int)>();
            var validConditions = _eventConditions.Gen().Where(x => x.Item1());

            foreach (var condition in validConditions)
            {
                var skill = skillData.Skills.Gen().Where(x =>
                    x.triggerType == TriggerType.EventBased &&
                    x.eventTriggerCondition.eventBasedTrigger == condition.Item2).FirstOrDefault();

                if (skill == null) continue;

                if (_cooldownTuples.Gen().Any(cd => cd.Item2 == skill.name)) continue;

                var skillDetail = GetSkillDetail(skill);
                selectedSkills.Add((skill, skillDetail?.StarLevel ?? 0));
                if (selectedSkills.Count >= MaxRandomSkills)
                    break;
            }

            return selectedSkills;
        }

        private void AddRandomSkills(List<(Skill, int)> selectedSkills, UpgradeType upgradeType)
        {
            var remainingSlots = MaxRandomSkills - selectedSkills.Count;
            if (remainingSlots <= 0) return;
            var additionalSkills = skillData.GetRandomSkills(upgradeType).Gen().Where(skill =>
                    selectedSkills.Gen().All(existing => existing.Item1 != skill) &&
                    (skill.triggerType == TriggerType.Passive ||
                        !_cooldownTuples.Gen().Any(cd => cd.Item2 == skill.name)))
                .Take(remainingSlots).Select(skill => (skill, GetSkillDetail(skill)?.StarLevel ?? 0));
            selectedSkills.AddRange(additionalSkills.ToList());
        }

        private float GetCooldownTime(Skill skill)
        {
            if (skill == null)
            {
                return 0;
            }

            var detail = GetSkillDetail(skill);
            return skill.triggerType switch
            {
                TriggerType.EventBased => skill.eventTriggerCondition.cooldowns.Gen()
                    .Where(x => x.level == detail.StarLevel).FirstOrDefault().cooldown,
                TriggerType.TimeBased => skill.timeBasedCondition.cooldowns.Gen()
                    .Where(x => x.level == detail.StarLevel).FirstOrDefault().cooldown,
                _ => 0
            };
        }

        #endregion

        #region Public Methods

        public void LoadSkillCollection()
        {
            _skillCollection = SaveLoadHelper.TryLoadRuntimeData<SkillCollection>();
            if (_skillCollection != null) return;
            LoggerNS.LogError("Failed to load SkillCollection: Skill collection data is null.");
        }

        public void ConfigureCharacterMetaSkills()
        {
            var characterMetaData = SaveLoadHelper.TryLoadPersistentData<CharacterMetaUpgradeData>();

            for (var i = 0; i < characterMetaData.UpgradeIndex; i++)
            {
                if (i >= characterUpgradeResources.CharacterUpgradeList.Count)
                {
                    break;
                }

                var upgradeDetail = characterUpgradeResources.CharacterUpgradeList[i].UpgradeDetails;
                ApplyStatUpgrade(upgradeDetail);
            }

            _playerStatusController.SetupHealthAndArmor();
        }

        public void OnResetSkillInvoke()
        {
            OnResetSkill?.Invoke();
            skillData.CreateNewSkillData();
            _skills.Clear();
        }

        public void ApplyStatUpgrade(List<UpgradeDetail> upgradeDetails)
        {
            foreach (var upgradeDetail in upgradeDetails)
            {
                OnSkillUpgrade?.Invoke(upgradeDetail);
            }
        }

        public void ApplySkillUpgrade(Skill skill)
        {
            if (skill == null)
            {
                LoggerNS.LogError("Cannot apply skill upgrade: Skill is null.");
                return;
            }

            if (skill.triggerType != TriggerType.Passive)
            {
                _cooldownTuples.Add((DateTime.UtcNow, skill.name));
            }

            _skills.Add(skill);
            ApplyStatUpgrade(skill);
            UpgradeSkillDetails(skill);
        }

        public void AddAbility(Ability ability)
        {
            abilities.Add(ability);
        }

        public List<(Skill, int)> GetRandomSkills(UpgradeType upgradeType)
        {
            RemoveExpiredCooldowns();
            LoadSkillCollection();
            if (_skillCollection == null)
                return null;
            var selectedSkills = GetEventBasedSkills();

            AddRandomSkills(selectedSkills, upgradeType);

            return selectedSkills;
        }

        public Skill GetSkillByUpgradeDetail(UpgradeDetail upgradeDetail)
        {
            return skillData.Skills.Gen().Where(x =>
                x.starUpgrades.Any(upgrade => upgrade.upgradeDetails.Contains(upgradeDetail))).FirstOrDefault();
        }

        public SkillDetail GetSkillDetail(Skill skill)
        {
            return _skillCollection.Skills.Gen().Where(s => s.Name == skill.name).FirstOrDefault();
        }

        public IAbility GetAbility(IAbilityService.AbilityType abilityType)
        {
            return abilities.Gen().Where(ability => ability.Type == abilityType).FirstOrDefault();
        }

        #endregion

        #region Static Methods

        public static void Calculate(ref float currentValue, ref string id, UpgradeDetail upgradeDetail)
        {
            var value = upgradeDetail.value * EvaluateOperator(upgradeDetail.valueModifierType);
            var calculateValue = upgradeDetail.valueModifierType switch
            {
                ValueModifierType.Add or ValueModifierType.Subtract => value,
                _ => currentValue * value
            };

            currentValue += calculateValue;

            if (!_waveManager.IsWaveActive)
            {
                return;
            }

            id = string.IsNullOrEmpty(id) ? Guid.NewGuid().ToString() : id;

            SkillValues.Add((id, calculateValue));
        }

        public static void ResetSkill(ref float currentValue, string id)
        {
            if (SkillValues is not {Count: > 0})
            {
                return;
            }

            if (!SkillValues.Gen().Any(x => x.Item1 == id))
            {
                return;
            }

            currentValue -= SkillValues.Gen().Sum(x => x.Item1 == id ? x.Item2 : 0);
            SkillValues.RemoveAll(x => x.Item1 == id);
        }

        private static float EvaluateOperator(ValueModifierType type)
        {
            return type switch
            {
                ValueModifierType.MultiplyIncrease => 0.01f,
                ValueModifierType.MultiplyDecrease => -0.01f,
                ValueModifierType.Add => 1f,
                ValueModifierType.Subtract => -1f,
                _ => 1
            };
        }

        #endregion
    }

    public class CarMetaUpgradeData
    {
        public List<CarMeta> CarMetaList = new();
    }

    public class CarMeta
    {
        public CarType CarType;
        public int UpgradeIndex;
    }

    public class CharacterMetaUpgradeData
    {
        public int UpgradeIndex;
    }

    public class CharacterSelectionData
    {
        public string SelectedCharacterKey;
    }
}
