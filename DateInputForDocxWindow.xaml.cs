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
using Проекты_8_9_Классы.res;
using IPM.res;

namespace Проекты_8_9_Классы
{
    /// <summary>
    /// Логика взаимодействия для DateInputForDocxWindow.xaml
    /// </summary>
    public partial class DateInputForDocxWindow : Window
    {

        public List<TextBox> TextBoxes { get; private set; } = new List<TextBox>();
        private TextBox SelectedTBox;
        private string ButtonStyleName = "PlusMinusBtn";
        private bool[] IsDatesReady = new bool[] { true, true };

        public DateInputForDocxWindow(string HeadTeacherName, string[] TeacherNames)
        {
            InitializeComponent();
            ChairmanName_TB.Focus();

            if (HeadTeacherName == "")
            {
                ChairmanName_TB.Text = (Application.Current as App).Settings.ChairmanName;
            }
            else
            {
                ChairmanName_TB.Text = HeadTeacherName;
            }
            if (TeacherNames.Length == 0)
            {
                if ((Application.Current as App).Settings.TeacherNames.Length == 0)
                    TeacherName_TB.Text = "";
                else
                    TeacherName_TB.Text = (Application.Current as App).Settings.TeacherNames[0];
            }
            else
            {
                TeacherName_TB.Text = TeacherNames[0];
            }

            TextBoxes.Add(TeacherName_TB);


            for (int i = 1; i < TeacherNames.Length; i++)
            {
                AddNewGridTeacherRow();
            }

            examinationDate_TB.Text = ((Application.Current.MainWindow as MainWindow).FsstFile.ExaminationDate.All(x => x == 0)) ?
                (Application.Current as App).Settings.ExamintionDate :
                    String.Join(".", (Application.Current.MainWindow as MainWindow).FsstFile.ExaminationDate);
            inputDate_TB.Text = ((Application.Current.MainWindow as MainWindow).FsstFile.InputDate.All(x => x == 0)) ?
                (Application.Current as App).Settings.InputDate :
                    String.Join(".", (Application.Current.MainWindow as MainWindow).FsstFile.InputDate);

            CreatePlusBtn(ButtonStyleName, 2, 0, ref FirstTeacher);



        }

        private void Tbox_GotFocus(object sender, RoutedEventArgs e)
        {
            SelectedTBox = sender as TextBox;
        }

        private void gridTBoxes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int index = TextBoxes.IndexOf(SelectedTBox) + 1;
                if (index >= TextBoxes.Count)
                {
                    examinationDate_TB.Focus();
                    SelectedTBox = null;
                }
                else
                {
                    TextBoxes[index].Focus();
                }

            }

        }

        private void DragWindow(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed &&
                !examinationDate_TB.IsMouseOver &&
                !inputDate_TB.IsMouseOver &&
                !TeacherName_TB.IsMouseOver &&
                !ChairmanName_TB.IsMouseOver &&
                !TextBoxes.Any((x) => x.IsMouseOver))
            {
                this.WindowState = WindowState.Normal;
                this.DragMove();

            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void examinationDate_TB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                inputDate_TB.Focus();
            }
        }

        private void inputDate_TB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter &&
                SaveBtn.IsEnabled)
            {
                this.DialogResult = true;
            }
        }

        private void DateTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((Application.Current.MainWindow as MainWindow).ValidateDate((sender as TextBox).Text.Split('.')).Any(x => ReferenceEquals(x, null)))
            {
                if ((sender as TextBox).Name == "examinationDate_TB")
                {
                    examBorder.Visibility = Visibility.Visible;
                    IsDatesReady[0] = false;
                }

                if ((sender as TextBox).Name == "inputDate_TB")
                {
                    inputBorder.Visibility = Visibility.Visible;
                    IsDatesReady[1] = false;
                }

            }
            else
            {
                if ((sender as TextBox).Name == "examinationDate_TB")
                {
                    examBorder.Visibility = Visibility.Hidden;
                    IsDatesReady[0] = true;
                }

                if ((sender as TextBox).Name == "inputDate_TB")
                {
                    inputBorder.Visibility = Visibility.Hidden;
                    IsDatesReady[1] = true;
                }
            }

            SaveBtn.IsEnabled = !(IsDatesReady.Any(x => !x) ||
                                TeacherName_TB.Text == "" ||
                                ChairmanName_TB.Text == "" ||
                                examinationDate_TB.Text == "" ||
                                inputDate_TB.Text == "" ||
                                TextBoxes.Any((x) => x.Text == ""));

        }

        private void TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveBtn.IsEnabled = !(IsDatesReady.Any(x => !x) ||
                                TeacherName_TB.Text == "" ||
                                ChairmanName_TB.Text == "" ||
                                examinationDate_TB.Text == "" ||
                                inputDate_TB.Text == "" ||
                                TextBoxes.Any((x) => x.Text == ""));
        }

        private void TeacherName_TB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (TextBoxes.Count == 0)
                {
                    examinationDate_TB.Focus();
                }
                else
                {
                    TextBoxes.First().Focus();
                }

            }
        }

        private void ChairmanName_TB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TeacherName_TB.Focus();
            }
        }

        private void ChairmanName_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TeacherName_TB.Text == "" ||
                ChairmanName_TB.Text == "" ||
                examinationDate_TB.Text == "" ||
                inputDate_TB.Text == "")
            {
                SaveBtn.IsEnabled = false;
            }
            else
            {
                SaveBtn.IsEnabled = true;
            }
        }

        private void CreatePlusBtn(string StyleName, int GridColumn, int GridRow, ref Grid grid)
        {
            var btn = new Add_DeleteButtons.PlusButton(StyleName, GridColumn, GridRow, ref grid);
            btn.Click += PlusBtnClick;
        }

        private void CreateMinusBtn(string StyleName, int GridColumn, int GridRow, ref Grid grid)
        {
            var minBtn = new Add_DeleteButtons.MinusButton(StyleName, GridColumn, GridRow, ref grid);
            minBtn.Click += MinusBtnClick;
        }

        private void PlusBtnClick(object sender, RoutedEventArgs e)
        {
            Grid gridLast = TeachersStackPanel.Children[TeachersStackPanel.Children.Count - 1] as Grid;

            CreateMinusBtn(ButtonStyleName, 2, 0, ref gridLast);

            Grid grid = AddNewGridTeacherRow();

            TeachersRowMain.Height = new GridLength(TeachersRowMain.Height.Value + 50);

            double newHeight = this.Height + 50;
            this.Height = newHeight;

            ((sender as Button).Parent as Grid).Children.Remove(sender as UIElement);

            grid.Children.Add(sender as UIElement);

            SaveBtn.IsEnabled = false;

        }

        private void MinusBtnClick(object sender, RoutedEventArgs e)
        {
            TeachersStackPanel.Children.Remove((sender as Button).Parent as UIElement);

            double newHeight = this.Height - 50;
            this.Height = newHeight;

            TeachersRowMain.Height = new GridLength(TeachersRowMain.Height.Value - 50);
        }

        private Grid AddNewGridTeacherRow()
        {
            Grid grid = new Grid();

            RowDefinition rd = new RowDefinition();
            rd.Height = new GridLength(50);
            grid.RowDefinitions.Add(rd);

            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(31, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(40, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(8, GridUnitType.Star) });

            TextBlock tb = new TextBlock();
            tb.Text = "Член комиссии";
            tb.SetValue(Grid.RowProperty, 0);
            tb.SetValue(Grid.ColumnProperty, 0);
            tb.HorizontalAlignment = HorizontalAlignment.Left;
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.TextWrapping = TextWrapping.Wrap;

            TextBox tbox = new TextBox();
            tbox.Tag = "Teacher_TB";
            tbox.SetValue(Grid.RowProperty, 0);
            tbox.SetValue(Grid.ColumnProperty, 1);
            tbox.Margin = new Thickness(0, 10, 0, 10);
            tbox.KeyDown += gridTBoxes_KeyDown;
            tbox.TextChanged += TB_TextChanged;
            tbox.GotFocus += Tbox_GotFocus;

            TextBoxes.Add(tbox);

            grid.Children.Add(tb);
            grid.Children.Add(tbox);

            TeachersStackPanel.Children.Add(grid);

            return grid;
        }
    }
}
