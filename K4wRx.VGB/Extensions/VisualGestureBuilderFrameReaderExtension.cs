using Microsoft.Kinect.VisualGestureBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K4wRx.Extensions
{
    public static class VisualGestureBuilderFrameReaderExtension
    {
        public static IObservable<VisualGestureBuilderFrameArrivedEventArgs> AsObservable(this VisualGestureBuilderFrameReader reader)
        {
            return BaseReaderExtension.AsObservable<VisualGestureBuilderFrameArrivedEventArgs>(reader);
        }
    }
}
