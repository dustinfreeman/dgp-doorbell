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
    /// Interaction logic for UserFrame.xaml
    /// </summary>
    public partial class UserFrame : UserControl
    {
        //presents an interface to a single user.

        public int CurrentSkeletonID = -1;

        public UserFrame()
        {
            InitializeComponent();

        }


        public void ControlPointAppear(Point pt, int ID)
        {
            CurrentSkeletonID = ID;
            HandEllipse.Visibility = Visibility.Visible;
            ControlPointUpdate(pt);
        }

        public void ControlPointUpdate(Point pt)
        {
            HandEllipse.SetValue(Canvas.LeftProperty, pt.X - HandEllipse.Width);
            HandEllipse.SetValue(Canvas.TopProperty, pt.Y - HandEllipse.Height);
        }

        public void ControlPointLose()
        {
            HandEllipse.Visibility = Visibility.Hidden;
        }

    }
}
