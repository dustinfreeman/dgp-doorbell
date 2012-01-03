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
    /// Interaction logic for GestureWidget.xaml
    /// </summary>
    public partial class GestureWidget : UserControl
    {
        protected TimeSpan HoverDuration = new TimeSpan(0); //time required to hover.
        protected TimeSpan HoverTime = new TimeSpan(0); //time spent hovering
        private DateTime _LastHoverIncrement = DateTime.Now;

        public double HoverFraction
        {
            get
            {
                return HoverTime.TotalMilliseconds / HoverDuration.TotalMilliseconds;
            }
        }

        WidgetState _state = WidgetState.Inactive;
        protected WidgetState State
        {
            get { return _state; }
            set
            {
                if (_state == value)
                    return; //no change

                _state = value;

                StateChanged();

            }
        }

        //called by visual parent to let the widget know the control point hits it.
        protected bool cpHits = false;
        public void ControlPointHits()
        {
            cpHits = true;
        }

        //called to actual do updating.
        virtual public void ControlPointUpdate(Point ctrlPt)
        {
            if (cpHits)
            {
                switch (State)
                {
                    case WidgetState.Inactive:
                        //start hover.
                        HoverTime = new TimeSpan(0);
                        State = WidgetState.Hovering;
                        _LastHoverIncrement = DateTime.Now;
                        break;
                    case WidgetState.Hovering:
                        //hover update
                        DateTime Now = DateTime.Now;
                        HoverTime += Now - _LastHoverIncrement;
                        if (HoverTime >= HoverDuration)
                        {
                            State = WidgetState.Active;
                        }
                        break;
                    case WidgetState.Active:

                        break;
                }
               
            } 
            else
            {
                State = WidgetState.Inactive;
            }
            cpHits = false;
        }

        public event Action Activated;
        public event Action<double> ActivatedWParam;
        protected void RaiseActivated()
        { if(Activated != null) Activated(); }
        protected void RaiseActivatedWParam(double obj)
        {
            ActivatedWParam(obj); 
        }

        virtual protected void StateChanged() { } //for visual stuff.

        public GestureWidget()
        {
            InitializeComponent();
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            // http://msdn.microsoft.com/en-us/library/ms752097.aspx

            HitTestResult result = base.HitTestCore(hitTestParameters);

            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }
    }

    public enum WidgetState
    {
        Inactive,
        Hovering,
        Active
    }
}
