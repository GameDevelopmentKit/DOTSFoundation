namespace TutorialSystem.Systems
{
    using GameFoundation.Scripts.UIModule.ScreenFlow.Signals;
    using GameFoundation.Scripts.Utilities.Extension;
    using TaskModule;
    using TaskModule.TaskBase;
    using TutorialSystem.Components;
    using Unity.Entities;
    using Zenject;

    [UpdateInGroup(typeof(TaskPresentationSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    public partial class WaitOnEnterScreenSystem : SystemBase
    {
        [Inject] private SignalBus      signalBus;
        private string                currentScreenId = string.Empty;
        protected override void OnCreate()
        {
            base.OnCreate();
            this.GetCurrentContainer().Inject(this);
            this.signalBus.ParentBus.Subscribe<ScreenShowSignal>(OnEnterScreen);
        }
        
        private void OnEnterScreen(ScreenShowSignal signal)
        {
            this.currentScreenId = signal.ScreenPresenter.ScreenId;
        }
        
        protected override void OnUpdate()
        {
            foreach (var (waitOnEnterScreen, entity) in SystemAPI.Query<WaitOnEnterScreen>().WithAll<TaskIndex, ActivatedTag>().WithDisabled<CompletedTag>().WithEntityAccess())
            {
                if (this.currentScreenId.EndsWith(waitOnEnterScreen.ScreenId, System.StringComparison.Ordinal))
                {
                    SystemAPI.SetComponentEnabled<CompletedTag>(entity, true);
                }
            }
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.signalBus.ParentBus.Unsubscribe<ScreenShowSignal>(OnEnterScreen);
        }
    }
}