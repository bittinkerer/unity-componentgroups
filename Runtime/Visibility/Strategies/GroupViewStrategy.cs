using Packages.Estenis.ComponentGroups_;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Packages.Estenis.UnityExts_;

public class GroupViewStrategy : IViewStrategy
{
    private string[] _groupExceptions = { "Transform", "ComponentFilter" };

    public void ShowGOComponents(ComponentGroup group)
    {
        var componentsInGONotInGroup = group.gameObject.GetComponents<Component>()
            .Where(c => !_groupExceptions.Any(s => s == c.GetType().Name)
                    && !group._components.Any(co => co._component == c)
                    && c != group
                    && (c && c != null));
        foreach (var component in componentsInGONotInGroup)
        {
            component.HideInInspector();
        }
        foreach (var componentData in group._components)
        {
            componentData._component.UnhideInInspector();
        }
    }

    public void UpdateGOComponentsOnDiffs(List<ListDifference<Component>> diffs)
    {
        foreach (var diff in diffs)
        {
            var component = diff.Item;
            if(diff.ListChangeType == ListChangeType.ADDED)
            {
                component.UnhideInInspector();
            }
            else if(diff.ListChangeType == ListChangeType.REMOVED)
            {
                component.HideInInspector();
            }
        }
    }
}