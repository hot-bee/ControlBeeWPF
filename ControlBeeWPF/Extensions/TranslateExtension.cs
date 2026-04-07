using System.Windows.Markup;
using ControlBee.Services;

namespace ControlBeeWPF.Extensions;

public class TranslateExtension(string key) : MarkupExtension
{
    public string Key { get; set; } = key;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var binding = new System.Windows.Data.Binding($"[{Key}]")
        {
            Source = LocalizationManager.Instance,
            Mode = System.Windows.Data.BindingMode.OneWay,
        };
        return binding.ProvideValue(serviceProvider);
    }
}
