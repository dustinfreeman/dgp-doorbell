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

        public CircularProgressPoly()
        {
            Poly = new Polygon();
            Poly.Fill = Brushes.AliceBlue;
            Poly.Stroke = Brushes.Black;
            Poly.StrokeThickness = 5;
            this.Children.Add(Poly);
            Update();
        }

        public void SetRadius(double radius)
        {
            this.radius = radius;
        }

        public void SetAngle(double angle)
        {
            this.angle = angle;
            Update();
        }

        void Update()
        {
            this.Poly.Points.Clear();
            this.Poly.Points.Add(new Point(radius, radius));

            for (double a = 0; a < Math.PI * 2; a += Math.PI / 40)
            {
                this.Poly.Points.Add(new Point(radius * Math.Cos(a) + radius, radius * Math.Sin(a) + radius));

                if (a > angle)
                {
                    break;
                }
            }

            this.Poly.Points.Add(new Point(radius, radius));

        }
    }
}
