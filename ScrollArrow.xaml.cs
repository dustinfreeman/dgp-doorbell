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

        ScrollDirn Direction = ScrollDirn.Left;

        public ScrollArrow()
        {
            InitializeComponent();
        }

        public void SetScrollDirn(ScrollDirn Direction)
        {
            this.Direction = Direction;

            switch (this.Direction)
            {
                case ScrollDirn.Left:

                    break;
                case ScrollDirn.Right:

                    break;
            }
            //TODO scroll arrow updates.
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            //taken from
            // http://msdn.microsoft.com/en-us/library/ms752097.aspx

            Point pt = hitTestParameters.HitPoint;
            return new PointHitTestResult(this, pt);
        }

        public void ControlPointUpdate(Point ctrlPt)
        {
            //TODO if not hit, do cancelling.

            //TODO do updating stuff.

            cpHits = false;
        }

        bool cpHits = false;
        public void ControlPointHits()
        {
            //TODO show visual hit feedback

            cpHits = true;
        }

        
    }

    public enum ScrollDirn
    {
        Left,
        Right
    }
}
