using System.Windows.Markup;
using ControlBee.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ControlBeeWPF.Extensions;

public class TranslateExtension(string key) : MarkupExtension
{
    public string Key { get; set; } = key;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return LocalizationManager.Instance.Translate(Key);
    }
}