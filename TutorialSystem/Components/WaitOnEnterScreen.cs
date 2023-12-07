namespace TutorialSystem.Components
{
    using System.Collections.Generic;
    using System.Linq;
    using GameFoundation.Scripts.UIModule.ScreenFlow.BaseScreen.View;
    using GameFoundation.Scripts.Utilities.Extension;
    using Sirenix.OdinInspector;
    using TaskModule.Authoring;
    using Unity.Entities;

    public class WaitOnEnterScreen : IComponentData, ITaskGoalComponentConverter
    {
        [ValueDropdown("GetListScreenId")]   
        public string ScreenId;
        public void   Convert(EntityManager entityManager, Entity taskEntity) { entityManager.AddComponentData(taskEntity, this); }
        
        private List<string> GetListScreenId() => ReflectionUtils.GetAllDerivedTypes<IScreenView>().Select(type => type.Name).ToList();
    }
}