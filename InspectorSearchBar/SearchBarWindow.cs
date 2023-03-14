/*using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SearchBarWindow : EditorWindow
{
    private Vector2 _scrollPosition;
    private List<Type> _componentTypes;
    private HashSet<Type> _selectedComponentTypes;

    [MenuItem("Window/Search Bar")]
    public static void ShowWindow()
    {
        GetWindow<SearchBarWindow>("Search Bar");
    }

    private void OnEnable()
    {
        // Get all component types in the project
        _componentTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(Component).IsAssignableFrom(type))
            .ToList();

        // Initialize selected component types
        _selectedComponentTypes = new HashSet<Type>();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Select Components", EditorStyles.boldLabel);

        // Scroll view for component list
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        foreach (Type componentType in _componentTypes)
        {
            bool isSelected = _selectedComponentTypes.Contains(componentType);
            bool newIsSelected = EditorGUILayout.ToggleLeft(componentType.Name, isSelected);

            if (newIsSelected != isSelected)
            {
                if (newIsSelected)
                {
                    _selectedComponentTypes.Add(componentType);
                }
                else
                {
                    _selectedComponentTypes.Remove(componentType);
                }
            }
        }

        EditorGUILayout.EndScrollView();

        // Generate script button
        if (GUILayout.Button("Generate Script"))
        {
            GenerateScript();
        }
    }

    private void GenerateScript()
    {
         // Generate script code
         /*string scriptCode = "using UnityEditor;\n\n";

         foreach (Type componentType in _selectedComponentTypes)
         {
             string editorClassName = $"{componentType.Name}Editor";
             scriptCode += $"[CustomEditor(typeof({componentType.FullName}), true)]\n";
             scriptCode += $"public class {editorClassName} : SearchBarEditor {{}}\n\n";
         }#1#

         // Save script to file
         /*string filePath = EditorUtility.SaveFilePanelInProject("Save Script", "SearchBarEditors", "cs", "Please enter a file name to save the script to");
         if (!string.IsNullOrEmpty(filePath))
         {
             System.IO.File.WriteAllText(filePath, scriptCode);
             AssetDatabase.Refresh();
         }#1#
         string scriptName = "InspectorSearchBarSettings.cs";
         string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
         string directoryPath = System.IO.Path.GetDirectoryName(scriptPath);

        // Generate script code
         string scriptCode = "using UnityEditor;\n\n";

         foreach (Type componentType in _selectedComponentTypes)
         {
             string editorClassName = $"{componentType.Name}Editor";

             // Check if custom editor class already exists
             Type existingEditorType = AppDomain.CurrentDomain.GetAssemblies()
                 .SelectMany(assembly => assembly.GetTypes())
                 .FirstOrDefault(type => type.Name == editorClassName);

             if (existingEditorType == null)
             {
                 // Generate new custom editor class
                 scriptCode += $"[CustomEditor(typeof({componentType.FullName}), true)]\n";
                 scriptCode += $"public class {editorClassName} : SearchBarEditor {{}}\n\n";
             }
         }

        // Save script to file
         string filePath = System.IO.Path.Combine(directoryPath, scriptName);
         System.IO.File.WriteAllText(filePath, scriptCode);
         AssetDatabase.Refresh();
     }
}*/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SearchBarWindow : EditorWindow
{
    private Vector2 _scrollPosition;
    private List<Type> _componentTypes;
    private HashSet<Type> _selectedComponentTypes;

    [MenuItem("Window/Search Bar")]
    public static void ShowWindow()
    {
        GetWindow<SearchBarWindow>("Search Bar");
    }

    private void OnEnable()
    {
        // Get all component types in the project
        _componentTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(Component).IsAssignableFrom(type))
            .ToList();

        // Initialize selected component types
        _selectedComponentTypes = new HashSet<Type>();
    }

private string _searchText = "";

private void OnGUI()
{
    EditorGUILayout.LabelField("Select Components", EditorStyles.boldLabel);

    // Dictionary to store parent component types and their subtypes
    Dictionary<Type, List<Type>> componentSubtypes = new Dictionary<Type, List<Type>>();

    // Iterate over all component types
    foreach (Type componentType in _componentTypes)
    {
        // Check if the component type is a subtype of any other component type
        Type parentType = _componentTypes.FirstOrDefault(t => t != componentType && t.IsAssignableFrom(componentType));

        // If it is a subtype, add it as a subitem to the parent component type
        if (parentType != null)
        {
            if (!componentSubtypes.ContainsKey(parentType))
            {
                componentSubtypes[parentType] = new List<Type>();
            }

            componentSubtypes[parentType].Add(componentType);
        }
        else // Otherwise, display it as a regular component type
        {
            if (string.IsNullOrEmpty(_searchText) || componentType.Name.ToLower().Contains(_searchText.ToLower()))
            {
                bool isSelected = _selectedComponentTypes.Contains(componentType);
                bool newIsSelected = EditorGUILayout.ToggleLeft(componentType.Name, isSelected);

                if (newIsSelected != isSelected)
                {
                    if (newIsSelected)
                    {
                        _selectedComponentTypes.Add(componentType);
                    }
                    else
                    {
                        _selectedComponentTypes.Remove(componentType);
                    }
                }
            }
        }
    }

    // Display parent component types and their subtypes as foldout lists
    _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

    foreach (KeyValuePair<Type, List<Type>> pair in componentSubtypes)
    {
        bool isExpanded = EditorPrefs.GetBool(pair.Key.FullName, true);

        GUIContent dropdownContent = new GUIContent(pair.Key.Name, $"{pair.Value.Count} items");
        isExpanded = EditorGUILayout.Foldout(isExpanded, dropdownContent);

        if (isExpanded)
        {
            EditorGUI.indentLevel++;

            foreach (Type subtype in pair.Value)
            {
                if (string.IsNullOrEmpty(_searchText) || subtype.Name.ToLower().Contains(_searchText.ToLower()))
                {
                    bool isSelected = _selectedComponentTypes.Contains(subtype);
                    bool newIsSelected = EditorGUILayout.ToggleLeft(subtype.Name, isSelected);

                    if (newIsSelected != isSelected)
                    {
                        if (newIsSelected)
                        {
                            _selectedComponentTypes.Add(subtype);
                        }
                        else
                        {
                            _selectedComponentTypes.Remove(subtype);
                        }
                    }
                }
            }

            EditorGUI.indentLevel--;
        }

        EditorPrefs.SetBool(pair.Key.FullName, isExpanded);
    }

    EditorGUILayout.EndScrollView();

    // Search bar
    EditorGUILayout.BeginHorizontal();
    _searchText = EditorGUILayout.TextField("Search", _searchText);
    if (GUILayout.Button("X", GUILayout.Width(20)))
    {
        _searchText = "";
    }
    EditorGUILayout.EndHorizontal();

    // Generate script button
    if (GUILayout.Button("Save Settings"))
    {
        GenerateScript();
    }
}

private void GenerateScript()
    {
         string scriptName = "InspectorSearchBarSettings.cs";
         string scriptPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
         string directoryPath = System.IO.Path.GetDirectoryName(scriptPath);

        // Generate script code
         string scriptCode = "using UnityEditor;\n\n";

         foreach (Type componentType in _selectedComponentTypes)
         {
             string editorClassName = $"{componentType.Name}Editor";

             // Check if custom editor class already exists
             Type existingEditorType = AppDomain.CurrentDomain.GetAssemblies()
                 .SelectMany(assembly => assembly.GetTypes())
                 .FirstOrDefault(type => type.Name == editorClassName);

             if (existingEditorType == null)
             {
                 // Generate new custom editor class
                 scriptCode += $"[CustomEditor(typeof({componentType.FullName}), true)]\n";
                 scriptCode += $"public class {editorClassName} : SearchBarEditor {{}}\n\n";
             }
         }

        // Save script to file
         string filePath = System.IO.Path.Combine(directoryPath, scriptName);
         System.IO.File.WriteAllText(filePath, scriptCode);
         AssetDatabase.Refresh();
     }
}