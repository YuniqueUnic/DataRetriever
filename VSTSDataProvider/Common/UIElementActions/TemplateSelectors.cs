using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Controls;

namespace VSTSDataProvider.Common;

class TemplateSelectors
{
}

public class DataGridTemplateSelector : DataTemplateSelector
{
    public DataTemplate OTEsModelTemplate { get; set; }
    public DataTemplate TCseTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item , DependencyObject container)
    {
        if( item is ConcurrentBag<Models.OTE_OfflineModel> )
            return OTEsModelTemplate;
        else if( item is ConcurrentBag<Models.TestCase> )
            return TCseTemplate;
        else
            return base.SelectTemplate(item , container);
    }
}