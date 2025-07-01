using UnityEditor;
using UnityEngine;

namespace Editor
{
   [CustomEditor(typeof(MapGenerator))]
   public class MapGeneratorEditor:UnityEditor.Editor
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
}