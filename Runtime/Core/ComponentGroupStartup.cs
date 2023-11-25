using UnityEditor;

namespace Packages.Estenis.ComponentGroups_
{
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
}