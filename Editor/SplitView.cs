#nullable enable
using UnityEngine.UIElements;

namespace Dcid
{
#if UNITY_2023_2_OR_NEWER
    [UxmlElement]
#endif
    internal partial class SplitView : TwoPaneSplitView
    {
#if !UNITY_2023_2_OR_NEWER
        public new class UxmlFactory : UxmlFactory<SplitView, UxmlTraits>
        {
        }
#endif
    }
}