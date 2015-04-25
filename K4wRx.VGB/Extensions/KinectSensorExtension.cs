using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.ComponentModel;
using Microsoft.Kinect.VisualGestureBuilder;
using K4wRx.Utils;
using K4wRx.Extensions;

namespace K4wRx.VGB.Extensions
{
    public static class KinectSensorExtension
    {
        private static CompositeDisposable disposables = new CompositeDisposable();
        /// <summary>
        /// Create VisualGestureBuilder stream from KinectSensor
        /// </summary>
        /// <param name="sensor">source of stream</param>
        /// <param name="gestures">gestures to detect</param>
        /// <param name="frameSource">out frame source instance to set tracking id</param>
        /// <returns>Observable VisualGestureBuidler frame stream</returns>
        public static IObservable<VisualGestureBuilderFrameArrivedEventArgs> VisualGestureBuilderFrameAsObservable(this KinectSensor sensor, IEnumerable<Gesture> gestures)
        {
            var frameSource = new VisualGestureBuilderFrameSource(sensor, 0);
            frameSource.AddGestures(gestures);
            var reader = frameSource.OpenReader();
            reader.IsPaused = false;
            disposables.Add(reader);
            return reader.AsObservable();
        }
        /// <summary>
        /// Create VisualGestureBuilder's DiscreteGestureResult stream for tracked bodies.
        /// </summary>
        /// <param name="sensor">source of stream</param>
        /// <param name="databasePath">path to database</param>
        /// <returns>Observable DiscreteGestureResult frame stream for tracked bodies.</returns>
        public static IObservable<IEnumerable<IDictionary<Gesture, DiscreteGestureResult>>> TrackedBodyDiscreteGestureResultsAsObservable(this KinectSensor sensor, string databasePath)
        {
            var frameStreams = Enumerable.Range(1, sensor.BodyFrameSource.BodyCount).Select(_ =>
            {
                List<Gesture> gestures = null;
                using (VisualGestureBuilderDatabase db = new VisualGestureBuilderDatabase(databasePath))
                {
                    gestures = db.AvailableGestures.Where(g => g.GestureType == GestureType.Discrete).ToList();
                }
                return sensor.VisualGestureBuilderFrameAsObservable(gestures)
                    .Select(e => e.FrameReference.AcquireFrame())
                    .Where(frame => frame != null)
                    .Select(frame => frame);
            });

            // maximum number of synchronized vgb frame event args will flow into this stream
            var zippedStreams = KinectSensorExtensionUtil.ZipStreams(frameStreams);
            // tracking ids that are tracked now will flow into this stream
            var trackingIdStream = sensor.BodyFrameSource.OpenReader().TrackedBodyAsObservable().Select(bodies => bodies.Select(b => b.TrackingId));

            return zippedStreams.Zip(trackingIdStream, (frames, ids) =>
            {
                // find ids that is not contained in VGBFrames
                var i = ids.Where(id => !frames.Where(f => f.TrackingId != 0 || f.IsTrackingIdValid).Select(f => f.TrackingId).Contains(id)).GetEnumerator();
                foreach (var f in frames.Where(f => f.TrackingId == 0 || !f.IsTrackingIdValid))
                {
                    if (i.MoveNext())
                    {
                        f.VisualGestureBuilderFrameSource.TrackingId = i.Current;
                    }
                }

                return frames.Select(frame =>
                {
                    if (frame.DiscreteGestureResults != null)
                    {
                        Dictionary<Gesture, DiscreteGestureResult> dic = frame
                            .DiscreteGestureResults.ToDictionary(kv => kv.Key, kv => kv.Value);
                        
                        frame.Dispose();
                        return dic;
                    }
                    else
                    {
                        return new Dictionary<Gesture, DiscreteGestureResult>();
                    }
                }).ToList();
            });
        }
        /// <summary>
        /// Create VisualGestureBuilder's ContinuousGestureResult stream for tracked bodies.
        /// </summary>
        /// <param name="sensor">source of stream</param>
        /// <param name="databasePath">path to database</param>
        /// <returns>Observable ContinuousGestureResult frame stream for tracked bodies.</returns>
        public static IObservable<IEnumerable<IDictionary<Gesture, ContinuousGestureResult>>> TrackedBodyContinuousGestureResultsAsObservable(this KinectSensor sensor, string databasePath)
        {
            var frameStreams = Enumerable.Range(1, sensor.BodyFrameSource.BodyCount).Select(_ =>
            {
                List<Gesture> gestures = null;
                using (VisualGestureBuilderDatabase db = new VisualGestureBuilderDatabase(databasePath))
                {
                    gestures = db.AvailableGestures.Where(g => g.GestureType == GestureType.Continuous).ToList();
                }
                return sensor.VisualGestureBuilderFrameAsObservable(gestures)
                    .Select(e => e.FrameReference.AcquireFrame())
                    .Where(frame => frame != null)
                    .Select(frame => frame);
            });

            // maximum number of synchronized vgb frame event args will flow into this stream
            var zippedStreams = KinectSensorExtensionUtil.ZipStreams(frameStreams);
            // tracking ids that are tracked now will flow into this stream
            var trackingIdStream = sensor.BodyFrameSource.OpenReader().TrackedBodyAsObservable().Select(bodies => bodies.Select(b => b.TrackingId));

            return zippedStreams.Zip(trackingIdStream, (frames, ids) =>
            {
                // find ids that is not contained in VGBFrames
                var i = ids.Where(id => !frames.Where(f => f.TrackingId != 0 || f.IsTrackingIdValid).Select(f => f.TrackingId).Contains(id)).GetEnumerator();
                foreach (var f in frames.Where(f => f.TrackingId == 0 || !f.IsTrackingIdValid))
                {
                    if (i.MoveNext())
                    {
                        f.VisualGestureBuilderFrameSource.TrackingId = i.Current;
                    }
                }

                return frames.Select(frame =>
                {
                    if (frame.ContinuousGestureResults != null)
                    {
                        Dictionary<Gesture, ContinuousGestureResult> dic = frame
                            .ContinuousGestureResults.ToDictionary(kv => kv.Key, kv => kv.Value);

                        frame.Dispose();
                        return dic;
                    }
                    else
                    {
                        return new Dictionary<Gesture, ContinuousGestureResult>();
                    }
                }).ToList();
            });
        }
    }
}
