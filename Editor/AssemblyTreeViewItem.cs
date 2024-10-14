#nullable enable
using System;

namespace Dcid
{
    [Serializable]
    internal class AssemblyTreeViewItem
    {
        public AssemblyTreeViewItemType type;
        public int id;
        public int parentId;
        public AssemblyDocumentationCommentIdData? assemblyIdData;
        public ClassDocumentationCommentIdData? classIdData;
        public DocumentationCommentIdData? memberIdData;
        public AssemblyTreeViewItemMemberType memberType;

        private AssemblyTreeViewItem()
        {
        }

        private AssemblyTreeViewItem(int id, AssemblyTreeViewItemType type, int parentId)
        {
            this.id = id;
            this.type = type;
            this.parentId = parentId;
        }

        public AssemblyTreeViewItem(int id, AssemblyDocumentationCommentIdData? assemblyIdData)
            : this(id, AssemblyTreeViewItemType.Assembly, 0) => this.assemblyIdData = assemblyIdData;

        public AssemblyTreeViewItem(int id, ClassDocumentationCommentIdData? classIdData, int parentId)
            : this(id, AssemblyTreeViewItemType.Class, parentId) => this.classIdData = classIdData;

        public AssemblyTreeViewItem(
            int id, DocumentationCommentIdData? memberIdData, int parentId, AssemblyTreeViewItemMemberType memberType)
            : this(id, AssemblyTreeViewItemType.Member, parentId)
        {
            this.memberIdData = memberIdData;
            this.memberType = memberType;
        }

        public override string ToString() => type switch
        {
            AssemblyTreeViewItemType.Assembly => assemblyIdData?.assemblyData?.fullQualifiedName ?? string.Empty,
            AssemblyTreeViewItemType.Class => classIdData?.classData?.fullQualifiedName ?? string.Empty,
            AssemblyTreeViewItemType.Member => memberIdData?.minimallyQualifiedName ?? string.Empty,
            _ => base.ToString()
        };

        public string GetDocumentationCommentId() => type switch
        {
            AssemblyTreeViewItemType.Assembly => assemblyIdData?.assemblyData?.documentationCommentId ?? string.Empty,
            AssemblyTreeViewItemType.Class => classIdData?.classData?.documentationCommentId ?? string.Empty,
            AssemblyTreeViewItemType.Member => memberIdData?.documentationCommentId ?? string.Empty,
            _ => base.ToString()
        };

        public bool IsMatch(AssemblyTreeViewItemMemberType match)
        {
            if (match == AssemblyTreeViewItemMemberType.Everything) return true;
            if (match == 0) return false;
            return match.HasFlag(memberType);
        }

        public bool IsMatch(AssemblyTreeViewItemDefineType match)
        {
            if (match == AssemblyTreeViewItemDefineType.Everything) return true;
            if (match == 0) return false;
            var data = type switch
            {
                AssemblyTreeViewItemType.Assembly => assemblyIdData?.assemblyData,
                AssemblyTreeViewItemType.Class => classIdData?.classData,
                AssemblyTreeViewItemType.Member => memberIdData,
                _ => null
            };

            if (data == null) return false;
            return match.HasFlag(data.isAutoGenerated
                ? AssemblyTreeViewItemDefineType.Generated
                : AssemblyTreeViewItemDefineType.User);
        }
    }
}