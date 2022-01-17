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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Проекты_8_9_Классы
{
    /// <summary>
    /// Логика взаимодействия для My_ScrollBar.xaml
    /// </summary>
    public partial class My_ScrollBar : UserControl
    {
        private static double defaultIncrement = 10;
        private double increment = defaultIncrement;
        public My_ScrollBar()
        {
            InitializeComponent();

        }



        private void ScrollBarScrolling_sliderMooving(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {

                Point sliderPlacePosition = ((UIElement)sender).TransformToAncestor(Application.Current.MainWindow).Transform(new Point(0, 0));
                double pos = e.GetPosition(Base).Y;
                if (pos <= Base.ActualHeight - Slider.ActualHeight - 2)
                {
                    Slider.SetValue(Canvas.TopProperty, pos);
                }

            }

        }

        private void ScrollingBtn_Pressed(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (((Button)sender).Name == "ButtonDown")
                {
                    double pos = (double)Slider.GetValue(Canvas.TopProperty) + increment;
                    if (pos <= Base.ActualHeight - Slider.ActualHeight - 2)
                    {
                        Slider.SetValue(Canvas.TopProperty, pos);
                        increment += 4;
                    }
                }
                if (((Button)sender).Name == "ButtonUp")
                {
                    double pos = (double)Slider.GetValue(Canvas.TopProperty) - increment;
                    if (pos >= Slider.ActualHeight - 2)
                    {
                        Slider.SetValue(Canvas.TopProperty, pos);
                        increment += 4;
                    }

                }
            }

        }

        private void ScrollingBtn_UnPressed(object sender, MouseEventArgs e)
        {
            if (((Button)sender).Name == "ButtonDown" || ((Button)sender).Name == "ButtonUp")
            {
                increment = defaultIncrement;
            }
        }
    }
}
