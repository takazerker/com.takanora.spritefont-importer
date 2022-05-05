using System.Collections.Generic;
using UnityEngine;
using UnityEditor.AssetImporters;
using UnityEditor;

[System.Serializable]
class SpriteFontJson
{
    [System.Serializable]
    public struct CharacterData
    {
        public string Character;
        public Sprite Sprite;
        public int XOffset;
        public int YOffset;
        public int Advance;
    }

    public float FontSize;
    public int BaseLine;
    public int LineHeight;
    public int Descent;
    public Shader Shader;
    public CharacterData[] Characters = new CharacterData[0];
}

[ScriptedImporter(1, "spritefont")]
class SpriteFontImporter : ScriptedImporter
{
    [MenuItem("Assets/Create/Sprite Font")]
    static void OnCreteAsset()
    {
        var json = EditorJsonUtility.ToJson(new SpriteFontJson(), true);

        ProjectWindowUtil.CreateAssetWithContent("New Sprite Font.spritefont", json);
    }

    public override void OnImportAsset(AssetImportContext ctx)
    {
        var json = new SpriteFontJson();
        EditorJsonUtility.FromJsonOverwrite(System.IO.File.ReadAllText(ctx.assetPath), json);

        if (json.Shader == null)
        {
            json.Shader = Shader.Find("GUI/Text Shader");
        }

        var font = new Font();
        var fontMaterial = new Material(json.Shader);

        font.material = fontMaterial;
        fontMaterial.hideFlags = HideFlags.HideInHierarchy;

        var characters = new List<CharacterInfo>();
        Texture2D fontTexture = null;

        for (int i = 0; i < json.Characters.Length; ++i)
        {
            ref var ch = ref json.Characters[i];

            if (ch.Sprite == null || string.IsNullOrEmpty(ch.Character))
            {
                continue;
            }

            if (fontTexture == null && ch.Sprite.texture != null)
            {
                fontTexture = ch.Sprite.texture;
                fontMaterial.mainTexture = fontTexture;
            }

            if (fontTexture == null)
            {
                continue;
            }

            var rect = ch.Sprite.rect;

            var ci = new CharacterInfo();
            ci.index = ch.Character[0];
            ci.glyphHeight = (int)rect.height;
            ci.glyphWidth = (int)rect.width;
            ci.advance = (int)(rect.width + ch.Advance);
            ci.minX = ch.XOffset;
            ci.minY = (int)(-rect.height + json.BaseLine + ch.YOffset);
            ci.maxX = (int)(ch.XOffset + rect.width);
            ci.maxY = (int)(ch.YOffset + json.BaseLine);
            ci.uvBottomLeft = new Vector2(rect.xMin / fontTexture.width, rect.yMin / fontTexture.height);
            ci.uvBottomRight = new Vector2(rect.xMax / fontTexture.width, rect.yMin / fontTexture.height);
            ci.uvTopLeft = new Vector2(rect.xMin / fontTexture.width, rect.yMax / fontTexture.height);
            ci.uvTopRight = new Vector2(rect.xMax / fontTexture.width, rect.yMax / fontTexture.height);

            characters.Add(ci);
        }

        font.characterInfo = characters.ToArray();

        foreach (var ch in json.Characters)
        {
            var path = AssetDatabase.GetAssetPath(ch.Sprite);
            if (!string.IsNullOrEmpty(path))
            {
                ctx.DependsOnArtifact(path);
            }
        }

        using (var fontObj = new SerializedObject(font))
        {
            fontObj.FindProperty("m_FontSize").floatValue = json.FontSize;
            fontObj.FindProperty("m_LineSpacing").floatValue = json.LineHeight;
            fontObj.FindProperty("m_Descent").floatValue = json.Descent;
            fontObj.ApplyModifiedPropertiesWithoutUndo();
        }

        ctx.AddObjectToAsset("Font", font);
        ctx.AddObjectToAsset("Material", fontMaterial);
    }
}
