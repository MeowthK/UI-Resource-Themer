using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UI_Resource_Themer
{
    /// <summary>
    /// Interaction logic for LoadingDialog.xaml
    /// </summary>
    public partial class LoadingDialog : Window
    {
        public ProgressBar Progress { get => LoadingBar; }

        public LoadingDialog()
        {
            InitializeComponent();
        }

        public void SetProgress(double value)
        {
            if (Progress == null)
                return;

            if (value >= Progress.Maximum)
                Hide();

            Progress.Dispatcher.Invoke(new Action(() => { Progress.Value = value; }));
        }
    }
}
