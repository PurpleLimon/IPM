using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace IPM.res
{
    public abstract class Add_DeleteButtons
    {

        public class Button : System.Windows.Controls.Button
        {
            protected string StyleName;
            public Button(string StyleName) : base()
            {
                this.Style = FindResource(StyleName) as Style;
                this.StyleName = StyleName;
                Margin = new Thickness(10);
                Height = 25;
                FontSize = 10;
                Foreground = FindResource("HighLight") as System.Windows.Media.Brush;
            }
        }

        public class PlusButton : Button
        {
            public PlusButton(string StyleName) : base(StyleName)
            {
                Content = "+";
            }

            public PlusButton(string StyleName, int GridColumn, int GridRow, ref System.Windows.Controls.Grid grid) : this(StyleName)
            {
                this.SetValue(Grid.ColumnProperty, GridColumn);
                this.SetValue(Grid.RowProperty, GridRow);
                grid.Children.Add(this);
            }
        }

        public class MinusButton : Button
        {
            private Grid grid;

            public MinusButton(string StyleName) : base(StyleName)
            {
                Content = "-";

            }

            public MinusButton(string StyleName, int GridColumn, int GridRow, ref System.Windows.Controls.Grid grid) : this(StyleName)
            {
                this.SetValue(Grid.ColumnProperty, GridColumn);
                this.SetValue(Grid.RowProperty, GridRow);
                grid.Children.Add(this);
                this.grid = grid;
            }
        }

    }
}
