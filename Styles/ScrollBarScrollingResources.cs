using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Проекты_8_9_Классы.Styles
{
    public class ScrollBarScrollingResources
    {
        public void OnThumbDragDelta(object sender, DragDeltaEventArgs args)
        {
            Canvas.SetLeft(sender as UIElement, Canvas.GetLeft(sender as UIElement) + args.HorizontalChange);
            Canvas.SetTop(sender as UIElement, Canvas.GetTop(sender as UIElement) + args.VerticalChange);
        }
    }
}
