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
    /// <summary>
    /// Interaction logic for ScrollArrow.xaml
    /// </summary>
    public partial class ScrollArrow : UserControl, IGestureWidget
    {
        public event Action Activated;
        public event Action<double> ActivatedWParam;

        bool _selected = false;
        public bool Selected
        {
            get { return _selected;  }
            set 
            {
                _selected = value;
                ImgArrowLeftSelect.Visibility = Visibility.Hidden;
                ImgArrowRightSelect.Visibility = Visibility.Hidden;
                switch (Direction)
                {
                    case ScrollDirn.Left:
                        ImgArrowLeftSelect.Visibility = _selected ? Visibility.Visible : Visibility.Hidden;
                        break;
                    case ScrollDirn.Right:
                        ImgArrowRightSelect.Visibility = _selected ? Visibility.Visible : Visibility.Hidden;

                        break;
                }
                
            }
        }

        ScrollDirn Direction = ScrollDirn.Left;

        public ScrollArrow()
        {
            InitializeComponent();
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
                    ScrollingSign = -1;
                    break;
                case ScrollDirn.Right:
                    ImgArrowRight.Visibility = Visibility.Visible;
                    ScrollingSign = 1;
                    break;
            }
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            // http://msdn.microsoft.com/en-us/library/ms752097.aspx

            HitTestResult result = base.HitTestCore(hitTestParameters);

            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }

        public void ControlPointUpdate(Point ctrlPt)
        {
            if (!cpHits)
            {
                Selected = false;
            }
            else
            {
                //TODO do updating stuff.
                if (ActivatedWParam != null)
                {
                    ActivatedWParam(ScrollingSign);
                }
            }
            cpHits = false;
        }

        bool cpHits = false;
        public void ControlPointHits()
        {
            Selected = true;
            cpHits = true;
        }

        
    }

    public enum ScrollDirn
    {
        Left,
        Right
    }
}
