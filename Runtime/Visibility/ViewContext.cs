

using System.Collections.Generic;

public sealed class ViewContext
{

    static ViewContext()
    {
        _viewStatesTable = new Dictionary<ViewMode, IViewStrategy>
        {
            { ViewMode.GROUP, new GroupViewStrategy() },
            { ViewMode.OTHERS, new OthersViewStrategy() },
            { ViewMode.ALL, new AllViewStrategy() },
        };

    }

    private static Dictionary<ViewMode, IViewStrategy> _viewStatesTable;

    public static IViewStrategy GetView(ViewMode mode)
        => _viewStatesTable[mode];

    
}