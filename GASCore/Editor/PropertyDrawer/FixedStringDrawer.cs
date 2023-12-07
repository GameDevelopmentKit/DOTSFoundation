namespace GASCore.Editor.PropertyDrawer
{
    using System;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities.Editor;
    using Unity.Collections;
    using UnityEngine;

    abstract class FixedStringDrawerBase<T> : OdinValueDrawer<T> where T : struct, IEquatable<T>
    {
        protected abstract int    MaxLength { get; }
        protected abstract string Value     { get; set; }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            Value = SirenixEditorFields.TextField(label, Value);
        }
    }

    class FixedString32Inspector : FixedStringDrawerBase<FixedString32Bytes>
    {
        protected override int MaxLength => FixedString32Bytes.UTF8MaxLengthInBytes;

        protected override string Value { get => this.ValueEntry.SmartValue.ToString(); set => this.ValueEntry.SmartValue = (FixedString32Bytes)value; }
    }
    
    class FixedString64Inspector : FixedStringDrawerBase<FixedString64Bytes>
    {
        protected override int MaxLength => FixedString64Bytes.UTF8MaxLengthInBytes;

        protected override string Value { get => this.ValueEntry.SmartValue.ToString(); set => this.ValueEntry.SmartValue = (FixedString64Bytes)value; }
    }
    
    class FixedString128Inspector : FixedStringDrawerBase<FixedString128Bytes>
    {
        protected override int MaxLength => FixedString128Bytes.UTF8MaxLengthInBytes;

        protected override string Value { get => this.ValueEntry.SmartValue.ToString(); set => this.ValueEntry.SmartValue = (FixedString128Bytes)value; }
    }
}