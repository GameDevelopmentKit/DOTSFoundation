namespace GASCore.Systems.SoundSystems.System
{
    using GameFoundation.Scripts.Utilities;
    using GASCore.Systems.SoundSystems.Components;
    using Unity.Entities;

    public partial class PlaySoundSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithoutBurst().WithChangeFilter<PlaySoundComponent>().ForEach((Entity entity,in PlaySoundComponent playSoundComponent) =>
            {
                AudioManager.Instance.StopAllSound(playSoundComponent.SoundAssetPath.Value);
                AudioManager.Instance.PlaySound(playSoundComponent.SoundAssetPath.Value,playSoundComponent.IsLoop);
            }).Run();
        }
    }
}