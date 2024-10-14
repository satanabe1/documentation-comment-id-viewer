using System;

namespace Dcid
{
    internal enum AssemblyTreeViewItemType
    {
        Assembly,
        Class,
        Member
    }

    [Flags]
    internal enum AssemblyTreeViewItemMemberType
    {
        // Class = 1 << 0,
        Method = 1 << 1,
        Property = 1 << 2,
        Field = 1 << 3,
        Other = 1 << 4,
        Everything = Method | Property | Field | Other
    }

    [Flags]
    internal enum AssemblyTreeViewItemDefineType
    {
        User = 1,
        Generated = 2,
        Everything = User | Generated
    }
}