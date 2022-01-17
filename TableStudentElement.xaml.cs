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
using Проекты_8_9_Классы.res;

namespace Проекты_8_9_Классы
{
    /// <summary>
    /// Логика взаимодействия для TableStudentElement.xaml
    /// </summary>
    public partial class TableStudentElement : UserControl
    {
        private static DependencyProperty IsSelectedDependecyProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(TableStudentElement), new PropertyMetadata(false));

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedDependecyProperty); }
            set { SetValue(IsSelectedDependecyProperty, value); }
        }
        public StudentStruct.Student thisStudent;
        public TableStudentElement()
        {
            InitializeComponent();
        }

        public void CreateTableStudentElement(StudentStruct.Student student, int index)
        {
            thisStudent = student;

            IDTextBlock.Text = index.ToString();
            FIOTextBlock.Text = student.FIO;
            FormTextBlock.Text = student.Form;
            ThemeTextBlock.Text = student.Theme;
            TutorTextBlock.Text = student.Tutor;
            byte ScoreSum = student.IndividualCriteriaScores.SumBytes();
            ScoreTextBlock.Text = ScoreSum.ToString();
            MarkTextBlock.Text = StudentStruct.CalculateMark(ScoreSum).ToString();

            SetGridColumnWidths(GetGridLengths());

        }

        private GridLength[] GetGridLengths()
        {
            List<GridLength> arr = new List<GridLength>();

            foreach (var item in (Application.Current.MainWindow as MainWindow).TableForStudents.Header.ColumnDefinitions)
            {
                arr.Add(item.Width);
            }

            return arr.ToArray();
        }

        private void SetGridColumnWidths(GridLength[] gridLengths)
        {
            int i = 0;

            foreach (var item in BaseGrid.ColumnDefinitions)
            {
                item.Width = gridLengths[i];
                i++;
            }

        }

        private void EditBtn_ContextMenu_Click(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = false;
            (Application.Current.MainWindow as MainWindow).OpenEditStudent();
        }

        private void DeleteBtn_ContextMenu_Click(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = false;
            (Application.Current.MainWindow as MainWindow).DeleteStudent();
        }

        private void IndivProtocolBtn_ContextMenu_Click(object sender, RoutedEventArgs e)
        {
            //(Application.Current.MainWindow as MainWindow).IndivProtocolShow();

            Protocol_popup.IsOpen = true;

        }

        private void ShowProtocol_Btn_Click(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = false;
            Protocol_popup.IsOpen = false;
            (Application.Current.MainWindow as MainWindow).IndivProtocolShow();
        }

        private void PrintProtocol_Btn_Click(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = false;
            Protocol_popup.IsOpen = false;
            (Application.Current.MainWindow as MainWindow).ExportIndivProtocol();
        }
    }
}
