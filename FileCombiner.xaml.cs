using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using Проекты_8_9_Классы.res;

namespace IPM
{
    /// <summary>
    /// Логика взаимодействия для FileCombiner.xaml
    /// </summary>
    public partial class FileCombiner : Window
    {
        #region Private

        private class SimpleStudent
        {
            public string FIO;
            public string Form;

            public SimpleStudent(string fio, string form)
            {
                FIO = fio;
                Form = form;
            }

            public override string ToString()
            {
                return FIO + " - " + Form;
            }
        }

        private class SpecialTextBlock: TextBlock
        {
            public SpecialTextBlock(string text) : base()
            {
                this.Text = text;
                this.FontSize = 12;
                TextWrapping = TextWrapping.Wrap;
                TextAlignment = TextAlignment.Left;
                Margin = new Thickness(5, 0, 0, 0);
            }
        }

        private class InfoRadioBtn : Grid
        {
            private string v;
            private CheckBox checkB;
            private TextBlock tb;
            
            public bool? IsChecked { get; private set; } = false;
            public event EventHandler<EventArgs> CheckedChange;
            public InfoRadioBtn(string v)
            {
                this.v = v;

                this.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(40)
                });

                this.ColumnDefinitions.Add(new ColumnDefinition());

                checkB = new CheckBox()
                {
                    Style = (Application.Current as App).FindResource("NormalCheckBox") as Style,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                checkB.Click += CheckB_Click;
                checkB.Content = new MahApps.Metro.IconPacks.PackIconMaterial()
                {
                    Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Close,
                    Foreground = (Application.Current as App).FindResource("HighLight") as Brush,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                };

                tb = new TextBlock()
                {
                    FontSize = 12,
                    Margin = new Thickness(3),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Text = this.v,
                    TextAlignment = TextAlignment.Left,
                    TextWrapping = TextWrapping.Wrap
                };
                tb.SetValue(Grid.ColumnProperty, 1);

                this.Children.Add(checkB);
                this.Children.Add(tb);

            }

            protected override void OnRender(DrawingContext dc)
            {
                base.OnRender(dc);
            }
            protected virtual void OnCheckedChange(EventArgs e)
            {
                CheckedChange?.Invoke(this, e);
            }

            private void CheckB_Click(object sender, RoutedEventArgs e)
            {
                this.IsChecked = checkB.IsChecked;
                if (this.IsChecked == true)
                {
                    checkB.Content = new MahApps.Metro.IconPacks.PackIconMaterial()
                    {
                        Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Check,
                        Foreground = new SolidColorBrush(Colors.White),
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                    };
                }
                else
                {
                    checkB.Content = new MahApps.Metro.IconPacks.PackIconMaterial()
                    {
                        Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Close,
                        Foreground = (Application.Current as App).FindResource("HighLight") as Brush,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                    };
                }

                OnCheckedChange(new EventArgs());
            }
        }

        private TextBlock selectedTBlock;
        private InfoRadioBtn studentInvalibilityRB;
        private bool studentsAllowness;

        #endregion

        public List<TextBlock> TextBlocks { get; private set; } = new List<TextBlock>();

        public List<FSSTFilePresenter.FSSTFile> FSSTFiles = new List<FSSTFilePresenter.FSSTFile>();

        public FileCombiner()
        {
            InitializeComponent();
            ChairmanName_TB.Focus();
            ChairmanName_TB.Text = (Application.Current as App).Settings.ChairmanName;


            examinationDate_TB.Text = ((Application.Current.MainWindow as MainWindow).FsstFile.ExaminationDate.All(x => x == 0)) ?
                (Application.Current as App).Settings.ExamintionDate :
                    String.Join(".", (Application.Current.MainWindow as MainWindow).FsstFile.ExaminationDate);
            inputDate_TB.Text = ((Application.Current.MainWindow as MainWindow).FsstFile.InputDate.All(x => x == 0)) ?
                (Application.Current as App).Settings.InputDate :
                    String.Join(".", (Application.Current.MainWindow as MainWindow).FsstFile.InputDate);
        }

        public FSSTFilePresenter.FSSTFile CombineFiles(FSSTFilePresenter.FSSTFile[] files)
        {
            List<StudentStruct.Student[]> studentsMatrix = new List<StudentStruct.Student[]>();
            HashSet<string> teacherNames = new HashSet<string>();

            foreach (var file in files)
            {
                studentsMatrix.Add(file.students.OrderBy(x => x.FIO).ToArray());
                file.TeacherNames.ForEach(x => teacherNames.Add(x));
            }

            List<StudentStruct.Student> individualStudentsInRow = studentsMatrix.ToArray().IndividualStudentsFromMatrix().ToList();

            List<int[]> studentsMarks = new List<int[]>(individualStudentsInRow.Count);

            studentsMarks.ForEach(x => x = new int[individualStudentsInRow.First().IndividualCriteriaScores.Length + 1]);

            for (int i = 0; i < studentsMatrix.Count; i++)
            {
                for (int j = 0; j < studentsMatrix[i].Length; j++)
                {
                    int index = individualStudentsInRow.IndexOf(studentsMatrix[i][j]);
                    studentsMarks[index].SumWith(studentsMatrix[i][j].IndividualCriteriaScores, 1, 0);
                    studentsMarks[index][0]++;
                }
            }

            for (int i = 0; i < individualStudentsInRow.Count; i++)
            {
                individualStudentsInRow[i].IndividualCriteriaScores = Extensions.DivideAllBy(studentsMarks[i].Skip(1).ToArray(), studentsMarks[i][0]);
            }

            FSSTFilePresenter.FSSTFile newfile = new FSSTFilePresenter.FSSTFile(FSSTFilePresenter.FsstFileType.Combined,
                                                                                files.First().CriteriaAmount,
                                                                                individualStudentsInRow.ToArray(),
                                                                                files.First().HeadTeacherName,
                                                                                teacherNames.ToList(),
                                                                                string.Join(".", files.First().ExaminationDate),
                                                                                string.Join(".", files.First().InputDate));

            return newfile;
        }

        private void Tbox_GotFocus(object sender, RoutedEventArgs e)
        {
            selectedTBlock = sender as TextBlock;
        }

        private void gridTBoxes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int index = TextBlocks.IndexOf(selectedTBlock) + 1;
                if (index >= TextBlocks.Count)
                {
                    examinationDate_TB.Focus();
                    selectedTBlock = null;
                }
                else
                {
                    TextBlocks[index].Focus();
                }

            }

        }

        private void DragWindow(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed &&
                !examinationDate_TB.IsMouseOver &&
                !inputDate_TB.IsMouseOver &&
                !ChairmanName_TB.IsMouseOver &&
                !TextBlocks.Any((x) => x.IsMouseOver))
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

        private bool IsSaveBtnReady()
        {
            return !(ChairmanName_TB.Text == "" ||
                    TextBlocks.Any((x) => x.Text == "") ||
                    examinationDate_TB.Text == "" ||
                    inputDate_TB.Text == "" &&
                    !studentsAllowness);
        }

        private void TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveBtn.IsEnabled = IsSaveBtnReady();
        }

        private void ChairmanName_TB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FirstAddBtn.Focus();
            }
        }

        private void GridAddNewFileRow(string text, UIElement Content)
        {
            Grid FilesGrid = new Grid
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

            FilesGrid.ColumnDefinitions.Add(cl1);
            FilesGrid.ColumnDefinitions.Add(cl2);
            FilesGrid.ColumnDefinitions.Add(cl3);

            TextBlock tb = new TextBlock();
            tb.Text = "Документ №" + (TextBlocks.Count + 1);
            tb.SetValue(Grid.ColumnProperty, 0);
            tb.HorizontalAlignment = HorizontalAlignment.Left;
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.TextWrapping = TextWrapping.Wrap;

            Grid ContentGrid = new Grid();
            ContentGrid.RowDefinitions.Add(new RowDefinition());
            

            TextBlock tbox = new TextBlock();
            tbox.SetValue(Grid.ColumnProperty, 1);
            tbox.Margin = new Thickness(0, 10, 0, 10);
            tbox.KeyDown += gridTBoxes_KeyDown;
            tbox.GotFocus += Tbox_GotFocus;
            tbox.Text = text;

            if (Content != null)
            {
                ContentGrid.RowDefinitions.Add(new RowDefinition());

                Content.SetValue(Grid.RowProperty, 1);

                ContentGrid.Children.Add(Content);
            }
           

            ContentGrid.Children.Add(tbox);
            

            Button plusBtn = CreateBasicMinusBtn();

            TextBlocks.Add(tbox);

            FilesGrid.Children.Add(tb);
            FilesGrid.Children.Add(ContentGrid);
            FilesGrid.Children.Add(plusBtn);

            FilesStackPanel.Children.Insert(FilesStackPanel.Children.Count - 1, FilesGrid);

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

        private void MinusBtn_Click(object sender, RoutedEventArgs e)
        {
            int index = FilesStackPanel.Children.IndexOf((sender as Button).Parent as UIElement);

            TextBlocks.RemoveAt(index);
            FSSTFiles.RemoveAt(index);
            FilesStackPanel.Children.Remove((sender as Button).Parent as UIElement);

            this.MinHeight -= 50;
            this.Height -= 50;

            FilesValidation();

        }

        private void PlusBtnFunction()
        {

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "FSST File (*.fsst)|*.fsst";
            openFileDialog.InitialDirectory = @"c:\temp\";
            openFileDialog.Title = "Открыть";

            if (openFileDialog.ShowDialog() == true)
            {
                FSSTFiles.Add(FSSTFilePresenter.BytesToFSSTFile(File.ReadAllBytes(openFileDialog.FileName)));

                (FilesStackPanel.Children[FilesStackPanel.Children.Count - 1] as Grid).Children.RemoveAt(2);

                Button minusBtn = CreateBasicMinusBtn();
                minusBtn.SetValue(Grid.ColumnProperty, 2);
                (FilesStackPanel.Children[FilesStackPanel.Children.Count - 1] as Grid).Children.Add(minusBtn);

                GridAddNewFileRow(openFileDialog.SafeFileName, null);

                this.Height += 50;
                this.MinHeight += 50;
            }

            
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "FSST File (*.fsst)|*.fsst";
            openFileDialog.InitialDirectory = @"c:\temp\";
            openFileDialog.Title = "Открыть";

            if (openFileDialog.ShowDialog() != true)
                return;

            FSSTFilePresenter.FSSTFile file = FSSTFilePresenter.BytesToFSSTFile(File.ReadAllBytes(openFileDialog.FileName));
            
#region Первичная проверка файлов
            
            if (TextBlocks.Any(x => x.Text == openFileDialog.SafeFileName))
            {
                MessageBox.Show("Выбранный файл уже есть в числе загруженных", "Внимание", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (FSSTFiles.Count > 0)
            {
                if (FSSTFiles.First().ExaminationDate.SequenceEqual(file.ExaminationDate) == false)
                {
                    if (!FSSTFiles.First().InputDate.SequenceEqual(file.InputDate))
                    {
                        MessageBox.Show("Даты выбранного файла не совпадают с датами ранее выбранного(-ых) файла(-ов)", "Внимание", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        return;
                    }

                    MessageBox.Show("Дата проведения экзамена в выбранном файле не совпадает с датой ранее выбранного(-ых) файла(-ов)", "Внимание", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                if (FSSTFiles.First().InputDate.SequenceEqual(file.InputDate) == false)
                {
                    MessageBox.Show("Дата внесения в протокол оценок в выбранном файле не совпадает с датой ранее выбранного(-ых) файла(-ов)", "Внимание", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
            }

#endregion

            FSSTFiles.Add(file);

            GridAddNewFileRow(openFileDialog.SafeFileName, null);

            FilesValidation();

            this.Height += 50;
            this.MinHeight += 50;

        }

        private IEnumerable<SimpleStudent> StudentsSimplification(IEnumerable<StudentStruct.Student> students)
        {
            List<SimpleStudent> simpleStudents = new List<SimpleStudent>();
            foreach (var student in students)
            {
                simpleStudents.Add(new SimpleStudent(student.FIO, student.Form));
            }

            return simpleStudents;
        }

        private void FilesValidation()
        {
            DocsInfo_StackPanel.Children.Clear();
            List<IEnumerable<StudentStruct.Student>> allStudents = new List<IEnumerable<StudentStruct.Student>>();
            foreach (var file in FSSTFiles)
            {
                allStudents.Add(file.students);
            }

            IEnumerable<SimpleStudent> extraStudents = StudentsSimplification(GetExtraStudents(allStudents));

            if (extraStudents.Count() > 0)
            {
                DocsInfo_StackPanel.Children.Add(new Label()
                {
                    Content = "Список учащихся не совпадает:",
                    FontSize = 13
                });

                foreach (var student in extraStudents)
                {
                    TextBlock tb = new SpecialTextBlock(student.ToString());
                    DocsInfo_StackPanel.Children.Add(tb);
                }

                studentInvalibilityRB = new InfoRadioBtn("Список учащихся в выбранных документах не совпадает, продолжить?");
                studentInvalibilityRB.CheckedChange += StudentInvalibilityRB_CheckedChange;

                DocsInfo_StackPanel.Children.Add(studentInvalibilityRB as Grid);
                studentsAllowness = false;
            }
            else
            {
                if (studentInvalibilityRB != null)
                {
                    DocsInfo_StackPanel.Children.Remove(studentInvalibilityRB);
                    studentInvalibilityRB = null;
                    studentsAllowness = true;
                }
            }

            if (FSSTFiles.Count > 0)
            {
                examinationDate_TB.Text = string.Join(".", FSSTFiles[0].ExaminationDate);
                inputDate_TB.Text = string.Join(".", FSSTFiles[0].InputDate);
            }
            else
            {
                examinationDate_TB.Text = "";
                inputDate_TB.Text = "";
            }

            SaveBtn.IsEnabled = IsSaveBtnReady();

        }

        private void StudentInvalibilityRB_CheckedChange(object sender, EventArgs e)
        {
            studentsAllowness = (sender as InfoRadioBtn).IsChecked == true;

            SaveBtn.IsEnabled = IsSaveBtnReady();
        }

        private IEnumerable<StudentStruct.Student> GetExtraStudents(IEnumerable<IEnumerable<StudentStruct.Student>> allStudents)
        {
            HashSet<StudentStruct.Student> AllUnic = new HashSet<StudentStruct.Student>();
            HashSet<StudentStruct.Student> ExtraStudents = new HashSet<StudentStruct.Student>();
            foreach (var students in allStudents)
            {
                foreach (var student in students)
                {
                    AllUnic.Add(student);
                }
            }

            foreach (var students in allStudents)
            {
                foreach (var student in AllUnic.Except(students))
                {
                    ExtraStudents.Add(student);
                }
                
            }

            return ExtraStudents;

        }

        
    }
}
