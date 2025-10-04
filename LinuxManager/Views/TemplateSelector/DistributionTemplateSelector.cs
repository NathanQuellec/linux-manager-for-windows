using System.Reflection;
using LinuxManager.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace LinuxManager.Views.TemplateSelector;

public class DistributionTemplateSelector : DataTemplateSelector
{
    public DataTemplate DefaultTemplate
    {
        get; set;
    }
    public DataTemplate DistributionTemplate
    {
        get; set;
    }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        return item is Distribution ? DistributionTemplate : DefaultTemplate;
    }
}
