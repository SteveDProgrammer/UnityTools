using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MultiplayerARPG.GameData.Model.Playables;
using UnityEditor;
using UnityEngine;

using UnityEngine.AI;
using UnityEngine.U2D;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class SearchBarEditor : Editor
{
    private string _searchString = "";
    private bool _showSearchBar;
    [Tooltip("Won't work if Search String is empty")]
    private bool _hideMissingReferences = false;
    private SerializedPropertyType _selectedPropertyType = SerializedPropertyType.Generic;
    
    private static readonly Dictionary<System.Type, SerializedPropertyType> TypeToSerializedPropertyType = new()
    {
        {typeof(int), SerializedPropertyType.Integer},
        {typeof(bool), SerializedPropertyType.Boolean},
        {typeof(float), SerializedPropertyType.Float},
        {typeof(string), SerializedPropertyType.String},
        {typeof(Color), SerializedPropertyType.Color},
        {typeof(UnityEngine.Object), SerializedPropertyType.ObjectReference},
        {typeof(LayerMask), SerializedPropertyType.LayerMask},
        {typeof(Enum), SerializedPropertyType.Enum},
        {typeof(Vector2), SerializedPropertyType.Vector2},
        {typeof(Vector3), SerializedPropertyType.Vector3},
        {typeof(Vector4), SerializedPropertyType.Vector4},
        {typeof(Rect), SerializedPropertyType.Rect},
        {typeof(Array), SerializedPropertyType.ArraySize},
        {typeof(AnimationCurve), SerializedPropertyType.AnimationCurve},
        {typeof(Bounds), SerializedPropertyType.Bounds},
        {typeof(Gradient), SerializedPropertyType.Gradient},
        {typeof(Quaternion), SerializedPropertyType.Quaternion},
        {typeof(ExposedReference<>), SerializedPropertyType.ExposedReference},
        {typeof(Vector2Int), SerializedPropertyType.Vector2Int},
        {typeof(Vector3Int), SerializedPropertyType.Vector3Int},
        {typeof(RectInt), SerializedPropertyType.RectInt},
        {typeof(BoundsInt), SerializedPropertyType.BoundsInt}
    };

public override void OnInspectorGUI()
{
    _showSearchBar = EditorGUILayout.Toggle("Show Search Bar", _showSearchBar);
    if (_showSearchBar || _hideMissingReferences)
    {
        _hideMissingReferences = EditorGUILayout.Toggle("Hide all Missing References", _hideMissingReferences);
        GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
        _searchString = GUILayout.TextField(_searchString, GUI.skin.FindStyle("ToolbarSeachTextField"));
        if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
        {
            _searchString = "";
            GUI.FocusControl(null);
        }
        GUILayout.EndHorizontal();

        // Get the script component
        MonoBehaviour scriptComponent = (MonoBehaviour)target;

        // Get all fields in the script component
        FieldInfo[] fields = scriptComponent.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // Get all unique field types
        System.Type[] fieldTypes = fields.Select(field => field.FieldType).Distinct().ToArray();

        // Get all property types used by the script component and available in the dictionary
        List<SerializedPropertyType> availablePropertyTypes = new List<SerializedPropertyType>();
        foreach (FieldInfo field in fields)
        {
            System.Type fieldType = field.FieldType;
            SerializedPropertyType serializedPropertyType;
            if (TypeToSerializedPropertyType.TryGetValue(fieldType, out serializedPropertyType) && !availablePropertyTypes.Contains(serializedPropertyType))
            {
                availablePropertyTypes.Add(serializedPropertyType);
            }
        }

        // Create property type names and property types arrays for available property types
        string[] propertyTypeNames = new string[availablePropertyTypes.Count + 1];
        SerializedPropertyType[] propertyTypes = new SerializedPropertyType[availablePropertyTypes.Count + 1];
        propertyTypeNames[0] = "All";
        propertyTypes[0] = SerializedPropertyType.Generic;

        for (int i = 0; i < availablePropertyTypes.Count; i++)
        {
            propertyTypeNames[i + 1] = availablePropertyTypes[i].ToString();
            propertyTypes[i + 1] = availablePropertyTypes[i];
        }


        // Select property type
        int selectedIndex = System.Array.IndexOf(propertyTypes, _selectedPropertyType);
        selectedIndex = EditorGUILayout.Popup("Property Type", selectedIndex, propertyTypeNames);
        _selectedPropertyType = propertyTypes[selectedIndex];

        // Filter properties based on search string and selected property type
        if (!string.IsNullOrEmpty(_searchString) || _selectedPropertyType != SerializedPropertyType.Generic)
        {
            SerializedProperty property = serializedObject.GetIterator();
            while (property.NextVisible(true))
            {
                if ((string.IsNullOrEmpty(_searchString) || property.displayName.ToLower().Contains(_searchString.ToLower())) &&
                    (_selectedPropertyType == SerializedPropertyType.Generic || property.propertyType == _selectedPropertyType) &&
                    (!_hideMissingReferences || IsGameObjectReference(property)))
                {
                    if (property.isArray && property.propertyType != SerializedPropertyType.String)
                    {
                        EditorGUILayout.PropertyField(property, true);
                        EditorGUI.indentLevel++;
                        for (int i = 0; i < property.arraySize; i++)
                        {
                            SerializedProperty elementProperty = property.GetArrayElementAtIndex(i);
                            EditorGUILayout.PropertyField(elementProperty, true);
                        }
                        EditorGUI.indentLevel--;
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(property, true);
                    }
                }
            }
        }
        else
        {
            base.OnInspectorGUI();
            }
        }
        else
        {
            base.OnInspectorGUI();
        }

        serializedObject.ApplyModifiedProperties();
    }

    bool IsGameObjectReference(SerializedProperty property)
    {
        if (property.propertyType == SerializedPropertyType.ObjectReference)
        {
            Object obj = property.objectReferenceValue;
            if (obj == null) return false;
            if (obj is GameObject) return true;
            if (obj is Component) return true;
            if (obj is Material) return true;
            if (obj is Texture) return true;
            if (obj is Mesh) return true;
            if (obj is AnimationClip) return true;
            if (obj is AudioClip) return true;
            if (obj is Shader) return true;
            if (obj is Sprite) return true;
            if (obj is SceneAsset) return true;
            if (obj is NavMeshData) return true;
            if (obj is SpriteAtlas) return true;
            if (obj is RenderTexture) return true;
            if (obj is Canvas) return true;
            if (obj is CanvasRenderer) return true;
            if (obj is RectTransform) return true;
            if (obj is RawImage) return true;
            if (obj is Text) return true;
        }
        return false;
    }

}
[CustomEditor(typeof(PlayableCharacterModel))]
public class YourScriptEditor : SearchBarEditor
{
}