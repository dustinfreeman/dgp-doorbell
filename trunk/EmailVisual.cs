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
    class EmailVisual : GestureWidget, IComparable<EmailVisual>
    {
        public string dgpID;
        public string emailAddress;
        public string GivenName;
        public string LastName;

        public const int EMAIL_WIDTH = 400;
        public const int EMAIL_HEIGHT = 170;
        public const int EMAIL_FONT_SIZE = 55;

        public Border border;
        TextBlock LastNameTxt;
        TextBlock GivenNameTxt;
        TextBlock EmailTxt;
        Rectangle HoverRectangle;

        public EmailVisual()
        {
            InitializeComponent();

            HoverDuration = TimeSpan.FromSeconds(STANDARD_HOVER_DURATION);

            LoadWidgets();
        }

        void LoadWidgets()
        {
            border = new Border();
            border.CornerRadius = new CornerRadius(6);
            border.BorderBrush = Brushes.Gray;
            border.Background = Brushes.LightGray;
            border.BorderThickness = new Thickness(2);
            border.Padding = new Thickness(8);

            Canvas BorderCanvas = new Canvas();
            border.Child = BorderCanvas;

            HoverRectangle = new Rectangle();
            HoverRectangle.Opacity = 0.5;
            HoverRectangle.Width = EMAIL_WIDTH;
            HoverRectangle.Height = EMAIL_HEIGHT;
            HoverRectangle.Fill = Brushes.Blue;
            HoverRectangle.Visibility = Visibility.Hidden;

            BorderCanvas.Children.Add(HoverRectangle);

            StackPanel sp = new StackPanel();
            sp.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);

            LastNameTxt = new TextBlock();
            LastNameTxt.FontSize = EMAIL_FONT_SIZE; LastNameTxt.FontWeight = FontWeights.Bold;
            LastNameTxt.Text = "LastName";
            sp.Children.Add(LastNameTxt);

            GivenNameTxt = new TextBlock();
            GivenNameTxt.FontSize = EMAIL_FONT_SIZE; GivenNameTxt.FontWeight = FontWeights.Bold;
            GivenNameTxt.Text = "GivenName";
            sp.Children.Add(GivenNameTxt);

            EmailTxt = new TextBlock();
            EmailTxt.FontSize = EMAIL_FONT_SIZE; EmailTxt.FontWeight = FontWeights.Bold;
            EmailTxt.Text = "EmailTxt";
            sp.Children.Add(EmailTxt);
            EmailTxt.Visibility = Visibility.Hidden;

            sp.Width = EMAIL_WIDTH;
            sp.Height = EMAIL_HEIGHT;

            BorderCanvas.Children.Add(sp);

            border.Width = EMAIL_WIDTH;
            border.Height = EMAIL_HEIGHT;

            widgetCanvas.Children.Add(border);
            //widgetCanvas.Children.Add(HoverRectangle);
            //widgetCanvas.Children.Add(sp);
        }

        public void UpdateNameData()
        {
            LastNameTxt.Text = LastName;
            GivenNameTxt.Text = GivenName;
            EmailTxt.Text = emailAddress;
        }

        protected override void StateChanged(WidgetState oldState)
        {
            switch (State)
            {
                case WidgetState.Inactive:
                    HoverRectangle.Visibility = Visibility.Hidden;
                    break;
                case WidgetState.Active:
                    HoverRectangle.Visibility = Visibility.Hidden;
                    RaiseActivated();
                    break;
                case WidgetState.Hovering:
                    HoverRectangle.Visibility = Visibility.Visible;
                    
                    break;
            }
        }

        public override void ControlPointUpdate(Point ctrlPt)
        {
            if (State == WidgetState.Hovering)
            {
                HoverRectangle.Height = HoverFraction * EMAIL_HEIGHT;
                HoverRectangle.SetValue(Canvas.TopProperty, (1 - HoverFraction) * EMAIL_HEIGHT);
            }
            base.ControlPointUpdate(ctrlPt);
        }

        public int CompareTo(EmailVisual other)
        {
            return String.Compare(this.LastName, other.LastName);
        }

        public static EmailVisual EmailVisualFactory(string line)
        {
            try
            {
                string[] lineSplit = line.Split(',');

                if (lineSplit[4] == "0")
                {   //account we don't want to use.
                    return null;
                }

                EmailVisual eListing = new EmailVisual();
                eListing.Width = EMAIL_WIDTH;
                eListing.Height = EMAIL_HEIGHT;
                eListing.dgpID = lineSplit[0];
                eListing.emailAddress = lineSplit[1];
                eListing.GivenName = lineSplit[2];
                eListing.LastName = lineSplit[3];

                eListing.UpdateNameData();

                return eListing;
            }
            catch (Exception e)
            {
                return null;
            }
        }

    }
}
