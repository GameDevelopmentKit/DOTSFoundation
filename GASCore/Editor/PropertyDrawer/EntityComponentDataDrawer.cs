//-----------------------------------------------------------------------
// <copyright file="EntityComponentDataDrawer.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace GASCore.Editor.PropertyDrawer
{
    using Sirenix.OdinInspector.Editor;
    using Unity.Entities;
    using UnityEngine;

    // public class EntityComponentDataDrawer<T> : OdinValueDrawer<T> where T : struct, IComponentData
    // {
    //     protected override void DrawPropertyLayout(GUIContent label)
    //     {
    //         OdinECSEditorGUI.HeaderLabel(typeof(T).FullName, OdinECSEditorGUI.EntityIcon, this.Property.Children.Count > 0);
    //         for (int i = 0; i < this.Property.Children.Count; i++)
    //         {
    //             this.Property.Children[i].Draw();
    //         }
    //         OdinECSEditorGUI.DrawVerticalInspectorSeparator();
    //     }
    // }
}
