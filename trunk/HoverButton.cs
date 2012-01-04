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
    class HoverButton:GestureWidget
    {
        double Radius = 80;
        double ProgressThickness = 15;

        CircularProgressPoly progressPoly;
        TextBlock Text;

        public HoverButton()
        {
            InitializeComponent();
            LoadComponents();

            HoverDuration = TimeSpan.FromSeconds(STANDARD_HOVER_DURATION);
        }

        void LoadComponents()
        {
            Ellipse main = new Ellipse();
            main.Width = (Radius-ProgressThickness) * 2;
            main.Height = (Radius-ProgressThickness) * 2;
            main.Fill = Brushes.White;
            main.SetValue(Canvas.TopProperty, -(Radius - ProgressThickness));
            main.SetValue(Canvas.LeftProperty, -(Radius - ProgressThickness));
            main.Opacity = 0.7;

            progressPoly = new CircularProgressPoly(Radius);
            progressPoly.Opacity = 0.8;

            Text = new TextBlock();
            Text.FontSize = 40; Text.FontWeight = FontWeights.Bold;
            //Text.Foreground = Brushes.White;
            SetText("Button");

            widgetCanvas.Children.Add(progressPoly);
            widgetCanvas.Children.Add(main);
            widgetCanvas.Children.Add(Text);
        }

        public void SetText(string newText)
        {
            Text.Text = newText;
            Text.Measure(new Size(800, 600)); //I have no idea what I'm doing - Dustin
            Text.SetValue(Canvas.LeftProperty, -Text.DesiredSize.Width / 2.0);
            Text.SetValue(Canvas.TopProperty, -Text.DesiredSize.Height / 2.0);
        }

        protected override void StateChanged(WidgetState oldState)
        {
            if (State == WidgetState.Inactive)
            {
                progressPoly.SetFraction(0);
            }
            if (State == WidgetState.Active)
            {
                progressPoly.SetFraction(1);
                RaiseActivated();
            }
        }

        public override void ControlPointUpdate(Point ctrlPt)
        {
            if (State == WidgetState.Hovering)
            {
                progressPoly.SetFraction(HoverFraction);
            }
            base.ControlPointUpdate(ctrlPt);
        }
    }
}
