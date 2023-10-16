using System.Collections.Generic;
using UnityEngine;

namespace Packages.Estenis.ComponentGroups_
{
    [DisallowMultipleComponent]
    public class ComponentGroupManager : MonoBehaviour
    {
        [SerializeField] private List<BaseComponentGroup> _componentGroups = new();
    }
}