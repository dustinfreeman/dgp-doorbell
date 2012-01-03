using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DGPDoorbell
{
    public class ScrollArrow : GestureWidget
    {
        Image ImgArrowLeft = new Image();
        Image ImgArrowLeftSelect = new Image();
        Image ImgArrowRight = new Image();
        Image ImgArrowRightSelect = new Image();
        

        protected override void StateChanged()
        {
            ImgArrowLeftSelect.Visibility = Visibility.Hidden;
            ImgArrowRightSelect.Visibility = Visibility.Hidden;
            switch (Direction)
            {
                case ScrollDirn.Left:
                    ImgArrowLeftSelect.Visibility = State == WidgetState.Active ? Visibility.Visible : Visibility.Hidden;
                    break;
                case ScrollDirn.Right:
                    ImgArrowRightSelect.Visibility = State == WidgetState.Active ? Visibility.Visible : Visibility.Hidden;
                    break;
            }
        }

        ScrollDirn Direction = ScrollDirn.Left;

        public ScrollArrow()
        {
            InitializeComponent();
            LoadArrows();
            
        }

        double ArrowWidth = 100;
        void LoadArrows()
        {
            ImgArrowLeft.Source = (BitmapImage)this.Resources["gui/ArrowLeft.png"];
            ImgArrowLeftSelect.Source = (BitmapImage)this.Resources["gui/ArrowLeftSelect.png"];
            ImgArrowRight.Source = (BitmapImage)this.Resources["gui/ArrowRight.png"];
            ImgArrowRightSelect.Source = (BitmapImage)this.Resources["gui/ArrowRightSelect.png"];

            List<Image> ImgArrows = new List<Image>() { ImgArrowLeft, ImgArrowLeftSelect, ImgArrowRight, ImgArrowRightSelect };

            ImgArrowLeft.Source = new BitmapImage(new Uri("pack://application:,,,/gui/" + "ArrowLeft.png", UriKind.RelativeOrAbsolute));
            ImgArrowLeftSelect.Source = new BitmapImage(new Uri("pack://application:,,,/gui/" + "ArrowLeftSelect.png", UriKind.RelativeOrAbsolute));
            ImgArrowRight.Source = new BitmapImage(new Uri("pack://application:,,,/gui/" + "ArrowRight.png", UriKind.RelativeOrAbsolute));
            ImgArrowRightSelect.Source = new BitmapImage(new Uri("pack://application:,,,/gui/" + "ArrowRightSelect.png", UriKind.RelativeOrAbsolute));

            foreach (Image img in ImgArrows)
            {
                img.Width = ArrowWidth;
                img.Height = ArrowWidth;
                img.SetValue(Canvas.TopProperty, -ArrowWidth / 2.0);
                img.SetValue(Canvas.LeftProperty, -ArrowWidth / 2.0);
                widgetCanvas.Children.Add(img);
                img.Visibility = Visibility.Hidden;
            }

        }

        double ScrollingSign = 1;
        public void SetScrollDirn(ScrollDirn Direction)
        {
            this.Direction = Direction;

            ImgArrowLeft.Visibility = Visibility.Hidden;
            ImgArrowLeftSelect.Visibility = Visibility.Hidden;
            ImgArrowRight.Visibility = Visibility.Hidden;
            ImgArrowRightSelect.Visibility = Visibility.Hidden;

            switch (this.Direction)
            {
                case ScrollDirn.Left:
                    ImgArrowLeft.Visibility = Visibility.Visible;
                    ScrollingSign = 1;
                    break;
                case ScrollDirn.Right:
                    ImgArrowRight.Visibility = Visibility.Visible;
                    ScrollingSign = -1;
                    break;
            }
        }

        public override void ControlPointUpdate(Point ctrlPt)
        {
            if (cpHits && State == WidgetState.Active)
            {
                RaiseActivatedWDouble(ScrollingSign);
            }
            base.ControlPointUpdate(ctrlPt);
        }

        
    }

    public enum ScrollDirn
    {
        Left,
        Right
    }
}
