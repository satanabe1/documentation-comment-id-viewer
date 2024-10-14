#nullable enable
using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Dcid
{
    internal class IMGUIDocumentationIdViewer : EditorWindow
    {
        // メニューバーからウィンドウを開けるようにする
        [MenuItem("Tools/Documentation ID Viewer")]
        public static void Open() => GetWindow<IMGUIDocumentationIdViewer>();

        // TreeViewの状態を保持できるようにするための情報
        private TreeViewState? _treeViewState;

        // TreeViewのインスタンス
        private IMGUIAssemblyTreeView? _treeView;

        // 検索窓のインスタンス
        private SearchField? _searchField;

        private void GetAsm()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assemblyData = DocumentationCommentIdUtility.GetDocumentationCommentIdData(assemblies);
            foreach (var data in assemblyData)
            {
                Debug.Log("<b>" + data + "</b>");
                foreach (var classData in data.classes)
                {
                    Debug.Log(classData);
                }
            }
        }

        private void OnGUI()
        {
            if (GUILayout.Button("DO"))
            {
                GetAsm();
            }

            // 値がnullの場合は初期化
            _treeViewState ??= new TreeViewState();
            _treeView ??= new IMGUIAssemblyTreeView(_treeViewState);
            _searchField ??= new SearchField();

            // 検索窓の領域を取得
            var searchRect = EditorGUILayout.GetControlRect(false, GUILayout.ExpandWidth(true),
                GUILayout.Height(EditorGUIUtility.singleLineHeight));
            // 検索窓を描画
            _treeView.searchString = _searchField.OnGUI(searchRect, _treeView.searchString);

            // ウィンドウ全体の領域を取得
            var treeViewRect =
                EditorGUILayout.GetControlRect(false, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            // TreeViewを描画
            _treeView.OnGUI(treeViewRect);
        }
    }
}