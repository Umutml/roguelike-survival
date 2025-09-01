using System.Collections.Generic;
using _Utilities;
using Cathei.LinqGen;
using MyBox;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "SkillData", menuName = "ScriptableObjects/SkillData", order = 0)]
    public class SkillData : ScriptableObject
    {
        [SerializeField] private List<Skill> skills;
        public List<SkillColorData> skillColorData;

        public List<Skill> Skills
        {
            get
            {
                skills.ForEach(x => x.starUpgrades.ForEach(y => y.upgradeDetails.ForEach(z => z.skill = x)));
                return skills;
            }
        }

        public void CreateNewSkillData()
        {
            SaveLoadHelper.DeleteData<SkillCollection>();
            var skillCollection = new SkillCollection
            {
                Skills = skills.Gen().Select(skill => new SkillDetail
                {
                    Name = skill.name,
                    StarLevel = 1
                }).ToArray()
            };
            SaveLoadHelper.SaveData(skillCollection);
        }

        public List<Skill> GetRandomSkills(UpgradeType upgradeType)
        {
            return skills.Gen().Where(x => (x.upgradeType & upgradeType) != 0).OrderBy(x => Random.value).ToList();
        }

        public StarUpgrade GetStarUpgrade(Skill skill, int starLevel)
        {
            return skill.starUpgrades.Gen().Where(x => x.starLevel == starLevel).FirstOrDefault();
        }
    }

    [System.Serializable]
    public class SkillCollection
    {
        public SkillDetail[] Skills { get; set; }
    }

    [System.Serializable]
    public class SkillDetail
    {
        public string Name { get; set; }
        public int StarLevel { get; set; }
    }


    [System.Serializable]
    public struct SkillColorData
    {
        public UpgradeType upgradeType;
        public Gradient gradient;
    }
}
