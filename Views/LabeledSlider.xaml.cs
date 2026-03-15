using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PaletteStudio.Views;

public sealed partial class LabeledSlider : UserControl
{
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(LabeledSlider), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(LabeledSlider),
            new PropertyMetadata(0.0, (d, _) => ((LabeledSlider)d).OnPropertyChanged(nameof(Value))));

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(LabeledSlider), new PropertyMetadata(0.0));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(LabeledSlider), new PropertyMetadata(100.0));

    public static readonly DependencyProperty AutomationNameProperty =
        DependencyProperty.Register(nameof(AutomationName), typeof(string), typeof(LabeledSlider), new PropertyMetadata(string.Empty));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public string AutomationName
    {
        get => (string)GetValue(AutomationNameProperty);
        set => SetValue(AutomationNameProperty, value);
    }

    private void OnPropertyChanged(string name) =>
        Bindings?.Update();

    public LabeledSlider()
    {
        InitializeComponent();
    }
}
