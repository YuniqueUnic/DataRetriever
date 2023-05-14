using System.Windows;
using System.Windows.Controls;

namespace VSTSDataProvider.Common;

class TemplateSelectors
{
}

public class MainPageDataGridTemplateSelector : DataTemplateSelector
{
    public DataTemplate VSTSPageDataTemplate { get; set; }
    public DataTemplate EditPageDataTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item , DependencyObject container)
    {
        string? mode = item?.ToString();

        return mode switch
        {
            "VSTS" => VSTSPageDataTemplate,
            "ModeSwitch" => EditPageDataTemplate,
            _ => VSTSPageDataTemplate
        };
    }
}

public class DataGridTemplateSelector : DataTemplateSelector
{
    public DataTemplate DetailsTemplate { get; set; }
    public DataTemplate OTEsemplate { get; set; }

    public override DataTemplate SelectTemplate(object item , DependencyObject container)
    {
        string? mode = item?.ToString();

        return mode switch
        {
            "VSTS" => DetailsTemplate,
            "ModeSwitch" => OTEsemplate,
            _ => DetailsTemplate
        };
    }
}