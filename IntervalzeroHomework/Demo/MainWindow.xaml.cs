using Demo.CompositionRoot;
using System.Windows;

namespace Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var disposer = Locator.GetView(out var view);

            this.Content = view;

            //TODO: dispose
        }
    }
}
