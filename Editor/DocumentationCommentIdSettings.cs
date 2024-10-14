#nullable enable
using System;
using UnityEditor;

namespace Dcid
{
    [FilePath(SettingPath, FilePathAttribute.Location.ProjectFolder)]
    public class DocumentationCommentIdSettings : ScriptableSingleton<DocumentationCommentIdSettings>
    {
        private const string SettingPath = "ProjectSettings/DocumentationCommentIdSettings.asset";
        public string[] targetDllNameRegexes = Array.Empty<string>();

        public void Save() => Save(EditorSettings.serializationMode != SerializationMode.ForceBinary);

        [CustomEditor(typeof(DocumentationCommentIdSettings))]
        private class DocumentationCommentIdSettingsEditor : Editor
        {
        }
    }
}