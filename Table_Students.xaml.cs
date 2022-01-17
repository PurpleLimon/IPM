using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Проекты_8_9_Классы.res;

namespace Проекты_8_9_Классы
{



    /// <summary>
    /// Логика взаимодействия для Table_Students.xaml
    /// </summary>
    public partial class Table_Students : UserControl
    {
        public static readonly DependencyProperty TableContentHeightDependencyProperty = DependencyProperty.Register("TableContentHeight", typeof(double), typeof(Table_Students));

        private double Mapping(double value, double start1, double stop1, double start2, double stop2)
        {
            return start2 + (stop2 - start2) * ((value - start1) / (stop1 - start1));
        }
        private double verticalScrollbarOffset = 0.0;

        #region Public Properties

        public double TableContentHeight
        {
            get
            {
                return Convert.ToDouble(GetValue(TableContentHeightDependencyProperty));
            }

            set
            {
                SetValue(TableContentHeightDependencyProperty, value);
            }
        }
        public StudentStruct.Student SelectedStudent = null;
        #endregion

        public Table_Students()
        {
            InitializeComponent();
            //(new TableStudentElement()).IsSelectedDependecyProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(TableStudentElement));
            SizeChanged += delegate
            {
                RecalculateElementsMeasures();
            };

        }

        public double CalculateElementWidth()
        {
            return this.Width - 33;
        }

        public void RecalculateElementsMeasures()
        {
            foreach (FrameworkElement element in ItemBase.Items)
            {
                element.Width = CalculateElementWidth();
            }
        }

        private void scrollViewer_DragDelta(object sender, DragDeltaEventArgs e)
        {
            //Move the Thumb to the mouse position during the drag operation
            verticalScrollbarOffset += e.VerticalChange;
            (sender as ScrollViewer).ScrollToVerticalOffset(verticalScrollbarOffset);
            //Thickness margin= .Margin;
            //margin.Top += e.VerticalChange;
            //(sender as Thumb).Margin = margin;

        }
    }
}