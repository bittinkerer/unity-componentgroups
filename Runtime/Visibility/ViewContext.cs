

using System.Collections.Generic;

public sealed class ViewContext
{

    public ViewContext()
    {
        _viewStatesTable = new Dictionary<ViewMode, IViewState>
        {
            { ViewMode.GROUP, new GroupViewState() },
            { ViewMode.OTHERS, new OthersViewState() },
            { ViewMode.ALL, new AllViewState() },
        };

    }

    private Dictionary<ViewMode, IViewState> _viewStatesTable;

    public IViewState GetView(ViewMode mode)
        => _viewStatesTable[mode];

    
}