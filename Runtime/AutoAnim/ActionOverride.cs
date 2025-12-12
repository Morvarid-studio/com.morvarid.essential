using System;
using System.Collections.Generic;
using UnityEngine;

namespace MorvaridEssential
{
    // سیستم داینامیک برای override کردن فیلدهای action
    [Serializable]
    public class ActionOverride
    {
        [Header("Override Settings")]
        [Tooltip("Enable to override specific action parameters")]
        public bool enabled = false;

        // Dictionary برای نگهداری override values
        // Key: نام فیلد در action, Value: مقدار override
        [SerializeField]
        private List<OverrideEntry> overrideEntries = new List<OverrideEntry>();

        [Serializable]
        public class OverrideEntry
        {
            public string fieldName;
            public bool isOverridden;
            public string valueJson; // برای نگهداری مقدار به صورت JSON (برای انواع مختلف)
            public System.Type fieldType;
        }

        // اعمال override با Reflection
        public void ApplyOverrides(UIAnimAction action)
        {
            if (!enabled || action == null) return;

            var actionType = action.GetType();
            foreach (var entry in overrideEntries)
            {
                if (!entry.isOverridden) continue;

                var field = actionType.GetField(entry.fieldName);
                if (field == null) continue;

                try
                {
                    object value = DeserializeValue(entry.valueJson, entry.fieldType);
                    if (value != null)
                    {
                        field.SetValue(action, value);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Failed to apply override for field {entry.fieldName}: {ex.Message}");
                }
            }
        }

        // ذخیره مقادیر اصلی برای بازگردانی
        private Dictionary<string, object> _originalValues = new Dictionary<string, object>();

        public void SaveOriginalValues(UIAnimAction action)
        {
            if (action == null) return;

            _originalValues.Clear();
            var actionType = action.GetType();

            foreach (var entry in overrideEntries)
            {
                var field = actionType.GetField(entry.fieldName);
                if (field != null)
                {
                    _originalValues[entry.fieldName] = field.GetValue(action);
                }
            }
        }

        // بازگردانی مقادیر اصلی
        public void RestoreOriginalValues(UIAnimAction action)
        {
            if (action == null || _originalValues.Count == 0) return;

            var actionType = action.GetType();
            foreach (var kvp in _originalValues)
            {
                var field = actionType.GetField(kvp.Key);
                if (field != null)
                {
                    field.SetValue(action, kvp.Value);
                }
            }
            _originalValues.Clear();
        }

        // دریافت لیست فیلدهای action
        public static List<FieldInfo> GetActionFields(UIAnimAction action)
        {
            var fields = new List<FieldInfo>();
            if (action == null) return fields;

            var actionType = action.GetType();
            var allFields = actionType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            foreach (var field in allFields)
            {
                // فقط فیلدهای serializable
                if (IsSerializableType(field.FieldType))
                {
                    fields.Add(new FieldInfo
                    {
                        name = field.Name,
                        type = field.FieldType,
                        value = field.GetValue(action)
                    });
                }
            }

            return fields;
        }

        // به‌روزرسانی entries بر اساس action
        public void UpdateEntriesFromAction(UIAnimAction action)
        {
            if (action == null) return;

            var actionFields = GetActionFields(action);
            var existingEntries = new Dictionary<string, OverrideEntry>();
            
            // حفظ entries موجود
            foreach (var entry in overrideEntries)
            {
                existingEntries[entry.fieldName] = entry;
            }

            overrideEntries.Clear();

            // ایجاد entries جدید بر اساس فیلدهای action
            foreach (var fieldInfo in actionFields)
            {
                if (existingEntries.TryGetValue(fieldInfo.name, out var existing))
                {
                    // حفظ تنظیمات قبلی
                    existing.fieldType = fieldInfo.type;
                    // به‌روزرسانی مقدار فعلی فقط اگر override نشده باشد
                    // اگر override شده باشد، مقدار override را حفظ می‌کنیم
                    if (!existing.isOverridden)
                    {
                        // اگر override نشده، همیشه با مقدار action همگام نگه دار
                        existing.valueJson = SerializeValue(fieldInfo.value, fieldInfo.type);
                    }
                    // اگر override شده، valueJson را تغییر نمی‌دهیم (مقدار override حفظ می‌شود)
                    overrideEntries.Add(existing);
                }
                else
                {
                    // ایجاد entry جدید
                    overrideEntries.Add(new OverrideEntry
                    {
                        fieldName = fieldInfo.name,
                        isOverridden = false,
                        valueJson = SerializeValue(fieldInfo.value, fieldInfo.type),
                        fieldType = fieldInfo.type
                    });
                }
            }
        }

        private static bool IsSerializableType(System.Type type)
        {
            return type == typeof(int) || type == typeof(float) || type == typeof(bool) ||
                   type == typeof(string) || type == typeof(Vector2) || type == typeof(Vector3) ||
                   type.IsEnum || (type.Namespace != null && type.Namespace.StartsWith("DG.Tweening") && type.Name == "Ease");
        }

        private static string SerializeValue(object value, System.Type type)
        {
            if (value == null) return "";
            
            if (type == typeof(float) || type == typeof(int) || type == typeof(bool))
                return value.ToString();
            
            if (type.IsEnum || (type.Namespace != null && type.Namespace.StartsWith("DG.Tweening") && type.Name == "Ease"))
                return value.ToString();
            
            return JsonUtility.ToJson(value);
        }

        private static object DeserializeValue(string json, System.Type type)
        {
            if (string.IsNullOrEmpty(json)) return GetDefaultValue(type);

            if (type == typeof(float))
                return float.TryParse(json, out var f) ? f : 0f;
            
            if (type == typeof(int))
                return int.TryParse(json, out var i) ? i : 0;
            
            if (type == typeof(bool))
                return bool.TryParse(json, out var b) && b;
            
            if (type.IsEnum)
                return System.Enum.Parse(type, json);
            
            if (type.Namespace != null && type.Namespace.StartsWith("DG.Tweening") && type.Name == "Ease")
                return System.Enum.Parse(type, json);

            try
            {
                return JsonUtility.FromJson(json, type);
            }
            catch
            {
                return GetDefaultValue(type);
            }
        }

        private static object GetDefaultValue(System.Type type)
        {
            if (type.IsValueType)
                return System.Activator.CreateInstance(type);
            return null;
        }

        public class FieldInfo
        {
            public string name;
            public System.Type type;
            public object value;
        }
    }
}
