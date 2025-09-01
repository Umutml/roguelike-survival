using _Scripts.GameCore.Level;
using _Scripts.GameCore.NPC;
using _Scripts.GameCore.Shop;
using _Scripts.GameCore.Tutorial;
using GameCore;
using GameCore.Health;
using GameCore.Inventory;
using GameCore.Level;
using GameCore.Player;
using GameCore.PopupSystem;
using GameCore.Spawner;
using GameCore.Tutorial;
using GameCore.Wave;
using Interfaces;
using Managers;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace SceneScopes
{
    public class GamesceneLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameSceneSetupManager gameSceneSetupManager;
        [SerializeField] private PopupManager popupManager;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerCarController playerCarController;
        [SerializeField] private MobManager mobManager;
        [SerializeField] private WaveLevelManager waveLevelManager;
        [SerializeField] private LootDropManager lootDropManager;
        [SerializeField] private DropIncrementManager dropIncrementManager;
        [SerializeField] private BoxManager boxManager;
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private PlayerStatusController playerStatusController;
        [SerializeField] private PlayerSkillController playerSkillController;
        [SerializeField] private GameInventoryManager gameInventoryManager;
        [SerializeField] private ItemPicker itemPicker;
        [SerializeField] private DestroyableManager destroyableManager;
        [SerializeField] private TutorialSequenceController tutorialSequenceController;
        [SerializeField] private DamageNumberManager damageNumberManager;
        [SerializeField] private VibrationManager vibrationManager;
        [SerializeField] private ManagementNpcController managementNpcController;
        [SerializeField] private PlayerCollectItemController playerCollectItemController;
        [SerializeField] private CarManager carManager;
        [SerializeField] private TimerInfoController timerInfoController;
        [SerializeField] private AreaNpc carUpgradeNpc;
        [SerializeField] private ModelVisualManager modelVisualManager;
        [SerializeField] private ObjectiveManager objectiveManager;
        [SerializeField] private GridManager gridManager;
        [SerializeField] private ShopManager shopManager;
        [SerializeField] private IAPurchaseManager iapManager;
        [SerializeField] private BadgeManager badgeManager;
        [SerializeField] private AlertManager alertManager;
        [SerializeField] private LevelManager levelManager;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(gameSceneSetupManager);
            builder.RegisterInstance(popupManager);
            builder.RegisterInstance(playerController);
            builder.RegisterInstance(playerCarController);
            builder.RegisterInstance(mobManager).As<IMobSpawnService>();
            builder.RegisterInstance(waveLevelManager).As<ILevelService>();
            builder.RegisterInstance(lootDropManager);
            builder.RegisterInstance(dropIncrementManager);
            builder.RegisterInstance(boxManager);
            builder.RegisterInstance(waveManager);
            builder.RegisterInstance(playerStatusController);
            builder.RegisterInstance(playerSkillController).As<IAbilityService>();
            builder.RegisterInstance(gameInventoryManager).As<IInventoryManager>();
            builder.RegisterInstance(itemPicker);
            builder.RegisterInstance(destroyableManager).As<IDamageableRegisterService>();
            builder.RegisterInstance(tutorialSequenceController).As<ITutorialService>();
            builder.RegisterInstance(damageNumberManager);
            builder.RegisterInstance(vibrationManager);
            builder.RegisterInstance(managementNpcController);
            builder.RegisterInstance(playerCollectItemController);
            builder.RegisterInstance(carManager);
            builder.RegisterInstance(timerInfoController);
            builder.RegisterInstance(carUpgradeNpc);
            builder.RegisterInstance(modelVisualManager);
            builder.RegisterInstance(objectiveManager);
            builder.RegisterInstance(gridManager);
            builder.RegisterInstance(shopManager);
            builder.RegisterInstance(iapManager);
            builder.RegisterInstance(badgeManager);
            builder.RegisterInstance(alertManager);
            builder.RegisterInstance(levelManager);
        }
    }
}
