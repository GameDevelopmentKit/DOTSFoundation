namespace GASCore.Systems.VisualEffectSystems.Systems
{
    using GameFoundation.Scripts.Utilities;
    using GASCore.Groups;
    using GASCore.Systems.SoundSystems.Components;
    using Unity.Entities;

    [UpdateInGroup(typeof(AbilityVisualEffectGroup))]
    [RequireMatchingQueriesForUpdate]
    public partial class PlaySoundSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            this.Entities.WithoutBurst().WithChangeFilter<PlaySoundComponent>().ForEach((in PlaySoundComponent playSoundComponent) =>
            {
                // AudioManager.Instance.StopAllSound(playSoundComponent.SoundAssetPath);
                AudioManager.Instance.PlaySound(playSoundComponent.SoundAssetPath, playSoundComponent.IsLoop);
            }).Run();
        }
    }
}