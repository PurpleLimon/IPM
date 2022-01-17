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
using Проекты_8_9_Классы;

namespace IPM
{
    /// <summary>
    /// Логика взаимодействия для PropertiesWindow.xaml
    /// </summary>
    public partial class PropertiesWindow : Window
    {
        public List<TextBox> TextBoxes { get; private set; } = new List<TextBox>();
        private TextBox SelectedTBox;
        bool[] datesReadiness = new bool[2];
        public PropertiesWindow(string HeadTeacherName, string[] TeacherNames)
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

            if (TeacherNames.Length > 0)
            {
                FirstTeacherName_TB.Text = TeacherNames[0];
            }
            else
            {
                FirstTeacherName_TB.Text = (Application.Current as App).Settings.TeacherNames[0];
            }
            TextBoxes.Add(FirstTeacherName_TB);

            for (int i = 1; i < TeacherNames.Length; i++)
            {
                PlusBtnFunction();
                ((TeachersStackPanel.Children[TeachersStackPanel.Children.Count - 1] as Grid).Children[1] as TextBox).Text = TeacherNames[i];
            }


            examinationDate_TB.Text = ((Application.Current.MainWindow as MainWindow).FsstFile.ExaminationDate.All(x => x == 0)) ?
                (Application.Current as App).Settings.ExamintionDate :
                    String.Join(".", (Application.Current.MainWindow as MainWindow).FsstFile.ExaminationDate);
            inputDate_TB.Text = ((Application.Current.MainWindow as MainWindow).FsstFile.InputDate.All(x => x == 0)) ?
                (Application.Current as App).Settings.InputDate :
                    String.Join(".", (Application.Current.MainWindow as MainWindow).FsstFile.InputDate);
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
                !FirstTeacherName_TB.IsMouseOver &&
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

        private bool IsSaveBtnReady()
        {
            return !(ChairmanName_TB.Text == "" ||
                    TextBoxes.Any((x) => x.Text == "") ||
                    examinationDate_TB.Text == "" ||
                    inputDate_TB.Text == "")
                    && datesReadiness.All((x) => x);
        }

        private void TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveBtn.IsEnabled = IsSaveBtnReady();
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
                FirstTeacherName_TB.Focus();
            }
        }

        private void ChairmanName_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxes.Any(x => x.Text == "") ||
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

        private void GridAddNewTeacherRow(string text)
        {
            Grid TeachersGrid = new Grid
            {
                Height = 50
            };

            ColumnDefinition cl1 = new ColumnDefinition
            {
                Width = new GridLength(31, GridUnitType.Star)
            };
            ColumnDefinition cl2 = new ColumnDefinition
            {
                Width = new GridLength(40, GridUnitType.Star)
            };
            ColumnDefinition cl3 = new ColumnDefinition
            {
                Width = new GridLength(8, GridUnitType.Star)
            };

            TeachersGrid.ColumnDefinitions.Add(cl1);
            TeachersGrid.ColumnDefinitions.Add(cl2);
            TeachersGrid.ColumnDefinitions.Add(cl3);

            TextBlock tb = new TextBlock();
            tb.Text = "Член комиссии";
            tb.SetValue(Grid.ColumnProperty, 0);
            tb.HorizontalAlignment = HorizontalAlignment.Left;
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.TextWrapping = TextWrapping.Wrap;

            TextBox tbox = new TextBox();
            tbox.SetValue(Grid.ColumnProperty, 1);
            tbox.Margin = new Thickness(0, 10, 0, 10);
            tbox.KeyDown += gridTBoxes_KeyDown;
            tbox.GotFocus += Tbox_GotFocus;
            tbox.TextChanged += TB_TextChanged;
            tbox.Text = text;

            Button plusBtn = CreateBasicPlusBtn();

            TextBoxes.Add(tbox);

            TeachersGrid.Children.Add(tb);
            TeachersGrid.Children.Add(tbox);
            TeachersGrid.Children.Add(plusBtn);

            TeachersStackPanel.Children.Add(TeachersGrid);

        }

        private Button CreateBasicBtn()
        {
            Button Btn = new Button();
            Btn.Content = "-";
            Btn.Style = (Application.Current as App).FindResource("PlusMinusBtn") as Style;
            Btn.Margin = new Thickness(10, 0, 10, 0);
            Btn.Height = 25;
            Btn.FontSize = 10;
            Btn.SetValue(Grid.ColumnProperty, 2);

            return Btn;
        }

        private Button CreateBasicMinusBtn()
        {
            Button minusBtn = CreateBasicBtn();
            minusBtn.Click += MinusBtn_Click;
            minusBtn.Content = "-";
            return minusBtn;
        }

        private Button CreateBasicPlusBtn()
        {
            Button plusBtn = CreateBasicBtn();
            plusBtn.Click += PlusButton_Click;
            plusBtn.Content = "+";
            return plusBtn;
        }

        private void MinusBtn_Click(object sender, RoutedEventArgs e)
        {
            TextBoxes.RemoveAt(TeachersStackPanel.Children.IndexOf((sender as Button).Parent as UIElement));
            TeachersStackPanel.Children.Remove((sender as Button).Parent as UIElement);

            this.MinHeight -= 50;
            this.Height -= 50;

            SaveBtn.IsEnabled = IsSaveBtnReady();

        }

        private void PlusBtnFunction()
        {
            (TeachersStackPanel.Children[TeachersStackPanel.Children.Count - 1] as Grid).Children.RemoveAt(2);

            Button minusBtn = CreateBasicMinusBtn();
            minusBtn.SetValue(Grid.ColumnProperty, 2);
            (TeachersStackPanel.Children[TeachersStackPanel.Children.Count - 1] as Grid).Children.Add(minusBtn);

            GridAddNewTeacherRow("");

            this.Height += 50;
            this.MinHeight += 50;

            SaveBtn.IsEnabled = false;
        }

        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            PlusBtnFunction();
        }

        private void examinationDate_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            //(Application.Current.MainWindow as MainWindow).ValidateDate(inputDate_TB.Text.Split('.')).All((x) => x != null);
            bool readiness = examinationDate_TB.Text == "" || (Application.Current.MainWindow as MainWindow).ValidateDate(examinationDate_TB.Text.Split('.')).All((x) => x != null);
            if (!readiness)
                examBorder.Visibility = Visibility.Visible;
            else
                examBorder.Visibility = Visibility.Hidden;

            datesReadiness[0] = readiness;
            SaveBtn.IsEnabled = datesReadiness.All((x) => x);

        }

        private void inputDate_TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool readiness = inputDate_TB.Text == "" || (Application.Current.MainWindow as MainWindow).ValidateDate(inputDate_TB.Text.Split('.')).All((x) => x != null);
            if (!readiness)
                inputBorder.Visibility = Visibility.Visible;
            else
                inputBorder.Visibility = Visibility.Hidden;

            datesReadiness[1] = readiness;
            SaveBtn.IsEnabled = IsSaveBtnReady();
        }
    }
}
