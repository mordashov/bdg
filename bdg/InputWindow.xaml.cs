using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace bdg
{
    /// <summary>
    /// Логика взаимодействия для InputWindow.xaml
    /// </summary>
    public partial class InputWindow : Window
    {

        private bool _isОК = false;

        public InputWindow()
        {
            InitializeComponent();
        }

        public bool IsОК { get => _isОК; set => _isОК = value; }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            if (TextBoxInput.Text == "") return;
            _isОК = true;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Application curApp = Application.Current;
            Window mainWindow = curApp.MainWindow;
            this.Left = mainWindow.Left + (mainWindow.Width - this.ActualWidth) / 2;
            this.Top = mainWindow.Top + (mainWindow.Height - this.ActualHeight) / 2;
        }
    }
}
