// Editor/ChatStepDataDrawer.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ChatStepData))]
public class ChatStepDataDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float h = 0;
        h += 60f;  // message textarea
        h += 6f;   // padding

        var conditions = property.FindPropertyRelative("conditions");
        h += EditorGUI.GetPropertyHeight(conditions, true);
        h += 6f;

        h += EditorGUIUtility.singleLineHeight;  // conditionOperator
        h += 6f;

        return h;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        float x = position.x;
        float y = position.y;
        float w = position.width;

        // ── TextArea cho message ──────────────────────────
        var msgProp = property.FindPropertyRelative("message");
        EditorGUI.LabelField(new Rect(x, y, w, EditorGUIUtility.singleLineHeight), "Message");
        y += EditorGUIUtility.singleLineHeight + 2f;

        msgProp.stringValue = EditorGUI.TextArea(
            new Rect(x, y, w, 58f),
            msgProp.stringValue
        );
        y += 60f + 6f;

        // ── Conditions list ───────────────────────────────
        var condProp = property.FindPropertyRelative("conditions");
        float condH = EditorGUI.GetPropertyHeight(condProp, true);
        EditorGUI.PropertyField(new Rect(x, y, w, condH), condProp, true);
        y += condH + 6f;

        // ── ConditionOperator ─────────────────────────────
        var opProp = property.FindPropertyRelative("conditionOperator");
        EditorGUI.PropertyField(
            new Rect(x, y, w, EditorGUIUtility.singleLineHeight),
            opProp
        );

        EditorGUI.EndProperty();
    }
}
#endif