using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DGPDoorbell
{
    interface IGestureWidget
    {
        //called by visual parent to let the widget know the control point hits it.
        void ControlPointHits();

        //called to actual do updating.
        void ControlPointUpdate(Point ctrlPt);

        event Action Activated;
        event Action<double> ActivatedWParam;
    }
}
