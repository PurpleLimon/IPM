using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using IPM;

namespace Проекты_8_9_Классы
{
    public struct FileProperties
    {
        public string[] TeachersNames { get; private set; }
        public string HeadTeacherName { get; private set; }
        public string ExaminationDate { get; private set; }
        public string InputDate { get; private set; }

        public FileProperties(string headTeacherName, string[] teachersNames, string examinationDate, string inputDate)
        {
            HeadTeacherName = headTeacherName;
            TeachersNames = teachersNames;
            ExaminationDate = examinationDate;
            InputDate = inputDate;
        }



    }

    public class IsAllTextBoxesFilledProperty : INotifyPropertyChanged
    {
        private bool[] _textBoxesIfFieldsFilled = new bool[4];
        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsField
        {
            get { return _textBoxesIfFieldsFilled.All((x) => x); }
        }

        public bool this[int i]
        {

            set
            {
                if (i >= _textBoxesIfFieldsFilled.Length)
                {
                    throw new Exception("Выход за пределы массива");
                }

                _textBoxesIfFieldsFilled[i] = value;
                OnPropertyChanged();

            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }

    public class IsDataChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private bool isDataChanged;
        public IsDataChanged(bool val)
        {
            isDataChanged = val;
        }

        public void DataWasChanged()
        {
            isDataChanged = true;
            OnPropertyChanged();
        }

        public void DataSaved()
        {
            isDataChanged = false;
            OnPropertyChanged();
        }

        public bool IsChanged()
        {
            return isDataChanged;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static RoutedCommand SaveShortcut = new RoutedCommand();
        public static RoutedCommand NewShortcut = new RoutedCommand();
        public static RoutedCommand OpenShortcut = new RoutedCommand();
        public static RoutedCommand SaveAsShortcut = new RoutedCommand();
        public static RoutedCommand RevokeShortcut = new RoutedCommand();
        public static RoutedCommand ReturnShortcut = new RoutedCommand();

        public FSSTFilePresenter.FSSTFile FsstFile;
        public IsAllTextBoxesFilledProperty isAllTextBoxesFilledProperty = new IsAllTextBoxesFilledProperty();
        public SortingType sortingType;
        public bool IsExporting = false;

        private enum FormState
        {
            Normal,
            Maximized,
            Minimized

        };
        public enum SortingType
        {
            Null,
            Form,
            Students,
            Scores,
            Tutors
        }
        private enum CriteriaWindowWorkState
        {
            Null,
            AddState,
            EditState,
            ReadOnly
        }
        private enum ActionType
        {
            Null = 0b_0000_0000,
            AddStudent = 0b_0000_0001,
            EditStudent = 0b_0000_0010,
            DeleteStudent = 0b_0000_0100,
            FilePropertiesChanged = 0b_0001_0000
        }

        private Stack<ActionType> ActionTypesActionHistory = new Stack<ActionType>();
        private Stack<ActionType> ActionTypesTrashHistory = new Stack<ActionType>();
        private Stack<StudentStruct.Student> StudentsActionHistory = new Stack<StudentStruct.Student>();
        private Stack<StudentStruct.Student> StudentsTrashHistory = new Stack<StudentStruct.Student>();
        private Stack<FileProperties> FilePropertiesHistory = new Stack<FileProperties>();
        private Stack<FileProperties> FilePropertiesTrash = new Stack<FileProperties>();

        private FormState formState;
        private CriteriaWindowWorkState criteriaWindowWorkState;
        public FileProperties actualFileProps;

        private Point NormalSize;
        private Point criteriaTabelPosition;

        private double screenHeight = SystemParameters.FullPrimaryScreenHeight;
        private double screenWidth = SystemParameters.FullPrimaryScreenWidth;

        private int SelectedStudentIndex;
        public string WorkWith_FilePath;
        private IsDataChanged isDataChanged = new IsDataChanged(false);

        private Point WindowLocationBeforeMaximaze;


        public MainWindow()
        {

            InitializeComponent();


            WindowLocationBeforeMaximaze.X = (screenWidth - this.Width) / 2;
            WindowLocationBeforeMaximaze.Y = (screenHeight - this.Height) / 2;


            #region CodeBehind Staff

            FsstFile = new FSSTFilePresenter.FSSTFile();

            SaveShortcut.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            NewShortcut.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
            OpenShortcut.InputGestures.Add(new KeyGesture(Key.F, ModifierKeys.Control));
            SaveAsShortcut.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift));
            RevokeShortcut.InputGestures.Add(new KeyGesture(Key.Z, ModifierKeys.Control));
            ReturnShortcut.InputGestures.Add(new KeyGesture(Key.Z, ModifierKeys.Shift | ModifierKeys.Control));

            //FsstFile = FSSTFilePresenter.LoadFile(@"DataFile.fsst.txt");
            //WriteToFileData();
            //FsstFile = FSSTFilePresenter.BytesToFSSTFile(File.ReadAllBytes("1.fsst"));
            #endregion

            Loaded += delegate
            {
                NormalSize.X = ActualWidth;
                NormalSize.Y = ActualHeight;

                criteriaTable.criteriaScores = CriteriaScores.CreateCriteriaScores(CriteriaTable_PropertyChanged);

                actualFileProps = new FileProperties((FsstFile?.HeadTeacherName == "") ? (Application.Current as App).Settings.ChairmanName : FsstFile.HeadTeacherName,
                    (FsstFile?.TeacherAmount == 0) ? (Application.Current as App).Settings.TeacherNames : FsstFile.TeacherNames.ToArray(),
                    (FsstFile == null || FsstFile.ExaminationDate.Any(x => x == 0)) ? (Application.Current as App).Settings.ExamintionDate : string.Join(".", FsstFile.ExaminationDate),
                    (FsstFile == null || FsstFile.InputDate.Any(x => x == 0)) ? (Application.Current as App).Settings.InputDate : string.Join(".", FsstFile.InputDate));

            };

            SizeChanged += delegate
            {
                if (formState != FormState.Maximized)
                {
                    NormalSize.X = ActualWidth;
                    NormalSize.Y = ActualHeight;
                }
                criteriaTabelPosition = TableForStudents.TransformToAncestor(this).Transform(new Point(0, 0));
                criteriaTable.Height = 385 + (NormalSize.Y - 700);
            };

            isDataChanged.PropertyChanged += (sender, e) =>
            {
                SaveFile_PopupBtns.IsEnabled = isDataChanged.IsChanged();
                if (isDataChanged.IsChanged())
                {
                    FileName_TextBlock.Text = "*" + FileName_TextBlock.Text.Replace("*", "");
                }
                else
                {
                    FileName_TextBlock.Text = FileName_TextBlock.Text.Replace("*", "");
                }

            };

            isAllTextBoxesFilledProperty.PropertyChanged += delegate
            {
                SaveBtnReadiness();
            };

            TableForStudents.Loaded += delegate
            {
                if (FsstFile.students != null)
                {
                    FillTableStudents();
                }

            };

        }

        public MainWindow(string filePath) : this()
        {
            FsstFile = FSSTFilePresenter.BytesToFSSTFile(File.ReadAllBytes(filePath));
            if (FsstFile == null)
            {
                Application.Current.Shutdown();
                return;
            }

            WorkWith_FilePath = filePath;
            FileName_TextBlock.Text = filePath.Split('\\').Last();
            isDataChanged.DataSaved();
            FillTableStudents();
        }

        #region Student: Edit, Delete, Add

        public void DeleteStudent()
        {

            if (MessageBox.Show("Вы уверены что хотите удалить этот элемент?", "Удалить", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                TableForStudents.ItemBase.Items.Remove(TableForStudents.ItemBase.Items[SelectedStudentIndex]);

                StudentStruct.Student deletedStudent = FsstFile.students[SelectedStudentIndex];

                var p = FsstFile.students.ToList();
                p.Remove(deletedStudent);
                FsstFile.students = p.ToArray();

                FillTableStudents();

                isDataChanged.DataWasChanged();
                SelectedStudentIndex = -1;

                AddNewActionToHistory(ActionType.DeleteStudent, deletedStudent);

            }

        }

        public void OpenEditStudent()
        {
            AllStudentsTable.Visibility = Visibility.Collapsed;
            StudentEditingWindow.Visibility = Visibility.Visible;
            Add_Btn.IsEnabled = false;
            Edit_Btn.IsEnabled = false;
            File_Btn.IsEnabled = false;
            DeleteStudentBtn.IsEnabled = false;

            criteriaWindowWorkState = CriteriaWindowWorkState.EditState;

            StudentFIO_TB.Text = TableForStudents.SelectedStudent.FIO;
            StudentForm_TB.Text = TableForStudents.SelectedStudent.Form;
            StudentProgectName_TB.Text = TableForStudents.SelectedStudent.Theme;
            TutorFIO_TB.Text = TableForStudents.SelectedStudent.Tutor;

            int i = 0;
            foreach (FrameworkElement item in criteriaTable.ItemBase.Children)
            {
                if ((item as CriteriaTableElement) != null)
                {

                    ((item as CriteriaTableElement).RadioButtonStackPanel.Children[TableForStudents.SelectedStudent.IndividualCriteriaScores[i] - 1] as RadioButton).IsChecked = true;
                    i++;
                }
            }

            ScoreSum_TB.Text = "Сумма баллов: " + criteriaTable.criteriaScores.Scores.SumBytes().ToString();

        }

        //--------Edit and AddNew students--------\\
        public void AddStudent()
        {
            var fio = StudentFIO_TB.Text.Split(' ').ToList();
            for (int i = 0; i < fio.Count; i++)
            {
                if (fio[i].Length == 0)
                {
                    fio.RemoveAt(i);
                    i--;
                    continue;
                }
                fio[i] = fio[i][0].ToString().ToUpper() + string.Join("", fio[i].Skip(1));
            }

            var tutor = TutorFIO_TB.Text.Split(' ').ToList();
            for (int i = 0; i < tutor.Count; i++)
            {
                if (tutor[i].Length == 0)
                {
                    tutor.RemoveAt(i);
                    i--;
                    continue;
                }
                tutor[i] = tutor[i][0].ToString().ToUpper() + string.Join("", tutor[i].Skip(1));
            }

            if (criteriaWindowWorkState == CriteriaWindowWorkState.AddState)
            {
                byte[] arr = new byte[criteriaTable.criteriaScores.Scores.Length];
                criteriaTable.criteriaScores.Scores.CopyTo(arr, 0);

                if (FsstFile.students == null || FsstFile.StudentAmount == 0)
                {
                    StudentStruct.Student newStudent = new StudentStruct.Student(
                        0,
                        string.Join(" ", fio),
                        StudentForm_TB.Text,
                        StudentProgectName_TB.Text.ToUpper()[0] + string.Join("", StudentProgectName_TB.Text.Skip(1)),
                        string.Join(" ", tutor),
                        arr,
                        DateTime.Today.ToString("dd.MM.yyyy")
                    );
                    FsstFile.students = new StudentStruct.Student[] { newStudent };

                    AddNewActionToHistory(ActionType.AddStudent, newStudent);

                }
                else
                {
                    StudentStruct.Student newStudent = new StudentStruct.Student(
                        Convert.ToByte(Convert.ToInt32(FsstFile.students.Last().ID) + 1),
                        string.Join(" ", fio),
                        StudentForm_TB.Text,
                        StudentProgectName_TB.Text.ToUpper()[0] + string.Join("", StudentProgectName_TB.Text.Skip(1)),
                        string.Join(" ", tutor),
                        arr,
                        DateTime.Today.ToString("dd.MM.yyyy")
                    );


                    AddNewActionToHistory(ActionType.AddStudent, newStudent);

                    List<StudentStruct.Student> students = FsstFile.students.ToList();
                    students.Add(newStudent);
                    FsstFile.students = students.ToArray();
                }


            }
            else if (criteriaWindowWorkState == CriteriaWindowWorkState.EditState)
            {
                byte[] arr = new byte[criteriaTable.criteriaScores.Scores.Length];
                criteriaTable.criteriaScores.Scores.CopyTo(arr, 0);

                StudentStruct.Student newStudent = new StudentStruct.Student(
                    TableForStudents.SelectedStudent.ID,
                    string.Join(" ", fio),
                    StudentForm_TB.Text,
                    StudentProgectName_TB.Text.ToUpper()[0] + string.Join("", StudentProgectName_TB.Text.Skip(1)),
                    string.Join(" ", tutor),
                    arr,
                    DateTime.Today.ToString("dd.MM.yyyy")
                    );

                AddNewActionToHistory(ActionType.EditStudent, FsstFile.students[SelectedStudentIndex]);

                FsstFile.students[SelectedStudentIndex] = newStudent;
            }

            FillTableStudents();

            TableForStudents.RecalculateElementsMeasures();

            ClearAllTextBoxes();
            ClearAllCriteria();

            AllStudentsTable.Visibility = Visibility.Visible;
            StudentEditingWindow.Visibility = Visibility.Collapsed;
            Add_Btn.IsEnabled = true;
            File_Btn.IsEnabled = true;

            criteriaWindowWorkState = CriteriaWindowWorkState.Null;
            isDataChanged.DataWasChanged();
        }

        #endregion

        private void Item_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Edit_Btn.IsEnabled = true;
            DeleteStudentBtn.IsEnabled = true;

            (sender as TableStudentElement).IsSelected = true;

            SelectedStudentIndex = TableForStudents.ItemBase.Items.IndexOf(sender);

            foreach (TableStudentElement item in TableForStudents.ItemBase.Items)
            {
                if (item != (sender as TableStudentElement))
                {
                    item.IsSelected = false;
                }
                else
                {
                    TableForStudents.SelectedStudent = (sender as TableStudentElement).thisStudent;
                }
            }

        }

        private void DragWindow(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed &&
                !TableForStudents.IsMouseOver &&
                !criteriaTable.IsMouseOver &&
                !StudentFIO_TB.IsMouseOver &&
                !StudentForm_TB.IsMouseOver &&
                !StudentProgectName_TB.IsMouseOver &&
                !TutorFIO_TB.IsMouseOver)
            {
                this.WindowState = WindowState.Normal;
                this.DragMove();

            }

        }

        public FrameworkElement CreateUIElementOfStudent(StudentStruct.Student student)
        {
            Grid grid = new Grid();
            double width = TableForStudents.CalculateElementWidth();
            grid.Width = width;

            ColumnDefinition column_0 = new ColumnDefinition
            {
                Width = TableForStudents.Header.ColumnDefinitions[0].Width
            };
            grid.ColumnDefinitions.Add(column_0);

            ColumnDefinition column_1 = new ColumnDefinition
            {
                Width = TableForStudents.Header.ColumnDefinitions[1].Width
            };
            grid.ColumnDefinitions.Add(column_1);

            ColumnDefinition column_2 = new ColumnDefinition
            {
                Width = TableForStudents.Header.ColumnDefinitions[2].Width
            };
            grid.ColumnDefinitions.Add(column_2);

            ColumnDefinition column_3 = new ColumnDefinition
            {
                Width = TableForStudents.Header.ColumnDefinitions[3].Width
            };
            grid.ColumnDefinitions.Add(column_3);

            ColumnDefinition column_4 = new ColumnDefinition
            {
                Width = TableForStudents.Header.ColumnDefinitions[4].Width
            };
            grid.ColumnDefinitions.Add(column_4);

            ColumnDefinition column_5 = new ColumnDefinition
            {
                Width = TableForStudents.Header.ColumnDefinitions[5].Width
            };
            grid.ColumnDefinitions.Add(column_5);

            ColumnDefinition column_6 = new ColumnDefinition
            {
                Width = TableForStudents.Header.ColumnDefinitions[6].Width
            };
            grid.ColumnDefinitions.Add(column_6);


            TextBlock IDTextBlock = new TextBlock();
            TextBlock FIOTextBlock = new TextBlock(); FIOTextBlock.Margin = new Thickness(5, 0, 0, 0);
            TextBlock FormTextBlock = new TextBlock();
            TextBlock ThemeTextBlock = new TextBlock(); ThemeTextBlock.Margin = new Thickness(5);
            TextBlock TutorTextBlock = new TextBlock(); TutorTextBlock.Margin = new Thickness(5);
            TextBlock ScoreTextBlock = new TextBlock();
            TextBlock MarkTextBlock = new TextBlock();

            Border IDBorder = new Border
            {
                Style = Resources["ElementInTableStyle"] as Style,
                Child = IDTextBlock
            };

            Border FIOBorder = new Border
            {
                Style = Resources["ElementInTableStyle"] as Style,
                Child = FIOTextBlock
            };

            Border FormBorder = new Border
            {
                Style = Resources["ElementInTableStyle"] as Style,
                Child = FormTextBlock
            };

            Border ThemeBorder = new Border
            {
                Style = Resources["ElementInTableStyle"] as Style,
                Child = ThemeTextBlock
            };

            Border TutorBorder = new Border
            {
                Style = Resources["ElementInTableStyle"] as Style,
                Child = TutorTextBlock
            };

            Border ScoreBorder = new Border
            {
                Style = Resources["ElementInTableStyle"] as Style,
                Child = ScoreTextBlock
            };

            Border MarkBorder = new Border
            {
                Style = Resources["ElementInTableStyle"] as Style,
                Child = MarkTextBlock
            };

            IDBorder.SetValue(Grid.ColumnProperty, 0);
            FIOBorder.SetValue(Grid.ColumnProperty, 1);
            FormBorder.SetValue(Grid.ColumnProperty, 2);
            ThemeBorder.SetValue(Grid.ColumnProperty, 3);
            TutorBorder.SetValue(Grid.ColumnProperty, 4);
            ScoreBorder.SetValue(Grid.ColumnProperty, 5);
            MarkBorder.SetValue(Grid.ColumnProperty, 6);

            IDTextBlock.Text = student.ID.ToString();
            FIOTextBlock.Text = student.FIO;
            FormTextBlock.Text = student.Form;
            ThemeTextBlock.Text = student.Theme;
            TutorTextBlock.Text = student.Tutor;
            byte ScoreSum = student.IndividualCriteriaScores.SumBytes();
            ScoreTextBlock.Text = ScoreSum.ToString();
            MarkTextBlock.Text = StudentStruct.CalculateMark(ScoreSum).ToString();

            IDTextBlock.TextWrapping = TextWrapping.Wrap;
            FIOTextBlock.TextWrapping = TextWrapping.Wrap;
            FormTextBlock.TextWrapping = TextWrapping.Wrap;
            ThemeTextBlock.TextWrapping = TextWrapping.Wrap;
            TutorTextBlock.TextWrapping = TextWrapping.Wrap;
            ScoreTextBlock.TextWrapping = TextWrapping.Wrap;
            MarkTextBlock.TextWrapping = TextWrapping.Wrap;

            IDTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
            FIOTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
            FormTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
            ThemeTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
            TutorTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
            ScoreTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
            MarkTextBlock.HorizontalAlignment = HorizontalAlignment.Center;

            IDTextBlock.VerticalAlignment = VerticalAlignment.Center;
            FIOTextBlock.VerticalAlignment = VerticalAlignment.Center;
            FormTextBlock.VerticalAlignment = VerticalAlignment.Center;
            ThemeTextBlock.VerticalAlignment = VerticalAlignment.Center;
            TutorTextBlock.VerticalAlignment = VerticalAlignment.Center;
            ScoreTextBlock.VerticalAlignment = VerticalAlignment.Center;
            MarkTextBlock.VerticalAlignment = VerticalAlignment.Center;


            grid.Children.Add(IDBorder);
            grid.Children.Add(FIOBorder);
            grid.Children.Add(FormBorder);
            grid.Children.Add(ThemeBorder);
            grid.Children.Add(TutorBorder);
            grid.Children.Add(ScoreBorder);
            grid.Children.Add(MarkBorder);

            return grid;
        }

        public void AddNewStudentIntoTable(StudentStruct.Student stud, int index)
        {
            TableStudentElement element = new TableStudentElement();
            element.CreateTableStudentElement(stud, index);

            element.HorizontalAlignment = HorizontalAlignment.Left;

            TableForStudents.ItemBase.Items.Add(element);

        }

        public void LoadStudentsAsUIElements(StudentStruct.Student[] studs)
        {
            int i = 0;
            foreach (var stud in studs)
            {
                i++;
                AddNewStudentIntoTable(stud, i);
            }

        }

        #region HeaderBtns EventHandlers

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!isDataChanged.IsChanged())
            {
                Application.Current.Shutdown();
                return;
            }

            var p = MessageBox.Show("В вашем файле есть несохраненные изменения. Сохранить перед выходом?", "Сохранить?", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

            if (p == MessageBoxResult.Yes)
            {
                if (!SaveFile())
                    return;
                Application.Current.Shutdown();
            }
            else if (p == MessageBoxResult.No)
            {
                Application.Current.Shutdown();
            }

        }

        private void MaxBtn_Click(object sender, RoutedEventArgs e)
        {
            if (formState == FormState.Maximized)
            {

                this.SetValue(WidthProperty, NormalSize.X);
                this.SetValue(HeightProperty, NormalSize.X);
                formState = FormState.Normal;

                this.Left = WindowLocationBeforeMaximaze.X;
                this.Top = WindowLocationBeforeMaximaze.Y;

            }
            else
            {
                formState = FormState.Maximized;
                WindowLocationBeforeMaximaze = new Point(this.Left, this.Top);

                this.SetValue(WidthProperty, SystemParameters.WorkArea.Width);
                this.SetValue(HeightProperty, SystemParameters.WorkArea.Height);

                this.Top = (screenHeight - this.Height) / 2 + 15;
                this.Left = (screenWidth - this.Width) / 2;
            }

        }

        private void MinBtn_Click(object sender, RoutedEventArgs e)
        {

            mainForm.WindowState = WindowState.Minimized;

        }

        #endregion

        #region WorkStaffBtns EventHandlers

        private void AddStudent_BtnClick(object sender, RoutedEventArgs e)
        {
            AllStudentsTable.Visibility = Visibility.Collapsed;
            StudentEditingWindow.Visibility = Visibility.Visible;
            Add_Btn.IsEnabled = false;
            Edit_Btn.IsEnabled = false;
            DeleteStudentBtn.IsEnabled = false;
            File_Btn.IsEnabled = false;

            TableForStudents.ItemBase.Items.Cast<TableStudentElement>().ToList().ForEach((x) => x.IsSelected = false);

            criteriaWindowWorkState = CriteriaWindowWorkState.AddState;

        }

        private void Edit_BtnClick(object sender, RoutedEventArgs e)
        {
            OpenEditStudent();
        }

        private void File_Btn_Click(object sender, RoutedEventArgs e)
        {
            FilePopup.IsOpen = !FilePopup.IsOpen;
        }

        #endregion

        #region CriteriaTable EventHandlers

        private void ClearAllCriteria()
        {
            foreach (FrameworkElement item in criteriaTable.ItemBase.Children)
            {
                if ((item as CriteriaTableElement) != null)
                {

                    foreach (RadioButton b in (item as CriteriaTableElement).RadioButtonStackPanel.Children)
                    {
                        b.IsChecked = false;
                    }

                }
            }

            ScoreSum_TB.Text = "Сумма баллов: ";
            Mark_TB.Text = "Оценка: ";
            criteriaTable.criteriaScores = CriteriaScores.CreateCriteriaScores(CriteriaTable_PropertyChanged);
        }

        public void CriteriaTable_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SaveBtnReadiness();

            ScoreSum_TB.Text = "Сумма баллов: " + criteriaTable.criteriaScores.Scores.SumBytes().ToString();

        }

        private void Cancel_BtnClick(object sender, RoutedEventArgs e)
        {
            if (criteriaWindowWorkState == CriteriaWindowWorkState.ReadOnly)
            {
                foreach (FrameworkElement item in criteriaTable.ItemBase.Children)
                {
                    if ((item as CriteriaTableElement) != null)
                    {
                        ((item as CriteriaTableElement).RadioButtonStackPanel.Children[0] as RadioButton).Style = (Style)FindResource("NormalRadioBtn");
                        ((item as CriteriaTableElement).RadioButtonStackPanel.Children[1] as RadioButton).Style = (Style)FindResource("NormalRadioBtn");
                        ((item as CriteriaTableElement).RadioButtonStackPanel.Children[2] as RadioButton).Style = (Style)FindResource("NormalRadioBtn");
                        ((item as CriteriaTableElement).RadioButtonStackPanel.Children[0] as RadioButton).IsEnabled = true;
                        ((item as CriteriaTableElement).RadioButtonStackPanel.Children[1] as RadioButton).IsEnabled = true;
                        ((item as CriteriaTableElement).RadioButtonStackPanel.Children[2] as RadioButton).IsEnabled = true;
                    }
                }

                StudentFIO_TB.IsReadOnly = false;
                StudentForm_TB.IsReadOnly = false;
                StudentProgectName_TB.IsReadOnly = false;
                TutorFIO_TB.IsReadOnly = false;

                StudentFIO_TB.TextAlignment = TextAlignment.Left;
                StudentForm_TB.TextAlignment = TextAlignment.Left;
                StudentProgectName_TB.TextAlignment = TextAlignment.Left;
                TutorFIO_TB.TextAlignment = TextAlignment.Left;
            }

            ClearAllTextBoxes();
            ClearAllCriteria();
            ScoreSum_TB.Text = "Сумма баллов: ";

            AllStudentsTable.Visibility = Visibility.Visible;
            StudentEditingWindow.Visibility = Visibility.Collapsed;
            Add_Btn.IsEnabled = true;
            File_Btn.IsEnabled = true;

            criteriaTable.criteriaScores.Scores = new byte[4 + 4 + 4 + 2];

            criteriaWindowWorkState = CriteriaWindowWorkState.Null;
            TableForStudents.ItemBase.Items.Cast<TableStudentElement>().ToList().ForEach((x) => x.IsSelected = false);
        }

        private void Save_BtnClick(object sender, RoutedEventArgs e)
        {
            AddStudent();
        }

        private void ClearAllTextBoxes()
        {
            StudentFIO_TB.Text = "";
            StudentForm_TB.Text = "";
            StudentProgectName_TB.Text = "";
            TutorFIO_TB.Text = "";
        }


        #endregion

        public void SaveBtnReadiness()
        {

            if (criteriaTable.criteriaScores.Scores.Reverse().Skip(criteriaTable.SkipableScoresAmount).Reverse().All((x) => x != 0))
            {
                Mark_TB.Text = "Оценка: " + StudentStruct.CalculateMark(criteriaTable.criteriaScores.Scores.SumBytes()).ToString();
                Mark_TB.IsEnabled = true;
            }
            else
            {
                Mark_TB.Text = "Оценка: ";
                Mark_TB.IsEnabled = false;
            }

            if (isAllTextBoxesFilledProperty.IsField
                    && criteriaTable.criteriaScores.Scores.Reverse().Skip(criteriaTable.SkipableScoresAmount).Reverse().All((x) => x != 0)
                    && criteriaWindowWorkState != CriteriaWindowWorkState.ReadOnly)
            {
                (Application.Current.MainWindow as MainWindow).Save_Btn.IsEnabled = true;
            }
            else
            {
                (Application.Current.MainWindow as MainWindow).Save_Btn.IsEnabled = false;
            }
        }

        private void TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox TB = sender as TextBox;
            if (!TB.Text.IsNullOrEmpty())
            {
                switch (TB.Name)
                {
                    case "StudentFIO_TB":
                        isAllTextBoxesFilledProperty[0] = true;
                        break;
                    case "StudentForm_TB":
                        isAllTextBoxesFilledProperty[1] = true;
                        break;
                    case "StudentProgectName_TB":
                        isAllTextBoxesFilledProperty[2] = true;
                        break;
                    case "TutorFIO_TB":
                        isAllTextBoxesFilledProperty[3] = true;
                        break;

                }
            }

            else
            {
                switch (TB.Name)
                {
                    case "StudentFIO_TB":
                        isAllTextBoxesFilledProperty[0] = false;
                        break;
                    case "StudentForm_TB":
                        isAllTextBoxesFilledProperty[1] = false;
                        break;
                    case "StudentProgectName_TB":
                        isAllTextBoxesFilledProperty[2] = false;
                        break;
                    case "TutorFIO_TB":
                        isAllTextBoxesFilledProperty[3] = false;
                        break;

                }
            }

        }

        private void ReloadWindow()
        {
            FileName_TextBlock.Text = "";
            TableForStudents.ItemBase.Items.Clear();
            SaveFileAs_PopupBtns.IsEnabled = true;
            sortingType = SortingType.Null;
        }

        private void SortStudents()
        {
            switch (sortingType)
            {
                case SortingType.Null:
                    break;
                case SortingType.Form:
                    FsstFile.students = FsstFile.students?.OrderBy((x) => x.Form).ToArray();
                    break;
                case SortingType.Students:
                    FsstFile.students = FsstFile.students?.OrderBy((x) => x.FIO).ToArray();
                    break;
                case SortingType.Scores:
                    FsstFile.students = FsstFile.students?.OrderByDescending((x) => x.IndividualCriteriaScores.SumBytes()).ToArray();
                    break;
                case SortingType.Tutors:
                    FsstFile.students = FsstFile.students?.OrderBy((x) => x.Tutor).ToArray();
                    break;
            }
        }

        private void FillTableStudents()
        {
            TableForStudents.ItemBase.Items.Clear();
            SaveFileAs_PopupBtns.IsEnabled = true;
            DeleteStudentBtn.IsEnabled = false;
            Edit_Btn.IsEnabled = false;

            if (FsstFile.students == null || FsstFile.StudentAmount == 0)
            {
                return;
            }


            SortStudents();

            ProtocolBtn.IsEnabled = true;

            LoadStudentsAsUIElements(FsstFile.students);
            TableForStudents.RecalculateElementsMeasures();

            foreach (TableStudentElement item in TableForStudents.ItemBase.Items)
            {
                item.MouseLeftButtonUp += Item_MouseLeftButtonUp;
                item.MouseRightButtonUp += Item_MouseLeftButtonUp;
                item.MouseRightButtonUp += Item_MouseRightButtonUp;
            }
        }

        private void Item_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {

            (sender as TableStudentElement).popup.IsOpen = (sender as TableStudentElement).IsSelected;
            //foreach (TableStudentElement item in TableForStudents.ItemBase.Items)
            //{
            //    item.popup.IsOpen = false;
            //}
        }

        #region Popup Buttons

        public string[] ValidateDate(string[] arr)
        {
            if (arr.Length != 3)
            {
                return new string[] { null, null, null };
            }
            Int16 p = 0;

            string[] res = new string[3];

            if (!Int16.TryParse(arr[0], out p) || !(p > 0 && p < 32))
                res[0] = null;
            else
                res[0] = arr[0];

            res[1] = NumberToMonth(arr[1]);

            if ((arr[2].Length != 4) || !Int16.TryParse(arr[2], out p))
                res[2] = null;
            else
                res[2] = arr[2];

            return res;
        }

        public string[] DateToStrings(UInt16[] arr)
        {
            string[] res = new string[3];
            res[0] = arr[0].ToString();
            res[1] = NumberToMonth(arr[1].ToString());
            res[2] = arr[2].ToString();

            return res;
        }
        private string NumberToMonth(string number)
        {
            switch (new StringBuilder().Append(number.SkipWhile((x) => x == '0').ToArray()).ToString())
            {
                case "1":
                    return "Январь";
                case "2":
                    return "Февраль";
                case "3":
                    return "Март";
                case "4":
                    return "Апрель";
                case "5":
                    return "Май";
                case "6":
                    return "Июнь";
                case "7":
                    return "Июль";
                case "8":
                    return "Август";
                case "9":
                    return "Сентябрь";
                case "10":
                    return "Октябрь";
                case "11":
                    return "Ноябрь";
                case "12":
                    return "Декабрь";
                default:
                    return null;
            }
        }
        private bool TryToShowWannaSave()
        {
            if (isDataChanged.IsChanged())
            {
                string message = "У вас есть несохраненные данные. Сохранить?";

                var result = MessageBox.Show(message, "Сохранить?", MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);

                if (result == MessageBoxResult.Yes)
                {
                    if (File.Exists(WorkWith_FilePath))
                    {
                        FsstFile.WriteToByteFSSTFile(WorkWith_FilePath);
                        isDataChanged.DataSaved();
                    }
                    else
                    {
                        SaveFileDialog saveFileDialog_1 = new SaveFileDialog();
                        saveFileDialog_1.Filter = "FSST File (*.fsst)|*.fsst|All files (*.*)|*.*";
                        saveFileDialog_1.InitialDirectory = @"c:\temp\";
                        if (saveFileDialog_1.ShowDialog() == true)
                        {
                            WorkWith_FilePath = saveFileDialog_1.FileName;
                            FileName_TextBlock.Text = saveFileDialog_1.SafeFileName;
                            FsstFile.WriteToByteFSSTFile(saveFileDialog_1.FileName);

                            isDataChanged.DataSaved();
                        }
                    }

                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return false;
                }
            }

            return true;

        }
        private bool SaveFile()
        {
            if (File.Exists(WorkWith_FilePath))
            {
                FsstFile.WriteToByteFSSTFile(WorkWith_FilePath);

                isDataChanged.DataSaved();

                return true;
            }
            else
            {

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "FSST File (*.fsst)|*.fsst|All files (*.*)|*.*";
                saveFileDialog.InitialDirectory = @"c:\temp\";
                saveFileDialog.Title = "Сохранить";
                if (saveFileDialog.ShowDialog() == true)
                {
                    string fileName = saveFileDialog.SafeFileName;
                    if (new StringBuilder().Append(saveFileDialog.SafeFileName.Reverse().TakeWhile((x) => x != '.').Reverse().ToArray()).ToString() != "fsst")
                    {
                        fileName = string.Join("", fileName.TakeWhile((x) => x != '.')) + ".fsst";
                    }

                    WorkWith_FilePath = System.IO.Path.GetDirectoryName(saveFileDialog.FileName) + "\\" + fileName;
                    FileName_TextBlock.Text = fileName;
                    FsstFile.WriteToByteFSSTFile(System.IO.Path.GetDirectoryName(saveFileDialog.FileName) + "\\" + fileName);

                    isDataChanged.DataSaved();
                    return true;
                }
            }

            return false;

        }
        private void OpenFile()
        {
            if (!TryToShowWannaSave())
            {
                return;
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "FSST File (*.fsst)|*.fsst";
            openFileDialog.InitialDirectory = @"c:\temp\";
            openFileDialog.Title = "Открыть";

            if (openFileDialog.ShowDialog() == true)
            {
                if (openFileDialog.FileName == WorkWith_FilePath)
                {
                    return;
                }



                FsstFile = FSSTFilePresenter.BytesToFSSTFile(File.ReadAllBytes(openFileDialog.FileName));

                if (FsstFile == null)
                {
                    return;
                }

                WorkWith_FilePath = openFileDialog.FileName;
                ReloadWindow();
                FileName_TextBlock.Text = new StringBuilder().Append(WorkWith_FilePath.Reverse().TakeWhile((x) => x != '\\').Reverse().ToArray()).ToString();
                ResetSortingType();
                FillTableStudents();
                isDataChanged.DataSaved();

            }
        }
        private void CreateFile()
        {
            if (!TryToShowWannaSave())
            {
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "FSST File (*.fsst)|*.fsst|All files (*.*)|*.*";
            saveFileDialog.InitialDirectory = @"c:\temp\";
            saveFileDialog.Title = "Создать новый";

            if (saveFileDialog.ShowDialog() == true)
            {
                string fileName = saveFileDialog.SafeFileName;
                if (new StringBuilder().Append(saveFileDialog.SafeFileName.Reverse().TakeWhile((x) => x != '.').Reverse().ToArray()).ToString() != "fsst")
                {
                    fileName = string.Join("", fileName.TakeWhile((x) => x != '.')) + ".fsst";
                }

                FsstFile = FSSTFilePresenter.LoadFile();
                FsstFile.WriteToByteFSSTFile(System.IO.Path.GetDirectoryName(saveFileDialog.FileName) + "\\" + fileName);

                WorkWith_FilePath = saveFileDialog.FileName;

                FileName_TextBlock.Text = fileName;
                FillTableStudents();
                ResetSortingType();

                isDataChanged.DataSaved();

            }
        }
        private void SaveAsFile()
        {

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "FSST File (*.fsst)|*.fsst|All files (*.*)|*.*";
            saveFileDialog.InitialDirectory = @"c:\temp\";
            saveFileDialog.Title = "Сохранить как";
            if (saveFileDialog.ShowDialog() == true)
            {
                string fileName = saveFileDialog.SafeFileName;
                if (new StringBuilder().Append(saveFileDialog.SafeFileName.Reverse().TakeWhile((x) => x != '.').Reverse().ToArray()).ToString() != "fsst")
                {
                    fileName = string.Join("", fileName.TakeWhile((x) => x != '.')) + ".fsst";
                }

                WorkWith_FilePath = System.IO.Path.GetDirectoryName(saveFileDialog.FileName) + "\\" + fileName;
                FileName_TextBlock.Text = fileName;
                FsstFile.WriteToByteFSSTFile(System.IO.Path.GetDirectoryName(saveFileDialog.FileName) + "\\" + fileName);

                isDataChanged.DataSaved();

            }

        }

        private void SaveFile_PopupBtns_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        private void OpenFile_PopupBtns_Click(object sender, RoutedEventArgs e)
        {
            OpenFile();
        }

        private void CreateBtn_PopupBtns_Click(object sender, RoutedEventArgs e)
        {
            CreateFile();
        }

        private void SaveFileAs_PopupBtns_Click(object sender, RoutedEventArgs e)
        {
            SaveAsFile();
        }

        private void PropertiesBtn_PopupBtns_Click(object sender, RoutedEventArgs e)
        {
            PropertiesWindow window = new PropertiesWindow(FsstFile.HeadTeacherName, FsstFile.TeacherNames.ToArray());
            window.Owner = this;

            if (window.ShowDialog() == true)
            {
                FsstFile.HeadTeacherName = window.ChairmanName_TB.Text;
                FsstFile.TeacherNames.Clear();
                foreach (var item in window.TextBoxes)
                {
                    FsstFile.TeacherNames.Add(item.Text);
                }

                FsstFile.TeacherAmount = (UInt16)FsstFile.TeacherNames.Count;

                UInt16[] exam = window.examinationDate_TB.Text.Split('.').ArrayCastToUInt16s();
                UInt16[] inp = window.inputDate_TB.Text.Split('.').ArrayCastToUInt16s();

                FsstFile.ExaminationDate = exam;
                FsstFile.InputDate = inp;

                SystemSettings.SysSettingsFile.SetNewSettings(out (Application.Current as App).Settings,
                    new FileProperties(FsstFile.HeadTeacherName, FsstFile.TeacherNames.ToArray(), window.examinationDate_TB.Text, window.inputDate_TB.Text));

            }
        }

        private void FileCombinerBtn_PopupBtns_Click(object sender, RoutedEventArgs e)
        {
            FileCombinerBtn_Click();
         }
        #endregion

        private void FileCombinerBtn_Click()
        {

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "FSST File (*.fsst)|*.fsst|All files (*.*)|*.*";
            saveFileDialog.InitialDirectory = @"c:\temp\";
            saveFileDialog.Title = "Создать сводный документ";

            if (saveFileDialog.ShowDialog() != true)
                return;

            FileCombiner fc = new FileCombiner();

            if (fc.ShowDialog() != true)
                return;

            FSSTFilePresenter.FSSTFile newfile = fc.CombineFiles(fc.FSSTFiles.ToArray());


            string fileName = saveFileDialog.SafeFileName;
            if (new StringBuilder().Append(saveFileDialog.SafeFileName.Reverse().TakeWhile((x) => x != '.').Reverse().ToArray()).ToString() != "fsst")
            {
                fileName = string.Join("", fileName.TakeWhile((x) => x != '.')) + ".fsst";
            }

            WorkWith_FilePath = System.IO.Path.GetDirectoryName(saveFileDialog.FileName) + "\\" + fileName;
            FileName_TextBlock.Text = fileName;
            newfile.WriteToByteFSSTFile(System.IO.Path.GetDirectoryName(saveFileDialog.FileName) + "\\" + fileName);

            isDataChanged.DataSaved();

        }

        private void Info_Button_Click(object sender, RoutedEventArgs e)
        {
            InfoWindow infoWindow = new InfoWindow();
            infoWindow.Owner = this;
            infoWindow.ShowDialog();
        }

        private void ResetSortingType()
        {
            Sorting.Children.OfType<RadioButton>().ToList().ForEach((x) => x.IsChecked = false);
            sortingType = SortingType.Null;
        }

        private void SortingButton_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as RadioButton).Content)
            {
                case "По классу":
                    sortingType = SortingType.Form;
                    FillTableStudents();
                    break;
                case "По ФИО учеников":
                    sortingType = SortingType.Students;
                    FillTableStudents();
                    break;
                case "По баллам":
                    sortingType = SortingType.Scores;
                    FillTableStudents();
                    break;
                case "По руководителю":
                    sortingType = SortingType.Tutors;
                    FillTableStudents();
                    break;
            }
        }

        private void AllStudentsTable_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TableForStudents.Width = ColumnForTable.ActualWidth;
            TableForStudents.Height = this.Height - AllStudentsTable.Margin.Bottom - AllStudentsTable.Margin.Top - 100;
        }

        #region Shortcuts

        private void SaveShortcut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (criteriaWindowWorkState == CriteriaWindowWorkState.Null)
                SaveFile();
        }

        private void NewShortcut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (criteriaWindowWorkState == CriteriaWindowWorkState.Null)
                CreateFile();
        }

        private void OpenShortcut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (criteriaWindowWorkState == CriteriaWindowWorkState.Null)
                OpenFile();
        }

        private void SaveAsShortcut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (criteriaWindowWorkState == CriteriaWindowWorkState.Null)
                SaveAsFile();
        }

        private void RevokeShortcut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (criteriaWindowWorkState == CriteriaWindowWorkState.Null)
                RevokeAction();
        }

        private void ReturnShortcut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (criteriaWindowWorkState == CriteriaWindowWorkState.Null)
                ReturnAction();
        }

        #endregion

        private void DeleteStudentBtn_Click(object sender, RoutedEventArgs e)
        {
            DeleteStudent();
        }


        #region ActionHistory

        private void AddNewActionToHistory(ActionType action, StudentStruct.Student student)
        {
            if (action > ActionType.DeleteStudent)
            {
                throw new Exception("Invalid ActionType");
            }

            StudentsActionHistory.Push(student);
            ActionTypesActionHistory.Push(action);
            StudentsTrashHistory.Clear();
            ActionTypesTrashHistory.Clear();


            RevokeBtn_PopupBtns.IsEnabled = true;

        }



        /// <summary>
        /// -->
        /// </summary>
        public void ReturnAction()
        {
            if (ActionTypesTrashHistory.Count <= 0)
            {
                return;
            }

            ActionType action = ActionTypesTrashHistory.Pop();
            ActionTypesActionHistory.Push(action);

            var p = FsstFile.students.ToList();
            switch (action)
            {
                case ActionType.AddStudent:
                    StudentsActionHistory.Push(StudentsTrashHistory.Pop());
                    p.Add(StudentsActionHistory.Peek());
                    break;

                case ActionType.EditStudent:
                    StudentStruct.Student student = p[SelectedStudentIndex];
                    p[p.IndexOf(p.Find(x => x.ID == StudentsTrashHistory.Peek().ID))] = StudentsTrashHistory.Pop();
                    StudentsActionHistory.Push(student);
                    break;

                case ActionType.DeleteStudent:
                    StudentsActionHistory.Push(StudentsTrashHistory.Pop());
                    p.Remove(StudentsActionHistory.Peek());
                    break;
            }

            FsstFile.students = p.ToArray();
            FillTableStudents();

        }

        /// <summary>
        /// <--
        /// </summary>
        public void RevokeAction()
        {
            if (ActionTypesActionHistory.Count <= 0)
            {
                return;
            }
            ActionType action = ActionTypesActionHistory.Pop();
            ActionTypesTrashHistory.Push(action);


            var p = FsstFile.students.ToList();
            switch (action)
            {
                case ActionType.AddStudent:
                    StudentsTrashHistory.Push(StudentsActionHistory.Pop());
                    p.Remove(StudentsTrashHistory.Peek());
                    break;

                case ActionType.EditStudent:
                    StudentStruct.Student student = p[SelectedStudentIndex];
                    p[p.IndexOf(p.Find(x => x.ID == StudentsActionHistory.Peek().ID))] = StudentsActionHistory.Pop();
                    StudentsTrashHistory.Push(student);
                    break;

                case ActionType.DeleteStudent:
                    StudentsTrashHistory.Push(StudentsActionHistory.Pop());
                    p.Insert(StudentsTrashHistory.Peek().ID, StudentsTrashHistory.Peek());
                    break;
            }

            FsstFile.students = p.ToArray();
            FillTableStudents();

            isDataChanged.DataWasChanged();




        }

        private void RevokeBtn_PopupBtns_Click(object sender, RoutedEventArgs e)
        {
            RevokeAction();
        }

        private void ReturnBtn_PopupBtns_Click(object sender, RoutedEventArgs e)
        {
            ReturnAction();
        }

        #endregion

        private void ProtocolBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Документ Word (*.docx)|*.docx|All files (*.*)|*.*";
            saveFileDialog.InitialDirectory = @"c:\temp\";
            saveFileDialog.Title = "Протокол";
            saveFileDialog.FileName = (WorkWith_FilePath == null) ? string.Empty : WorkWith_FilePath.Split('\\').Last().Split('.')[0] + ".docx";
            if (saveFileDialog.ShowDialog() == true)
            {
                FileStream fs = null;
                bool isError = false;
                string fileName = saveFileDialog.SafeFileName;
                if (fileName.Split('.').Last() != ".docx")
                {
                    var arr = fileName.Split('.').ToList();
                    arr.Add(".docx");
                    fileName = string.Join("", arr);
                }


                do
                {
                    try
                    {
                        fs = System.IO.File.Open(saveFileDialog.FileName, FileMode.Open);
                        isError = false;
                    }
                    catch (FileNotFoundException)
                    {

                    }
                    catch (Exception ex)
                    {
                        if (ex.IsFileLocked())
                        {
                            // Файл открыт в стороннем процессе
                            if (MessageBox.Show("Файл \"" + saveFileDialog.SafeFileName + " открыт в стороннем приложении. Пожалуйста, закройте этот файл и повторите попытку.",
                                            "\"" + saveFileDialog.SafeFileName + "\" открыт", MessageBoxButton.OKCancel, MessageBoxImage.Error)
                                            == MessageBoxResult.Cancel
                                            )
                            {
                                return;
                            }

                            isError = true;

                        }
                        else
                        {
                            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    finally
                    {
                        fs?.Close();
                    }
                } while (isError);

                string[] examinationDate = new string[0];
                string[] inputDate = new string[0];
                string ChairmanName = null;
                List<string> names = new List<string>();

                FileProperties newProps = new FileProperties();

                bool p = true;
                do
                {

                    DateInputForDocxWindow window = new DateInputForDocxWindow(FsstFile.HeadTeacherName, FsstFile.TeacherNames.ToArray());
                    window.Owner = this;

                    if (window.ShowDialog() == true)
                    {

                        examinationDate = window.examinationDate_TB.Text.Split('.');
                        inputDate = window.inputDate_TB.Text.Split('.');

                        p = ValidateDate(examinationDate).All((x) => x != null) && ValidateDate(inputDate).All((x) => x != null);
                        if (!p)
                        {
                            MessageBox.Show("Введен неверный формат даты. (30 января 2020г. = 30.01.2020)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            continue;
                        }

                        List<string> Teachers = new List<string>();
                        foreach (TextBox tb in window.TextBoxes) { Teachers.Add(tb.Text); }

                        ChairmanName = window.ChairmanName_TB.Text;

                        FsstFile.HeadTeacherName = ChairmanName;
                        FsstFile.TeacherNames = Teachers;

                        FsstFile.ExaminationDate = examinationDate.ArrayCastToUInt16s();
                        FsstFile.InputDate = inputDate.ArrayCastToUInt16s();

                        newProps = new FileProperties(ChairmanName, Teachers.ToArray(), window.examinationDate_TB.Text, window.inputDate_TB.Text);

                    }
                    else return;
                } while (!p);

                //FsstFile.FSSTFileToDocxBase(saveFileDialog.FileName, examinationDate, inputDate);

                (Application.Current as App).Settings.TeacherNames = FsstFile.TeacherNames.ToArray();
                (Application.Current as App).Settings.ChairmanName = FsstFile.HeadTeacherName;

                SystemSettings.SysSettingsFile.SetNewSettings(out (Application.Current as App).Settings, newProps);
                SystemSettings.SaveFile((Application.Current as App).AppPath + (Application.Current as App).SettingsPath, (Application.Current as App).Settings);

                if (!(ReferenceEquals(WorkWith_FilePath, "") || ReferenceEquals(WorkWith_FilePath, null)))
                {
                    FSSTFilePresenter.WriteToByteFSSTFile(FsstFile, WorkWith_FilePath);
                }

                FsstFile.FSSTFileToNewDocx(saveFileDialog.FileName);

            }
        }

        public void IndivProtocolShow()
        {
            AllStudentsTable.Visibility = Visibility.Collapsed;
            StudentEditingWindow.Visibility = Visibility.Visible;
            Add_Btn.IsEnabled = false;
            Edit_Btn.IsEnabled = false;
            File_Btn.IsEnabled = false;
            DeleteStudentBtn.IsEnabled = false;

            StudentFIO_TB.IsReadOnly = true;
            StudentForm_TB.IsReadOnly = true;
            StudentProgectName_TB.IsReadOnly = true;
            TutorFIO_TB.IsReadOnly = true;

            StudentFIO_TB.TextAlignment = TextAlignment.Center;
            StudentForm_TB.TextAlignment = TextAlignment.Center;
            StudentProgectName_TB.TextAlignment = TextAlignment.Center;
            TutorFIO_TB.TextAlignment = TextAlignment.Center;

            criteriaWindowWorkState = CriteriaWindowWorkState.ReadOnly;

            StudentFIO_TB.Text = TableForStudents.SelectedStudent.FIO;
            StudentForm_TB.Text = TableForStudents.SelectedStudent.Form;
            StudentProgectName_TB.Text = TableForStudents.SelectedStudent.Theme;
            TutorFIO_TB.Text = TableForStudents.SelectedStudent.Tutor;


            int i = 0;
            foreach (FrameworkElement item in criteriaTable.ItemBase.Children)
            {
                if ((item as CriteriaTableElement) != null)
                {
                    ((item as CriteriaTableElement).RadioButtonStackPanel.Children[0] as RadioButton).Style = (Style)FindResource("ReadOnlyRadioBtn");
                    ((item as CriteriaTableElement).RadioButtonStackPanel.Children[1] as RadioButton).Style = (Style)FindResource("ReadOnlyRadioBtn");
                    ((item as CriteriaTableElement).RadioButtonStackPanel.Children[2] as RadioButton).Style = (Style)FindResource("ReadOnlyRadioBtn");

                    ((item as CriteriaTableElement).RadioButtonStackPanel.Children[TableForStudents.SelectedStudent.IndividualCriteriaScores[i] - 1] as RadioButton).IsChecked = true;
                    ((item as CriteriaTableElement).RadioButtonStackPanel.Children[0] as RadioButton).IsEnabled = false;
                    ((item as CriteriaTableElement).RadioButtonStackPanel.Children[1] as RadioButton).IsEnabled = false;
                    ((item as CriteriaTableElement).RadioButtonStackPanel.Children[2] as RadioButton).IsEnabled = false;
                    i++;
                }
            }
        }

        public void ExportIndivProtocol()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Документ Word (*.docx)|*.docx|All files (*.*)|*.*";
            saveFileDialog.InitialDirectory = @"c:\temp\";
            saveFileDialog.Title = "Индивидуальный протокол";
            saveFileDialog.FileName = FsstFile.students[SelectedStudentIndex].FIO + ".docx";
            if (saveFileDialog.ShowDialog() == true)
            {
                FileStream fs = null;
                bool isError = false;
                string fileName = saveFileDialog.SafeFileName;
                if (fileName.Split('.').Last() != "docx")
                {
                    var arr = fileName.Split('.').ToList();
                    arr.Add("docx");
                    fileName = string.Join(".", arr);
                }
                do
                {
                    try
                    {
                        fs = System.IO.File.Open(saveFileDialog.FileName, FileMode.Open);
                        isError = false;
                    }
                    catch (FileNotFoundException)
                    {

                    }
                    catch (Exception ex)
                    {
                        if (ex.IsFileLocked())
                        {
                            // Файл открыт в стороннем процессе
                            if (MessageBox.Show("Файл \"" + saveFileDialog.SafeFileName + " открыт в стороннем приложении. Пожалуйста, закройте этот файл и повторите попытку.",
                                            "\"" + saveFileDialog.SafeFileName + "\" открыт", MessageBoxButton.OKCancel, MessageBoxImage.Error)
                                            == MessageBoxResult.Cancel
                                            )
                            {
                                return;
                            }

                            isError = true;

                        }
                        else
                        {
                            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    finally
                    {
                        fs?.Close();
                    }
                } while (isError);

                FsstFile.students[SelectedStudentIndex].studentDataToIndivProtocol(fileName);

            }


        }

        
    }
}
