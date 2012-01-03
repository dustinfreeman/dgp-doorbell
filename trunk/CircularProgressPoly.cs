using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace DGPDoorbell
{
    class CircularProgressPoly:Canvas
    {
        Polygon Poly;
        double angle = 0;
        double radius = 100;

        const double DRAW_SMOOTHNESS = Math.PI / 40; 

        public CircularProgressPoly(double radius)
        {
            this.radius = radius;

            Poly = new Polygon();
            Poly.Fill = Brushes.Black;
            //Poly.Stroke = Brushes.Black;
            //Poly.StrokeThickness = 5;
            this.Children.Add(Poly);
            Update();
        }

        public void SetRadius(double radius)
        {
            this.radius = radius;
        }

        public void SetFraction(double fraction)
        {
            SetAngle(fraction * 2 * Math.PI);
        }

        public void SetAngle(double angle)
        {
            this.angle = angle;
            Update();
        }

        void Update()
        {
            this.Poly.Points.Clear();

            if (angle == 0)
                return;

            this.Poly.Points.Add(new Point(0, 0));

            for (double a = 0; a <= Math.PI * 2 + DRAW_SMOOTHNESS; a += DRAW_SMOOTHNESS)
            {
                if (a > angle)
                {
                    break;
                }
                this.Poly.Points.Add(new Point(radius * Math.Cos(a), radius * Math.Sin(a)));
            }

            this.Poly.Points.Add(new Point(0, 0));

        }
    }
}
