using System.Windows;
using TimedSleep.ViewModels;

namespace TimedSleep;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
