using GameCore.Scriptables;
using UI.Game.Architectural;

namespace UI.Game.InGame.InventoryPanel
{
    public class InGameWeaponSegment : Content
    {
        private const string BACKGROUND = "Background";
        private const string WEAPON_AREA = "WeaponArea";

        public void Initialize(PostMatchItem item)
        {
            SetGameObject(BACKGROUND, item == null);
            SetGameObject(WEAPON_AREA, item != null);
        }
    }
}
