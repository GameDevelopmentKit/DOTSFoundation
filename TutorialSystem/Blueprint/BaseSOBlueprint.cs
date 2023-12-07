namespace TutorialSystem.Blueprint
{
    using Sirenix.OdinInspector;
    using UnityEngine.AddressableAssets;

    public interface IBaseSOBlueprint
    {
    }
    public abstract class BaseSOBlueprint<T>: SerializedScriptableObject , IBaseSOBlueprint where T : BaseSOBlueprint<T>
    {
        public static  T Current {
            get {
                if (_current == null)
                {
                    _current = Addressables.LoadAssetAsync<T>($"{typeof(T).Name}").WaitForCompletion();
                    _current.OnDatabaseLoaded();
                }

                return _current;
            }
        }
        private static  T _current;

        protected virtual void OnDatabaseLoaded()
        {
            
        }
        
#if UNITY_EDITOR
        protected static readonly string MenuItemName = $"Create {nameof(T)}";

        // [UnityEditor.MenuItem("GDK/SOBlueprint/" + MenuItemName)]
        public static void CreateAsset () {
            if (Current == null) {
                _current = CreateInstance<T>();
                UnityEditor.AssetDatabase.CreateAsset(_current, $"Assets/ScriptableObjects/Blueprints/{typeof(T).Name}.asset");
                UnityEditor.AssetDatabase.SaveAssets();
            }

            UnityEditor.Selection.activeObject = Current;
        }
#endif

    }
}
    
        
    
