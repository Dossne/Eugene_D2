using Features.Boosters;
using Features.BottomPanel;
using Features.Competition;
using Features.HudLevelButtons;
using Features.LavaQuest;
using Features.RewardTrack;
using VContainer;
using VContainer.Unity;

namespace Infrastructure.Scope
{
    public class MetaSceneScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<MetaSceneUIService>(Lifetime.Singleton);
            builder.Register<StartLevelButtonController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<PreBoosterPopupController>(Lifetime.Singleton);
            builder.Register<RewardTrackUiController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<LavaQuestUIController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            builder.Register<BottomPanelController>(Lifetime.Singleton).AsSelf();
            builder.Register<CompetitionUIController>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();
            
            builder.RegisterEntryPoint<MetaSceneFlow>();
        }
    }
}