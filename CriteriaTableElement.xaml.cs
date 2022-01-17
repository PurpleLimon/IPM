using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Проекты_8_9_Классы
{
    //public class CheckedValue: INotifyPropertyChanged
    //{
    //    public byte value
    //    {
    //        get { return value; }
    //        set { this.value = value;
    //            OnPropertyChanged();
    //        }
    //    }

    //    public event PropertyChangedEventHandler PropertyChanged;

    //    public CheckedValue(byte value)
    //    {
    //        this.value = value;
    //    }

    //    protected void OnPropertyChanged([CallerMemberName] string name = null)
    //    {
    //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    //    }
    //}

    /// <summary>
    /// Логика взаимодействия для CriteriaTableElement.xaml
    /// </summary>
    public partial class CriteriaTableElement : UserControl
    {

        public static readonly DependencyProperty HintContentDependencyProperty = DependencyProperty.Register("HintContent", typeof(UIElement), typeof(CriteriaTableElement));
        public static readonly DependencyProperty LabelContentDependancyProperty = DependencyProperty.Register("LabelContent", typeof(string), typeof(CriteriaTableElement));
        public static readonly DependencyProperty CriteriaTextDependancyProperty = DependencyProperty.Register("CriteriaText", typeof(string), typeof(CriteriaTableElement));

        private bool IsHintOpened = false;
        private Storyboard popupAnimation;
        private Storyboard VanishAnimation;

        #region Properties

        public UIElement HintContent
        {
            get
            {
                return GetValue(HintContentDependencyProperty) as UIElement;
            }

            set
            {
                SetValue(HintContentDependencyProperty, value);
            }
        }

        public string LabelContent
        {
            get
            {
                return GetValue(LabelContentDependancyProperty) as string;
            }
            set
            {
                SetValue(LabelContentDependancyProperty, value);
            }
        }

        public string CriteriaText
        {
            get { return GetValue(CriteriaTextDependancyProperty) as string; }
            set { SetValue(CriteriaTextDependancyProperty, value); }
        }

        //public CheckedValue checkedValue = new CheckedValue(0);

        #endregion

        private void AddAllSubElements()
        {
            if (LabelContent != null)
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Text = LabelContent;
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                textBlock.Background = null;
                textBlock.FontSize = FontSize - 2;
                LabelBorder.Child = textBlock;
            }
            if (CriteriaText != null)
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Text = CriteriaText;
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                textBlock.HorizontalAlignment = HorizontalAlignment.Left;
                textBlock.TextWrapping = TextWrapping.Wrap;
                textBlock.Padding = new Thickness(10, 0, 0, 0);
                textBlock.Background = null;

                CriteriaBorder.Child = textBlock;
            }

        }
        private double CalculateWidth()
        {
            return Column_0.ActualWidth + Column_1.ActualWidth;
        }
        public CriteriaTableElement()
        {
            InitializeComponent();

            //checkedValue.PropertyChanged += CheckedValue_PropertyChanged;

            Loaded += delegate
            {

                AddAllSubElements();
                foreach (TextBlock element in ((HintContent as Panel).Children.Cast<TextBlock>()))
                {
                    element.Margin = new Thickness(4, 4, 4, 4);
                }

                HintScrollViewer.Content = HintContent;
            };

            popupAnimation = new Storyboard();
            Storyboard.SetTarget(popupAnimation, HintCanvas);
            Storyboard.SetTargetProperty(popupAnimation, new PropertyPath(Canvas.WidthProperty));

            VanishAnimation = new Storyboard();
            Storyboard.SetTarget(VanishAnimation, HintCanvas);
            Storyboard.SetTargetProperty(VanishAnimation, new PropertyPath(Canvas.WidthProperty));

        }

        private void QuestionMark_MouseUp(object sender, MouseButtonEventArgs e)
        {

            if (!IsHintOpened)
            {

                popupAnimation.Children.Clear();

                DoubleAnimationUsingKeyFrames popupFrames = new DoubleAnimationUsingKeyFrames();

                double AllowedWidth = CalculateWidth();

                popupFrames.Duration = new Duration(TimeSpan.FromSeconds(0.75));
                popupFrames.KeyFrames.Add(new LinearDoubleKeyFrame(AllowedWidth * 0.8, KeyTime.FromPercent(0.5)));
                popupFrames.KeyFrames.Add(new LinearDoubleKeyFrame(AllowedWidth, KeyTime.FromPercent(1)));

                popupAnimation.AutoReverse = false;
                popupAnimation.Children.Add(popupFrames);
                popupAnimation.Begin(this);

                IsHintOpened = true;

            }
            else
            {
                VanishAnimation.Children.Clear();

                DoubleAnimationUsingKeyFrames vanishFrames = new DoubleAnimationUsingKeyFrames();

                double AllowedWidth = CalculateWidth();

                vanishFrames.Duration = new Duration(TimeSpan.FromSeconds(0.75));
                vanishFrames.KeyFrames.Add(new LinearDoubleKeyFrame(AllowedWidth * 0.2, KeyTime.FromPercent(0.5)));
                vanishFrames.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(1)));

                VanishAnimation.AutoReverse = false;
                VanishAnimation.Children.Add(vanishFrames);
                VanishAnimation.Begin();

                IsHintOpened = false;
            }

        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //if (IsHintOpened)
            //{
            //    HintCanvas.SetValue(Canvas.WidthProperty, CalculateWidth());
            //}
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            int i = Convert.ToInt16(LabelContent[0].ToString());
            int j = Convert.ToInt16(LabelContent[2].ToString());
            (Application.Current.MainWindow as MainWindow).criteriaTable.criteriaScores[i - 1, j - 1] = Convert.ToByte((sender as RadioButton).Content);



        }
    }
}
