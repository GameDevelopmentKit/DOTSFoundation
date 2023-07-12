namespace GASCore.Systems.SoundSystems.System
{
    using GameFoundation.Scripts.Utilities;
    using GASCore.Systems.SoundSystems.Components;
    using global::System.Collections.Generic;
    using Unity.Collections;
    using Unity.Entities;

    public partial class PlaySoundSystem : SystemBase
    {
        private Dictionary<FixedString64Bytes, string> soundFixedStringToValue = new();
        protected override void OnUpdate()
        {
            Entities.WithoutBurst().WithChangeFilter<PlaySoundComponent>().ForEach((in PlaySoundComponent playSoundComponent) =>
            {
                if (!this.soundFixedStringToValue.TryGetValue(playSoundComponent.SoundAssetPath, out var soundName))
                {
                    soundName = playSoundComponent.SoundAssetPath.Value;
                    this.soundFixedStringToValue.Add(playSoundComponent.SoundAssetPath, soundName);
                }
    
                AudioManager.Instance.StopAllSound(soundName);
                AudioManager.Instance.PlaySound(soundName, playSoundComponent.IsLoop);
            }).Run();
        }
    }
}