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
        private readonly CancellationTokenSource disposalTokenSource = new CancellationTokenSource();

        public MainWindow()
        {
            InitializeComponent();
            this.joinableTaskCollection = this.joinableTaskContext.CreateCollection();
            this.joinableTaskFactory = this.joinableTaskContext.CreateFactory(this.joinableTaskCollection);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            this.disposalTokenSource.Cancel();
            this.joinableTaskContext.Factory.Run(() => this.joinableTaskCollection.JoinTillEmptyAsync());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.joinableTaskFactory.RunAsync(async delegate
            {
                await TestAsync();
            });
        }

        private async Task TestAsync()
        {
            await Test2Async();
            await RecordUIThreadCheckAsync(Label3);
        }

        private async Task Test2Async()
        {
            await Task.Yield();
            await RecordUIThreadCheckAsync(Label1);

            await Task.Delay(100).ConfigureAwait(false); // also try 0
            await RecordUIThreadCheckAsync(Label2);
        }

        private async Task RecordUIThreadCheckAsync(Label label)
        {
            bool onUIThread = this.Dispatcher.Thread == Thread.CurrentThread;
            await this.joinableTaskFactory.SwitchToMainThreadAsync();
            label.Content = onUIThread;
        }

        private void StartLongProcess_Click(object sender, RoutedEventArgs e)
        {
            this.joinableTaskFactory.RunAsync(async delegate
            {
                StartLongProcess.IsEnabled = false;
                for (int i = 0; i <= 100; i++)
                {
                    this.disposalTokenSource.Token.ThrowIfCancellationRequested();
                    TaskProgress.Value = i;
                    await Task.Delay(50);
                }

                StartLongProcess.IsEnabled = true;
            });
        }
    }
}
