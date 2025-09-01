using _Scripts.Interfaces;
using _Scripts.Managers;
using Interfaces;
using Managers;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SceneScopes
{
    public class BasesceneLifetimeScope : LifetimeScope
    {
        [SerializeField] private SceneLoadManager sceneLoadManager;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private AnalyticManager analyticManager;
        [SerializeField] private EnergyManager energyManager;
        [SerializeField] private MediationManager mediationManager;
        [SerializeField] private GeneralOnClickManager generalOnClickManager;
        [SerializeField] private GameParameterManager gameParameterManager;

        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(sceneLoadManager).As<ISceneLoadService>();
            builder.RegisterInstance(audioManager).As<IAudioService>();
            builder.RegisterInstance(gameManager).As<IGameService>();
            builder.RegisterInstance(analyticManager).As<IAnalyticsService>();
            builder.RegisterInstance(energyManager).As<IEnergyService>();
            builder.RegisterInstance(mediationManager).As<IMediationService>();
            builder.RegisterInstance(generalOnClickManager).As<IGeneralOnClickManager>();
            builder.RegisterInstance(gameParameterManager).As<GameParameterManager>();
        }
    }
}
