#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using MorvaridEssential;

namespace MorvaridEssential.Editor
{
    [CustomPropertyDrawer(typeof(ActionOverride))]
    public class ActionOverridePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var enabledProp = property.FindPropertyRelative("enabled");
            var entriesProp = property.FindPropertyRelative("overrideEntries");
            
            var rect = position;
            rect.height = EditorGUIUtility.singleLineHeight;

            // Draw enabled toggle
            enabledProp.boolValue = EditorGUI.Toggle(rect, "Override Parameters", enabledProp.boolValue);
            rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (enabledProp.boolValue)
            {
                EditorGUI.indentLevel++;

                // پیدا کردن action از parent
                var action = GetActionFromParent(property);
                
                if (action == null)
                {
                    EditorGUI.HelpBox(rect, "Please assign an Action first", MessageType.Info);
                    rect.y += EditorGUIUtility.singleLineHeight * 2;
                }
                else
                {
                    // به‌روزرسانی entries بر اساس action (هر بار که action تغییر می‌کند)
                    if (Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint)
                    {
                        UpdateEntriesIfNeeded(property, action);
                    }

                    // نمایش فیلدها
                    var fields = ActionOverride.GetActionFields(action);
                    if (fields.Count == 0)
                    {
                        EditorGUI.HelpBox(rect, "No overrideable fields found in this action", MessageType.Info);
                        rect.y += EditorGUIUtility.singleLineHeight * 2;
                    }
                    else
                    {
                        foreach (var fieldInfo in fields)
                        {
                            DrawFieldOverride(property, ref rect, fieldInfo, action);
                        }
                    }
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        private UIAnimAction GetActionFromParent(SerializedProperty property)
        {
            // تلاش برای پیدا کردن action از parent Item
            var parent = property.serializedObject.targetObject;
            
            if (parent is Animalo animalo)
            {
                // باید از طریق property path پیدا کنیم
                var path = property.propertyPath;
                // path چیزی شبیه "items.Array.data[0].actionOverride" است
                // باید index را پیدا کنیم و action را بگیریم
                var match = System.Text.RegularExpressions.Regex.Match(path, @"items\.Array\.data\[(\d+)\]");
                if (match.Success)
                {
                    var index = int.Parse(match.Groups[1].Value);
                    if (animalo.items != null && index < animalo.items.Length)
                    {
                        return animalo.items[index].action;
                    }
                }
            }
            
            return null;
        }

        private void UpdateEntriesIfNeeded(SerializedProperty property, UIAnimAction action)
        {
            var entriesProp = property.FindPropertyRelative("overrideEntries");
            var actionType = action.GetType();
            var actionFields = ActionOverride.GetActionFields(action);
            
            // بررسی اینکه آیا entries به‌روز هستند
            bool needsUpdate = false;
            var existingFieldNames = new HashSet<string>();
            
            for (int i = 0; i < entriesProp.arraySize; i++)
            {
                var entry = entriesProp.GetArrayElementAtIndex(i);
                var fieldNameProp = entry.FindPropertyRelative("fieldName");
                if (fieldNameProp != null)
                {
                    existingFieldNames.Add(fieldNameProp.stringValue);
                }
            }
            
            foreach (var fieldInfo in actionFields)
            {
                if (!existingFieldNames.Contains(fieldInfo.name))
                {
                    needsUpdate = true;
                    break;
                }
            }
            
            if (needsUpdate)
            {
                // به‌روزرسانی entries
                var targetObj = property.serializedObject.targetObject;
                if (targetObj is Animalo animalo)
                {
                    var path = property.propertyPath;
                    var match = System.Text.RegularExpressions.Regex.Match(path, @"items\.Array\.data\[(\d+)\]");
                    if (match.Success)
                    {
                        var index = int.Parse(match.Groups[1].Value);
                        if (animalo.items != null && index < animalo.items.Length)
                        {
                            animalo.items[index].actionOverride.UpdateEntriesFromAction(action);
                            EditorUtility.SetDirty(targetObj);
                        }
                    }
                }
            }
        }

        private void DrawFieldOverride(SerializedProperty property, ref Rect rect, ActionOverride.FieldInfo fieldInfo, UIAnimAction action)
        {
            var entriesProp = property.FindPropertyRelative("overrideEntries");
            
            // پیدا کردن entry مربوط به این field
            SerializedProperty entryProp = null;
            int entryIndex = -1;
            for (int i = 0; i < entriesProp.arraySize; i++)
            {
                var entry = entriesProp.GetArrayElementAtIndex(i);
                var fieldNameProp = entry.FindPropertyRelative("fieldName");
                if (fieldNameProp != null && fieldNameProp.stringValue == fieldInfo.name)
                {
                    entryProp = entry;
                    entryIndex = i;
                    break;
                }
            }
            
            if (entryProp == null) return;

            var isOverriddenProp = entryProp.FindPropertyRelative("isOverridden");
            var valueJsonProp = entryProp.FindPropertyRelative("valueJson");
            var fieldTypeProp = entryProp.FindPropertyRelative("fieldType");

            // خواندن مقدار اصلی از action
            var field = action.GetType().GetField(fieldInfo.name);
            if (field == null) return;
            
            var originalValue = field.GetValue(action);
            var originalValueStr = SerializeValue(originalValue, fieldInfo.type);
            
            bool isOverridden = isOverriddenProp.boolValue;
            
            // اگر override نشده بودیم و مقدار action تغییر کرده، valueJson را به‌روز کن
            if (!isOverridden)
            {
                // اگر override نشده، همیشه valueJson را با مقدار فعلی action همگام نگه دار
                if (valueJsonProp.stringValue != originalValueStr)
                {
                    valueJsonProp.stringValue = originalValueStr;
                }
            }
            // اگر override شده، هیچ تغییری نمی‌دهیم - حتی اگر مقدار با action اصلی برابر باشد
            // چون کاربر عمداً override کرده و می‌خواهد این مقدار را حفظ کند

            // خط آبی کنار فیلد override شده (مثل Prefab Variant)
            if (isOverridden)
            {
                var blueLineRect = rect;
                blueLineRect.width = 3f;
                blueLineRect.x = 0;
                EditorGUI.DrawRect(blueLineRect, new Color(0.2f, 0.6f, 1f, 1f)); // آبی روشن
            }

            // نمایش label با style bold اگر override شده
            var labelRect = rect;
            labelRect.x += isOverridden ? 3f : 0; // فاصله از خط آبی
            labelRect.width = EditorGUIUtility.labelWidth - (isOverridden ? 3f : 0);
            
            if (isOverridden)
            {
                var boldStyle = new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold };
                EditorGUI.LabelField(labelRect, fieldInfo.name, boldStyle);
            }
            else
            {
                EditorGUI.LabelField(labelRect, fieldInfo.name);
            }

            // Value field (همیشه editable)
            var valueRect = rect;
            valueRect.x += EditorGUIUtility.labelWidth;
            valueRect.width = rect.width - EditorGUIUtility.labelWidth - (isOverridden ? 20 : 0); // فضا برای دکمه Revert

            // خواندن مقدار فعلی
            object currentValue;
            
            if (isOverridden)
            {
                // اگر override شده، از valueJsonProp بخوان
                currentValue = DeserializeValue(valueJsonProp.stringValue, fieldInfo.type, originalValue);
            }
            else
            {
                // اگر override نشده، از action بخوان
                currentValue = originalValue;
            }

            // Context Menu برای راست کلیک - روی کل rect (شامل label و value)
            var fullRect = rect;
            if (Event.current.type == EventType.ContextClick && fullRect.Contains(Event.current.mousePosition))
            {
                ShowContextMenu(property, entryIndex, fieldInfo, action, originalValue);
                Event.current.Use();
            }

            // نمایش مقدار - با رنگ متفاوت اگر override شده
            var originalColor = GUI.color;
            var originalBgColor = GUI.backgroundColor;
            
            if (isOverridden)
            {
                GUI.backgroundColor = new Color(0.2f, 0.6f, 1f, 0.2f); // پس‌زمینه آبی روشن
            }

                DrawValueField(property, valueRect, fieldInfo.type, currentValue, valueJsonProp, action, field, isOverridden, originalValue, entryIndex, isOverriddenProp);
            
            // دکمه Revert کنار فیلد override شده
            if (isOverridden)
            {
                var revertButtonRect = rect;
                revertButtonRect.x = valueRect.xMax + 2;
                revertButtonRect.width = 18;
                revertButtonRect.height = EditorGUIUtility.singleLineHeight;
                
                if (GUI.Button(revertButtonRect, "↺", EditorStyles.miniButton))
                {
                    RevertField(property, entryIndex, fieldInfo, action, originalValue);
                }
            }
            
            GUI.color = originalColor;
            GUI.backgroundColor = originalBgColor;

            rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        private void ShowContextMenu(SerializedProperty property, int entryIndex, ActionOverride.FieldInfo fieldInfo, UIAnimAction action, object originalValue)
        {
            var menu = new GenericMenu();
            
            var entriesProp = property.FindPropertyRelative("overrideEntries");
            var entryProp = entriesProp.GetArrayElementAtIndex(entryIndex);
            var isOverriddenProp = entryProp.FindPropertyRelative("isOverridden");
            var valueJsonProp = entryProp.FindPropertyRelative("valueJson");

            bool isOverridden = isOverriddenProp.boolValue;

            if (isOverridden)
            {
                // گزینه Revert
                menu.AddItem(new GUIContent("Revert"), false, () =>
                {
                    RevertField(property, entryIndex, fieldInfo, action, originalValue);
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Revert"));
            }

            menu.ShowAsContext();
        }

        private void RevertField(SerializedProperty property, int entryIndex, ActionOverride.FieldInfo fieldInfo, UIAnimAction action, object originalValue)
        {
            var entriesProp = property.FindPropertyRelative("overrideEntries");
            var entryProp = entriesProp.GetArrayElementAtIndex(entryIndex);
            var isOverriddenProp = entryProp.FindPropertyRelative("isOverridden");
            var valueJsonProp = entryProp.FindPropertyRelative("valueJson");

            // غیرفعال کردن override
            isOverriddenProp.boolValue = false;
            
            // به‌روزرسانی مقدار به مقدار اصلی
            valueJsonProp.stringValue = SerializeValue(originalValue, fieldInfo.type);

            property.serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(property.serializedObject.targetObject);
        }

        private string SerializeValue(object value, Type type)
        {
            if (value == null) return "";
            
            if (type == typeof(float) || type == typeof(int) || type == typeof(bool))
                return value.ToString();
            
            if (type.IsEnum || (type.Namespace != null && type.Namespace.StartsWith("DG.Tweening") && type.Name == "Ease"))
                return value.ToString();
            
            return JsonUtility.ToJson(value);
        }

        private object DeserializeValue(string json, Type type, object defaultValue)
        {
            if (string.IsNullOrEmpty(json)) return defaultValue;

            if (type == typeof(float))
                return float.TryParse(json, out var f) ? f : defaultValue;
            
            if (type == typeof(int))
                return int.TryParse(json, out var i) ? i : defaultValue;
            
            if (type == typeof(bool))
                return bool.TryParse(json, out var b) ? b : defaultValue;
            
            if (type.IsEnum)
                return System.Enum.TryParse(type, json, out var enumVal) ? enumVal : defaultValue;
            
            if (type.Namespace != null && type.Namespace.StartsWith("DG.Tweening") && type.Name == "Ease")
                return System.Enum.TryParse(type, json, out var easeVal) ? easeVal : defaultValue;

            try
            {
                return JsonUtility.FromJson(json, type);
            }
            catch
            {
                return defaultValue;
            }
        }

        private void DrawValueField(SerializedProperty property, Rect rect, Type fieldType, object currentValue, SerializedProperty valueJsonProp, UIAnimAction action, FieldInfo field, bool isOverridden, object originalValue, int entryIndex, SerializedProperty isOverriddenProp)
        {
            // همیشه editable است - اگر تغییر کرد، override فعال می‌شود
            
            if (fieldType == typeof(float))
            {
                var floatValue = (float)currentValue;
                EditorGUI.BeginChangeCheck();
                var newValue = EditorGUI.FloatField(rect, floatValue);
                if (EditorGUI.EndChangeCheck())
                {
                    // همیشه مقدار جدید را ذخیره کن
                    valueJsonProp.stringValue = newValue.ToString();
                    
                    // اگر قبلاً override نشده بود، حالا که تغییر دادیم، override را فعال کن
                    if (!isOverriddenProp.boolValue)
                    {
                        isOverriddenProp.boolValue = true;
                    }
                    // اگر قبلاً override شده بود، آن را true نگه دار (حتی اگر مقدار با action اصلی برابر شود)
                    
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
            else if (fieldType == typeof(int))
            {
                var intValue = (int)currentValue;
                EditorGUI.BeginChangeCheck();
                var newValue = EditorGUI.IntField(rect, intValue);
                if (EditorGUI.EndChangeCheck())
                {
                    // همیشه مقدار جدید را ذخیره کن
                    valueJsonProp.stringValue = newValue.ToString();
                    
                    // اگر قبلاً override نشده بود، حالا که تغییر دادیم، override را فعال کن
                    if (!isOverriddenProp.boolValue)
                    {
                        isOverriddenProp.boolValue = true;
                    }
                    // اگر قبلاً override شده بود، آن را true نگه دار
                    
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
            else if (fieldType == typeof(bool))
            {
                var boolValue = (bool)currentValue;
                EditorGUI.BeginChangeCheck();
                var newValue = EditorGUI.Toggle(rect, boolValue);
                if (EditorGUI.EndChangeCheck())
                {
                    // همیشه مقدار جدید را ذخیره کن
                    valueJsonProp.stringValue = newValue.ToString();
                    
                    // اگر قبلاً override نشده بود، حالا که تغییر دادیم، override را فعال کن
                    if (!isOverriddenProp.boolValue)
                    {
                        isOverriddenProp.boolValue = true;
                    }
                    // اگر قبلاً override شده بود، آن را true نگه دار
                    
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
            else if (fieldType.IsEnum || (fieldType.Namespace != null && fieldType.Namespace.StartsWith("DG.Tweening") && fieldType.Name == "Ease"))
            {
                var enumValue = currentValue != null ? (Enum)currentValue : (Enum)Enum.GetValues(fieldType).GetValue(0);
                EditorGUI.BeginChangeCheck();
                var newValue = EditorGUI.EnumPopup(rect, enumValue);
                if (EditorGUI.EndChangeCheck())
                {
                    // همیشه مقدار جدید را ذخیره کن
                    valueJsonProp.stringValue = newValue.ToString();
                    
                    // اگر قبلاً override نشده بود، حالا که تغییر دادیم، override را فعال کن
                    if (!isOverriddenProp.boolValue)
                    {
                        isOverriddenProp.boolValue = true;
                    }
                    // اگر قبلاً override شده بود، آن را true نگه دار
                    
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                EditorGUI.LabelField(rect, "Complex type - not supported");
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var enabledProp = property.FindPropertyRelative("enabled");
            if (!enabledProp.boolValue)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            var action = GetActionFromParent(property);
            if (action == null)
            {
                return EditorGUIUtility.singleLineHeight * 3;
            }

            var fields = ActionOverride.GetActionFields(action);
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // enabled toggle
            
            height += (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * fields.Count;

            return height;
        }
    }
}
#endif
