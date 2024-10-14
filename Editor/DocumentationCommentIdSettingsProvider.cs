#nullable enable
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dcid
{
    public class DocumentationCommentIdSettingsProvider : SettingsProvider
    {
        public static void OpenSetting() => SettingsService.OpenProjectSettings(SettingPath);

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
            => new DocumentationCommentIdSettingsProvider(SettingPath, SettingsScope.Project);

        private DocumentationCommentIdSettingsProvider(string path, SettingsScope scopes,
            IEnumerable<string>? keywords = null) : base(path, scopes, keywords)
        {
        }

        private const string SettingPath = "Project/DocumentationCommentId";
        private Editor? _cachedEditor;

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            var instance = DocumentationCommentIdSettings.instance;
            instance.hideFlags &= ~HideFlags.NotEditable;
        }

        public override void OnGUI(string searchContext)
        {
            base.OnGUI(searchContext);
            _cachedEditor = GetOrCreateEditor();
            EditorGUI.BeginChangeCheck();
            _cachedEditor.serializedObject.Update();
            _cachedEditor.OnInspectorGUI();
            _cachedEditor.serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck()) DocumentationCommentIdSettings.instance.Save();
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            if (_cachedEditor != null)
            {
                Object.DestroyImmediate(_cachedEditor, true);
                _cachedEditor = null;
            }
        }

        private Editor GetOrCreateEditor()
        {
            if (_cachedEditor != null && _cachedEditor.target != null) return _cachedEditor;
            // 設定表示中にアセットを削除された場合はEditorを作り直す
            if (_cachedEditor != null)
            {
                Object.DestroyImmediate(_cachedEditor, true);
                _cachedEditor = null;
            }

            var setting = DocumentationCommentIdSettings.instance;
            _cachedEditor = Editor.CreateEditorWithContext(new Object[] { setting }, setting);
            return _cachedEditor;
        }
    }
}