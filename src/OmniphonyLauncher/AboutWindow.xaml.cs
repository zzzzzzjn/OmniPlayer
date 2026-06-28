using System.Windows;
using System.Windows.Input;

namespace OmniphonyLauncher;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        var version = typeof(AboutWindow).Assembly.GetName().Version?.ToString(3) ?? "1.4.0";
        VersionText.Text = string.Format(Application.Current.TryFindResource("AboutVersion")?.ToString() ?? "Version {0}", version);
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) Close();
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }
}
