using GameCore.Scriptables;
using UI.Game.Architectural;

namespace UI.Game.InGame.InventoryPanel
{
    public class InGameInventoryItemSegment : Content
    {
        private const string BACKGROUND = "Background";
        private const string SKILL_AREA = "SkillArea";

        public void Initialize(PostMatchItem item)
        {
            SetGameObject(BACKGROUND, item == null);
            SetGameObject(SKILL_AREA, item != null);
        }
    }
}
