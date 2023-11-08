using Packages.Estenis.ComponentGroups_;
using System.Collections.Generic;
using UnityEngine;

public interface IViewStrategy
{
    void ShowGOComponents(ComponentGroup group);
    void UpdateGOComponentsOnDiffs(List<ListDifference<Component>> diffs);

}