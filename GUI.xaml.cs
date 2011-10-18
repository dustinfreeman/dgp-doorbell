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
    /// Interaction logic for GUI.xaml
    /// </summary>
    public partial class GUI : UserControl
    {
        List<Image> guiImages = new List<Image>();

        public GUI()
        {
            InitializeComponent();

            this.Loaded +=new RoutedEventHandler(GUI_Loaded);
        }

        void GUI_Loaded(object sender, RoutedEventArgs e)
        {
            guiImages.Add(BlankImage);
            guiImages.Add(DownImage);
            guiImages.Add(LeftImage);
            guiImages.Add(RightImage);
            guiImages.Add(UpImage);

            foreach (Image img in guiImages)
            {
                img.Width = 200;
                img.SetValue(Canvas.LeftProperty, -img.ActualWidth / 2);
                img.SetValue(Canvas.TopProperty, -img.ActualHeight / 2);
            }
            ResetGUI();
        }

        public void Left()
        {
            HideAll();
            LeftImage.Visibility = Visibility.Visible;
        }

        public void Right()
        {
            HideAll();
            RightImage.Visibility = Visibility.Visible;
        }

        public void Up()
        {
            HideAll();
            UpImage.Visibility = Visibility.Visible;
        }

        public void ResetGUI()
        {
            HideAll();
            BlankImage.Visibility = Visibility.Visible;
        }

        void HideAll()
        {
            foreach (Image img in guiImages)
            {
                img.Visibility = Visibility.Hidden;
            }
        }
    }
}
