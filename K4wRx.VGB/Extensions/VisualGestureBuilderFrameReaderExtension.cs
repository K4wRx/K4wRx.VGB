using Microsoft.Kinect.VisualGestureBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using K4wRx.Extensions;

namespace K4wRx.VGB.Extensions
{
    public static class VisualGestureBuilderFrameReaderExtension
    {
        public static IObservable<VisualGestureBuilderFrameArrivedEventArgs> AsObservable(this VisualGestureBuilderFrameReader reader)
        {
            return BaseReaderExtension.AsObservable<VisualGestureBuilderFrameArrivedEventArgs>(reader);
        }
    }
}
