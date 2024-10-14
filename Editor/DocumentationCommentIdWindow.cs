#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using TreeView = UnityEngine.UIElements.TreeView;

namespace Dcid
{
    public class DocumentationCommentIdWindow : EditorWindow
    {
        private static class UiId
        {
            public const string ReloadButton = "reload-button";
            public const string SettingButton = "setting-button";
            public const string IdTextArea = "id-text-area";
            public const string AssemblyTreeView = "assembly-tree-view";
            public const string ProgressBar = "progress-bar";
            public const string TreeSearchField = "tree-search-field";
            public const string MemberTypeFlags = "member-type-flags";
            public const string MemberDefineTypeFlags = "member-define-type-flags";
        }

        [SerializeField] private VisualTreeAsset _visualTreeAsset = default!;
        [SerializeField] private List<AssemblyTreeViewItem> _cachedTreeData = new();
        private bool _isReloading;

        [MenuItem("Tools/Documentation ID Viewer", false, 7000)]
        public static void ReopenWindow()
        {
            // foreach (var documentationCommentIdWindow in Resources.FindObjectsOfTypeAll<DocumentationCommentIdWindow>())
            // {
            //     documentationCommentIdWindow.Close();
            // }
            GetWindow<DocumentationCommentIdWindow>(
                ObjectNames.NicifyVariableName(nameof(DocumentationCommentIdWindow)));
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            _visualTreeAsset.CloneTree(root);
            SetProgress(0, 0);

            if (_cachedTreeData.Count > 0)
            {
                RebuildTreeView(_cachedTreeData);
            }

            RegisterEvents();
        }

        private void RegisterEvents()
        {
            rootVisualElement.Q<Button>(UiId.SettingButton).clicked += OpenSetting;
            rootVisualElement.Q<TextField>(UiId.IdTextArea);
            GetReloadButton().clicked += ReloadAssemblies;
            GetTreeSearchField().RegisterValueChangedCallback(_ => UpdateFilter());
            GetMemberTypeFlags().RegisterValueChangedCallback(_ => UpdateFilter());
            GetMemberDefineTypeFlags().RegisterValueChangedCallback(_ => UpdateFilter());
            GetAssemblyTreeView().selectionChanged += selection =>
            {
                var sb = new StringBuilder();
                foreach (var data in selection.OfType<AssemblyTreeViewItem>())
                {
                    sb.AppendLine(data.GetDocumentationCommentId());
                }

                GetIdTextArea().value = sb.ToString();
            };
        }

        private void UpdateFilter() => ThreadPool.QueueUserWorkItem(_ =>
        {
            var filterValue = GetTreeSearchField().value;

            var memberTypes = (AssemblyTreeViewItemMemberType)GetMemberTypeFlags().value;
            var memberDefineTypes = (AssemblyTreeViewItemDefineType)GetMemberDefineTypeFlags().value;

            if (string.IsNullOrEmpty(filterValue) &&
                memberTypes == AssemblyTreeViewItemMemberType.Everything &&
                memberDefineTypes == AssemblyTreeViewItemDefineType.Everything)
            {
                RebuildTreeAndScroll(_cachedTreeData);
                return;
            }

            var rootItems = _cachedTreeData
                .Where(x => x.IsMatch(memberTypes) && x.IsMatch(memberDefineTypes))
                .Where(x => string.IsNullOrWhiteSpace(filterValue) ||
                            x.ToString().Contains(filterValue, StringComparison.OrdinalIgnoreCase))
                .ToList();
            EditorApplication.delayCall += () => RebuildTreeAndScroll(rootItems);
        });

        private void RebuildTreeAndScroll(List<AssemblyTreeViewItem> rootItems)
        {
            var treeView = GetAssemblyTreeView();
            var lastSelected = treeView.selectedItem as AssemblyTreeViewItem;
            var beforeCount = treeView.itemsSource != null ? treeView.GetTreeCount() : 0;
            var afterCount = rootItems.Count;
            RebuildTreeView(rootItems, () =>
            {
                // 最後に選択されたもの付近までスクロールする
                var tv = GetAssemblyTreeView();
                if (beforeCount >= afterCount) return;
                tv.ExpandAll();
                if (lastSelected != null)
                {
                    tv.ScrollToItemById(lastSelected.id);
                }
            });
        }

        private TreeView GetAssemblyTreeView() => rootVisualElement.Q<TreeView>(UiId.AssemblyTreeView);
        private int GetTreeRootCount() => GetAssemblyTreeView().GetRootIds().Count();
        private Button GetReloadButton() => rootVisualElement.Q<Button>(UiId.ReloadButton);
        private TextField GetIdTextArea() => rootVisualElement.Q<TextField>(UiId.IdTextArea);

        private ToolbarSearchField GetTreeSearchField()
            => rootVisualElement.Q<ToolbarSearchField>(UiId.TreeSearchField);

        private EnumFlagsField GetMemberTypeFlags()
            => rootVisualElement.Q<EnumFlagsField>(UiId.MemberTypeFlags);

        private EnumFlagsField GetMemberDefineTypeFlags()
            => rootVisualElement.Q<EnumFlagsField>(UiId.MemberDefineTypeFlags);

        private static void OpenSetting() => DocumentationCommentIdSettingsProvider.OpenSetting();

        private void SetProgress(int current, int max)
        {
            var progressBar = rootVisualElement.Q<ProgressBar>(UiId.ProgressBar);
            progressBar.lowValue = 0;
            progressBar.highValue = Mathf.Max(max, 1);
            progressBar.SetValueWithoutNotify(current);
            progressBar.title = $"{current}/{max}";
        }

        private void ReloadAssemblies()
        {
            if (_isReloading) return;
            SetProgress(0, 0);
            var regexTexts = DocumentationCommentIdSettings.instance.targetDllNameRegexes;
            ThreadPool.QueueUserWorkItem(_ =>
            {
                var regexes = regexTexts.Select(x => new Regex(x, RegexOptions.Compiled)).ToArray();
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(x => regexes.Length == 0 || regexes.Any(regex => regex.IsMatch(x.GetName().Name)));
                _isReloading = true;
                CacheTreeData(assemblies);
                RebuildTreeView(_cachedTreeData, () =>
                {
                    var rootCount = GetTreeRootCount();
                    SetProgress(rootCount, rootCount);
                    _isReloading = false;
                });
            });
        }

        private void RebuildTreeView(List<AssemblyTreeViewItem> treeData, Action? onRebuild = null)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    var items = BuildTreeItems(treeData);
                    EditorApplication.delayCall += () =>
                    {
                        RebuildTreeView(items);
                        onRebuild?.Invoke();
                    };
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    onRebuild?.Invoke();
                }
            });
        }

        private void RebuildTreeView(IList<TreeViewItemData<AssemblyTreeViewItem>> items)
        {
            var treeView = GetAssemblyTreeView();
            treeView.SetRootItems(items);
            treeView.Rebuild();
        }

        private static IList<TreeViewItemData<AssemblyTreeViewItem>> BuildTreeItems(List<AssemblyTreeViewItem> treeData)
        {
            var idToItem = treeData.ToDictionary(x => x.id, x => new TreeViewItemData<AssemblyTreeViewItem>(x.id, x));

            var rootItems = new List<TreeViewItemData<AssemblyTreeViewItem>>();
            foreach (var item in idToItem.Values)
            {
                if (idToItem.TryGetValue(item.data.parentId, out var parentItem))
                {
                    var children = (IList<TreeViewItemData<AssemblyTreeViewItem>>)parentItem.children;
                    children.Add(item);
                }
                else
                {
                    rootItems.Add(item);
                }
            }

            return rootItems;
        }

        private void CacheTreeData(IEnumerable<Assembly> assemblies)
        {
            var asmArray = assemblies.ToArray();
            var data = DocumentationCommentIdUtility.GetDocumentationCommentIdData(asmArray);
            var id = 1;
            _cachedTreeData.Clear();
            foreach (var (asmData, i) in data.Select((x, i) => (x, i)))
            {
                id++;
                EditorApplication.delayCall += () => SetProgress(i, asmArray.Length);
                var root = new AssemblyTreeViewItem(id++, asmData);
                _cachedTreeData.Add(root);
                foreach (var c in asmData.classes)
                {
                    var classData = new AssemblyTreeViewItem(id++, c, root.id);
                    foreach (var m in c.GetMembers())
                    {
                        _cachedTreeData.Add(new AssemblyTreeViewItem(id++, m, classData.id, GetMemberType(m)));
                    }

                    _cachedTreeData.Add(classData);
                }
            }
        }

        private static AssemblyTreeViewItemMemberType GetMemberType(DocumentationCommentIdData data) => data.type switch
        {
            // DefinitionType.Class => AssemblyTreeViewItemMemberType.Class,
            DefinitionType.Method => AssemblyTreeViewItemMemberType.Method,
            DefinitionType.Property => AssemblyTreeViewItemMemberType.Property,
            DefinitionType.Field => AssemblyTreeViewItemMemberType.Field,
            _ => AssemblyTreeViewItemMemberType.Other
        };
    }
}