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

namespace Проекты_8_9_Классы
{
    /// <summary>
    /// Логика взаимодействия для InfoWindow.xaml
    /// </summary>
    public partial class InfoWindow : Window
    {
        private double ScrollViewerMarginHeight;
        private double ScrollViewerMarginWidth;
        private double screenHeight = SystemParameters.FullPrimaryScreenHeight;
        private double screenWidth = SystemParameters.FullPrimaryScreenWidth;
        private enum FormState
        {
            Normal,
            Maximized,
            Minimized

        };

        private FormState formState;
        private Point NormalSize;
        private Point WindowLocationBeforeMaximaze;

        public static RoutedCommand Ctrl_C = new RoutedCommand();

        public InfoWindow()
        {
            InitializeComponent();
            WindowLocationBeforeMaximaze.X = (screenWidth - this.Width) / 2;
            WindowLocationBeforeMaximaze.Y = (screenHeight - this.Height) / 2;

            Ctrl_C.InputGestures.Add(new KeyGesture(Key.C, ModifierKeys.Control));

            scrollViewer.Loaded += delegate
            {
                ScrollViewerMarginHeight = ActualHeight - scrollViewer.ActualHeight;
                ScrollViewerMarginWidth = ActualWidth - scrollViewer.ActualWidth;
                scrollViewer.Height = ActualHeight - ScrollViewerMarginHeight - 70;
                scrollViewer.Width = ActualWidth - ScrollViewerMarginHeight - 70;
            };
            SizeChanged += delegate
            {
                scrollViewer.Height = ActualHeight - ScrollViewerMarginHeight - 70;
                scrollViewer.Width = ActualWidth - ScrollViewerMarginHeight - 70;
                if (formState != FormState.Maximized)
                {
                    NormalSize.X = ActualWidth;
                    NormalSize.Y = ActualHeight;
                }
            };
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void DragWindow(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed &&
                !scrollViewer.IsMouseOver)
            {
                this.DragMove();
                this.WindowState = WindowState.Normal;
            }

        }

        private void MaxBtn_Click(object sender, RoutedEventArgs e)
        {
            if (formState == FormState.Maximized)
            {

                this.Width = NormalSize.X;
                this.Height = NormalSize.Y;
                formState = FormState.Normal;

                this.Left = WindowLocationBeforeMaximaze.X;
                this.Top = WindowLocationBeforeMaximaze.Y;

            }
            else
            {
                formState = FormState.Maximized;
                WindowLocationBeforeMaximaze = new Point(this.Left, this.Top);

                this.Width = SystemParameters.WorkArea.Width;
                this.Height = SystemParameters.WorkArea.Height;
                this.Top = (screenHeight - this.Height) / 2 + 15;
                this.Left = (screenWidth - this.Width) / 2;
            }

        }

        private void CTRL_C_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (CopyrightTB.IsMouseOver)
            {
                Clipboard.SetText("kozyrev_lit@mail.ru");
            }
        }
    }
}
