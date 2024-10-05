using UnityEditor;
using UnityEngine;

namespace Editor.Tools.FigmaImporter
{
    [CustomEditor(typeof(FigmaNodeImporter))]
    public class FigmaImporterEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Import"))
            {
                ((FigmaNodeImporter)target).Import();
            }
        }
    }
}