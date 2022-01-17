using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Проекты_8_9_Классы
{

    public class CriteriaScores : INotifyPropertyChanged
    {
        public byte[] Scores;
        public event PropertyChangedEventHandler PropertyChanged;
        public int Length
        {
            get { return Scores.Length; }
        }

        private CriteriaScores(int length)
        {
            Scores = new byte[length];
        }

        public byte this[int i, int j]
        {
            get
            {
                int index = i * 4 + j;
                if (index >= Scores.Length || i >= 4 || j >= 4)
                {
                    throw new Exception("Выход за пределы массива");
                }
                return Scores[index];
            }
            set
            {
                int index = i * 4 + j;
                if (index >= Scores.Length || i >= 4 || j >= 4)
                {
                    throw new Exception("Выход за пределы массива");
                }

                Scores[index] = value;
                OnPropertyChanged();
            }
        }

        public static CriteriaScores CreateCriteriaScores(PropertyChangedEventHandler action)
        {
            CriteriaScores cs = new CriteriaScores(4 + 4 + 4 + 2);
            cs.PropertyChanged += action;
            return cs;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }

    /// <summary>
    /// Логика взаимодействия для CriteriaTable.xaml
    /// </summary>
    public partial class CriteriaTable : UserControl
    {

        private double verticalScrollbarOffset = 0.0;

        public CriteriaScores criteriaScores { get; set; } = CriteriaScores.CreateCriteriaScores((Application.Current.MainWindow as MainWindow).CriteriaTable_PropertyChanged);
        public int SkipableScoresAmount = 0;

        public CriteriaTable()
        {
            InitializeComponent();
            IsVisibleChanged += (sender, e) => { scrollViewer.ScrollToVerticalOffset(0); };
            //SkipableScoresAmount = criteriaScores.Length - StudentStruct.GetCriteriaAmount();
            SkipableScoresAmount = criteriaScores.Length - 14;
        }

        private void scrollViewer_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            verticalScrollbarOffset += e.VerticalChange;

            if (!ItemBase.IsMouseOver)
                (sender as ScrollViewer).ScrollToVerticalOffset(verticalScrollbarOffset);
        }
    }
}
