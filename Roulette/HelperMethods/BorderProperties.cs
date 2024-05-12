using Avalonia;
using Avalonia.Controls;

namespace Roulette.HelperMethods;

public class BorderProperties {
    public static readonly AttachedProperty<string> NameProperty =
        AvaloniaProperty.RegisterAttached<Border, string>("Name", typeof(BorderProperties));
    public static string GetName(Border border) {return border.GetValue(NameProperty);}
    public static void SetName(Border border, string value) => border.SetValue(NameProperty, value);
}


