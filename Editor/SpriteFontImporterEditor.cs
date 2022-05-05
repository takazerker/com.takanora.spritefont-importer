using UnityEngine;
using UnityEditor.AssetImporters;
using UnityEditor;

[CustomEditor(typeof(SpriteFontImporter))]
class SpriteFontImporterEditor : ScriptedImporterEditor
{
    public SpriteFontJson JsonData = new SpriteFontJson();
    SerializedObject mSerializedObject;
    bool mModified;

    public override bool showImportedObject => false;

    public override void OnEnable()
    {
        base.OnEnable();

        ResetValues();

        mSerializedObject = new SerializedObject(this);
    }

    public override bool HasModified()
    {
        return mModified;
    }

    protected override void ResetValues()
    {
        mModified = false;

        var importer = target as AssetImporter;
        EditorJsonUtility.FromJsonOverwrite(System.IO.File.ReadAllText(importer.assetPath), JsonData);
    }

    protected override void Apply()
    {
        var importer = target as AssetImporter;
        System.IO.File.WriteAllText(importer.assetPath, EditorJsonUtility.ToJson(JsonData, true));
        mModified = false;
    }

    public override void OnInspectorGUI()
    {
        hideFlags &= ~HideFlags.NotEditable;

        EditorGUI.BeginChangeCheck();

        mSerializedObject.Update();
        EditorGUILayout.PropertyField(mSerializedObject.FindProperty("JsonData.FontSize"));
        EditorGUILayout.PropertyField(mSerializedObject.FindProperty("JsonData.LineHeight"));
        EditorGUILayout.PropertyField(mSerializedObject.FindProperty("JsonData.BaseLine"));
        EditorGUILayout.PropertyField(mSerializedObject.FindProperty("JsonData.Descent"));
        EditorGUILayout.PropertyField(mSerializedObject.FindProperty("JsonData.Shader"));
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(mSerializedObject.FindProperty("JsonData.Characters"));
        mSerializedObject.ApplyModifiedProperties();

        if (EditorGUI.EndChangeCheck())
        {
            mModified = true;
        }

        ApplyRevertGUI();
    }
}

[CustomPropertyDrawer(typeof(SpriteFontJson.CharacterData))]
class CharacterDataDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var lineRect = position;
        lineRect.height = 18;

        var characterProperty = property.FindPropertyRelative("Character");
        var spriteProperty = property.FindPropertyRelative("Sprite");
        var xOffsetProperty = property.FindPropertyRelative("XOffset");
        var yOffsetProperty = property.FindPropertyRelative("YOffset");
        var xAdvanceProperty = property.FindPropertyRelative("Advance");

        var characterRect = lineRect;
        characterRect.width *= 0.15f;
        characterProperty.stringValue = EditorGUI.TextField(characterRect, characterProperty.stringValue);

        var spriteRect = lineRect;
        spriteRect.xMin = characterRect.xMax + 2;
        spriteProperty.objectReferenceValue = EditorGUI.ObjectField(spriteRect, spriteProperty.objectReferenceValue, typeof(Sprite), false);

        lineRect.y += 20;

        var labelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 30;

        var columnRect = lineRect;
        columnRect.width /= 3;

        xOffsetProperty.intValue = EditorGUI.IntField(columnRect, "BX", xOffsetProperty.intValue);
        columnRect.x += columnRect.width;

        yOffsetProperty.intValue = EditorGUI.IntField(OffsetRect(columnRect, 4), "BY", yOffsetProperty.intValue);
        columnRect.x += columnRect.width;

        xAdvanceProperty.intValue = EditorGUI.IntField(OffsetRect(columnRect, 4), "AX", xAdvanceProperty.intValue);

        EditorGUIUtility.labelWidth = labelWidth;
    }

    static Rect OffsetRect(Rect rect, float val)
    {
        rect.xMin += val;
        return rect;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 40;
    }
}
