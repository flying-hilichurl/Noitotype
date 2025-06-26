using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor:Editor
{
   public override void OnInspectorGUI()
   {
      base.DrawDefaultInspector();
      if(GUILayout.Button("Generate Map"))
      {
         ((MapGenerator)target).GenerateMap();
      }

      if (GUILayout.Button("Cleat Map"))
      {
         ((MapGenerator)target).ClearMap();
      }
   }
}