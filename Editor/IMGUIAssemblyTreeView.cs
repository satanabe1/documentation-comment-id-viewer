#nullable enable
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Dcid
{
    internal class IMGUIAssemblyTreeView : TreeView
    {
        // コンストラクタ
        public IMGUIAssemblyTreeView(TreeViewState state) : base(state, CreateHeader())
        {
            Reload();
        }

        private static MultiColumnHeader CreateHeader()
        {
            // MultiColumnHeaderState.Column型の配列を作成
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    // カラムのヘッダに表示する要素
                    headerContent = new GUIContent("Name"),
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Field1"),
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Field2"),
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Field3"),
                },
            };

            // 配列から MultiColumnHeaderState を構築
            var state = new MultiColumnHeaderState(columns);
            // State から MultiColumnHeader を構築
            return new MultiColumnHeader(state);
        }

        // ルート要素を構築して返す
        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };

            int id = 0;
            var items = new List<TreeViewItem>
            {
                new SampleTreeViewItem
                    { id = ++id, depth = 0, displayName = "1", Field1 = "1A", Field2 = "1B", Field3 = "1C" },
                new SampleTreeViewItem
                    { id = ++id, depth = 1, displayName = "1-1", Field1 = "1-1A", Field2 = "1-1B", Field3 = "1-1C" },
                new SampleTreeViewItem
                    { id = ++id, depth = 0, displayName = "2", Field1 = "2A", Field2 = "2B", Field3 = "2C" },
                new SampleTreeViewItem
                    { id = ++id, depth = 1, displayName = "2-1", Field1 = "2-1A", Field2 = "2-1B", Field3 = "2-1C" },
                new SampleTreeViewItem
                    { id = ++id, depth = 1, displayName = "2-2", Field1 = "2-2A", Field2 = "2-2B", Field3 = "2-2C" },
                new SampleTreeViewItem
                    { id = ++id, depth = 0, displayName = "3", Field1 = "3A", Field2 = "3B", Field3 = "3C" },
                new SampleTreeViewItem
                    { id = ++id, depth = 1, displayName = "3-1", Field1 = "3-1A", Field2 = "3-1B", Field3 = "3-1C" },
                new SampleTreeViewItem
                {
                    id = ++id, depth = 2, displayName = "3-1-1", Field1 = "3-1-1A", Field2 = "3-1-1B", Field3 = "3-1-1C"
                },
            };

            // 親子関係を登録
            SetupParentsAndChildrenFromDepths(root, items);

            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (SampleTreeViewItem)args.item;

            // 各列のフィールドを描画
            for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                var rect = args.GetCellRect(i);
                var columnIndex = args.GetColumn(i);

                switch (columnIndex)
                {
                    case 0:
                        // 1列目は要素名
                        // インデントする必要がある
                        rect.xMin += GetContentIndent(item);
                        EditorGUI.LabelField(rect, item.displayName);
                        break;
                    case 1:
                        // 2列目以降はフィールド
                        EditorGUI.LabelField(rect, item.Field1);
                        break;
                    case 2:
                        EditorGUI.LabelField(rect, item.Field2);
                        break;
                    case 3:
                        EditorGUI.LabelField(rect, item.Field3);
                        break;
                    default:
                        break;
                }
            }
        }

        // 追加の情報を持たせるため、TreeViewItemを継承したクラスを作成
        private class SampleTreeViewItem : TreeViewItem
        {
            public string? Field1 { get; set; }
            public string? Field2 { get; set; }
            public string? Field3 { get; set; }
        }
    }
}