using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualStudio.Threading;

namespace AsyncAndThreadingDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Have only 1 of these in the entire application!
        private readonly JoinableTaskContext joinableTaskContext = new JoinableTaskContext();

        private readonly JoinableTaskFactory joinableTaskFactory;
        private readonly JoinableTaskCollection joinableTaskCollection;

        public MainWindow()
        {
            InitializeComponent();
            this.joinableTaskCollection = this.joinableTaskContext.CreateCollection();
            this.joinableTaskFactory = this.joinableTaskContext.CreateFactory(this.joinableTaskCollection);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            this.joinableTaskContext.Factory.Run(() => this.joinableTaskCollection.JoinTillEmptyAsync());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TestAsync(); // fix me! There's a warning here.
        }

        private async Task TestAsync()
        {
            await Test2Async();
            RecordUIThreadCheck(Label3);
        }

        private async Task Test2Async()
        {
            await Task.Yield();
            RecordUIThreadCheck(Label1);

            await Task.Delay(100).ConfigureAwait(false); // also try 0
            RecordUIThreadCheck(Label2);
        }

        private void RecordUIThreadCheck(Label label)
        {
            bool onUIThread = this.Dispatcher.Thread == Thread.CurrentThread;
            var _ = Dispatcher.BeginInvoke(new Action(() => label.Content = onUIThread));
        }

        private void StartLongProcess_Click(object sender, RoutedEventArgs e)
        {
            this.joinableTaskFactory.RunAsync(async delegate
            {
                StartLongProcess.IsEnabled = false;
                for (int i = 0; i <= 100; i++)
                {
                    TaskProgress.Value = i;
                    await Task.Delay(50);
                }

                StartLongProcess.IsEnabled = true;
            });
        }
    }
}
