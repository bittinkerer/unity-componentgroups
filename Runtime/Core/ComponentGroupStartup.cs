using UnityEditor;

namespace Packages.Estenis.ComponentGroups_
{
#if UNITY_EDITOR
    [InitializeOnLoad]
    public static class Startup
    {
        static Startup()
        {
            if (!EditorApplication.isPlaying)
            {
                ObjectNamesUtility.SetTitleForType($"[GROUP]", typeof(ComponentGroup));
            }
        }
    }
#endif
}