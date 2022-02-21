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

namespace GT10ConnectProgramm
{
    /// <summary>
    /// Window1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoadingScreen : Window
    {
        [System.ComponentModel.Description("LoadingErrorEvent")]
        public event RoutedEventHandler LoadingError;

        [System.ComponentModel.Description("LoadingCancelEvent")]
        public event RoutedEventHandler LoadingCancel;
        public LoadingScreen()
        {
            InitializeComponent();
        }

    }
}
