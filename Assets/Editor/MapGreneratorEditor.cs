using MapFactory;
using UnityEditor;
using UnityEngine;

namespace Editor
{
   [CustomEditor(typeof(IrregularMapFactory))]
   public class MapGeneratorEditor:UnityEditor.Editor
   {
      public override void OnInspectorGUI()
      {
         IrregularMapFactory mapAbstractGenerator = (IrregularMapFactory)target;
         base.DrawDefaultInspector();
         if (GUILayout.Button("Generate Map"))
         {
            if(Application.isPlaying)
               mapAbstractGenerator.CreateMap();
            else
            {
               Debug.LogWarning("请在运行模式下使用");
            }
         }

         if (GUILayout.Button("Clear Map"))
         {
            if(Application.isPlaying)
               mapAbstractGenerator.ClearMap();
            else
               Debug.LogWarning("请在运行模式下使用");
        }
      }
   }
}