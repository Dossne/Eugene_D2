using Features.Boosters;
using Features.CameraFollow;
using Features.Character;
using Features.Collectables;
using Features.Events;
using Features.ExtraItemSpawn;
using Features.FlyingTaskIcon;
using Features.FlyingText;
using Features.LevelTasks;
using Features.LevelComplete;
using Features.LevelTime;
using Features.LevelConfiguration;
using Features.LevelUp;
using Features.LevelLoose;
using Features.TargetMarker;
using Infrastructure.Ads;
using Infrastructure.Popups;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Features.CollectableCurrency;
using Features.ExtraCollectableUi;
using Features.LevelSessionStateControl;
using Features.RewardTrack;
using Features.WinStreak;
using Infrastructure.GameSceneUIControl;
using Features.Shadows;

namespace Infrastructure.Scope
{
    public class GameSceneScope : LifetimeScope
    {
        [SerializeField] private Transform levelRoot;


        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<GameSceneUIService>(Lifetime.Singleton);
            builder.Register<LevelSessionStateController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<ExtraCollectableUIService>(Lifetime.Singleton);
            builder.Register<LevelCreateManager>(Lifetime.Singleton).WithParameter(levelRoot).AsSelf().AsImplementedInterfaces();
            builder.Register<ExtraItemsSpawner>(Lifetime.Singleton).WithParameter(levelRoot).AsSelf().AsImplementedInterfaces();
            builder.Register<CharacterManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<LevelUpManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<PopupOpedListener>(Lifetime.Singleton);
            builder.Register<ItemCollectManager>(Lifetime.Singleton);
            builder.Register<CollectableItemDeactivator>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<LevelTaskManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<LevelFlyingTextManager>(Lifetime.Singleton);
            builder.Register<LevelCompleteManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<LevelLooseManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<LevelTimeManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<FlyingTaskIconManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<PreBoosterInGamePopupController>(Lifetime.Singleton);
            builder.Register<CollectableCurrencyService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

            {
                builder.Register<InGameBoosterController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
                builder.Register<BoosterBehaviourFactory>(Lifetime.Singleton);
            }
            
            builder.Register<CameraManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<ShadowsService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

            builder.Register<GameBaseAnalytics>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

            {
                builder.Register<CollectItemEvent>(Lifetime.Singleton);
                builder.Register<AllTaskCompleteEvent>(Lifetime.Singleton);
                builder.Register<ExperienceAddRequest>(Lifetime.Singleton);
            }
            
            builder.Register<TargetMarkerService>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<WinStreakBonusApplier>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<RewardTrackCollectController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

            
            builder.RegisterEntryPoint<GameSceneFlow>(Lifetime.Scoped).WithParameter(this);
        }
    }
}