// This file is part of GStreamerSharp.
//
// GStreamerSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// GStreamerSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with GStreamerSharp.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;

namespace GStreamerSharp
{
    [TemplatePart(Name = PART_VideoHwndHost, Type = typeof(VideoHwndHost))]
    internal sealed class MediaControl : Control
    {
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                nameof(Source),
                typeof(string),
                typeof(MediaControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, SourceChanged));

        private static readonly DependencyPropertyKey DurationPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(Duration),
                typeof(TimeSpan),
                typeof(MediaControl),
                new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.None));

        public static readonly DependencyProperty DurationProperty = DurationPropertyKey.DependencyProperty;

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(
                nameof(Position),
                typeof(TimeSpan),
                typeof(MediaControl),
                new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.None, PositionChanged));

        public static readonly DependencyProperty SpeedRatioProperty =
            DependencyProperty.Register(
                nameof(SpeedRatio),
                typeof(double),
                typeof(MediaControl),
                new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.None, SpeedRatioChanged));

        public static readonly DependencyProperty ScrubbingProperty =
            DependencyProperty.Register(
                nameof(Scrubbing),
                typeof(bool),
                typeof(MediaControl),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None));

        private const string PART_VideoHwndHost = "PART_VideoHwndHost";

        private VideoHwndHost _videoHwndHost = null;

        private Gst.Element _playbin;

        private Thread _playbinBusThread;

        private Gst.Video.VideoOverlayAdapter _videoOverlayAdapter;

        public string Source
        {
            get => (string)GetValue(SourceProperty);

            set => SetValue(SourceProperty, value);
        }

        public TimeSpan Duration
        {
            get => (TimeSpan)GetValue(DurationProperty);

            private set => SetValue(DurationPropertyKey, value);
        }

        public TimeSpan Position
        {
            get => (TimeSpan)GetValue(PositionProperty);

            set => SetValue(PositionProperty, value);
        }

        public double SpeedRatio
        {
            get => (double)GetValue(SpeedRatioProperty);

            set => SetValue(SpeedRatioProperty, value);
        }

        public bool Scrubbing
        {
            get => (bool)GetValue(ScrubbingProperty);

            set => SetValue(ScrubbingProperty, value);
        }

        static MediaControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MediaControl), new FrameworkPropertyMetadata(typeof(MediaControl)));

            CommandManager.RegisterClassCommandBinding(typeof(MediaControl), new CommandBinding(MediaCommands.Play, ExecutePlay, CanExecutePlay));
            CommandManager.RegisterClassCommandBinding(typeof(MediaControl), new CommandBinding(MediaCommands.Stop, ExecuteStop, CanExecuteStop));
            CommandManager.RegisterClassCommandBinding(typeof(MediaControl), new CommandBinding(MediaCommands.Pause, ExecutePause, CanExecutePause));
            CommandManager.RegisterClassCommandBinding(typeof(MediaControl), new CommandBinding(MediaCommands.StepForward, ExecuteStepForward, CanExecuteStepForward));
            CommandManager.RegisterClassCommandBinding(typeof(MediaControl), new CommandBinding(MediaCommands.StepBackward, ExecuteStepBackward, CanExecuteStepBackward));
            CommandManager.RegisterClassCommandBinding(typeof(MediaControl), new CommandBinding(MediaCommands.FastForward, ExecuteFastForward, CanExecuteFastForward));
            CommandManager.RegisterClassCommandBinding(typeof(MediaControl), new CommandBinding(MediaCommands.FastBackward, ExecuteFastBackward, CanExecuteFastBackward));
        }

        public MediaControl()
        {
            Loaded += HandleLoadedEvent;
            Unloaded += HandleUnloadedEvent;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _videoHwndHost = (VideoHwndHost)GetTemplateChild(PART_VideoHwndHost);
        }

        private void Load()
        {
            _playbin = Gst.ElementFactory.Make("playbin", "playbin");
            _playbin.Bus.EnableSyncMessageEmission();
            _playbin.Bus.SyncMessage += BusSyncMessageReceived;
            _playbin["uri"] = Gst.Util.UriIsValid(Source) ? Source : Gst.Util.FilenameToUri(Source);

            var flags = (GstPlayFlags)_playbin["flags"];
            flags |= GstPlayFlags.Video | GstPlayFlags.Audio | GstPlayFlags.Text;
            _playbin["flags"] = (uint)flags;

            _playbinBusThread = new Thread(PopBusMessages);
            _playbinBusThread.Start();
        }

        private void Unload()
        {
            if (_playbin != null)
            {
                _playbin.SetState(Gst.State.Null);
                _playbin.Bus.DisableSyncMessageEmission();
                _playbin.Bus.SyncMessage -= BusSyncMessageReceived;
                _playbin.Dispose();
                _playbin = null;
            }

            if (_playbinBusThread != null)
            {
                _playbinBusThread.Abort();
                _playbinBusThread = null;
            }

            _videoOverlayAdapter = null;
        }

        public void Play()
        {
            SpeedRatio = 1d;
        }

        public void Stop()
        {
            _playbin.SetState(Gst.State.Ready);
        }

        public void Pause()
        {
            SpeedRatio = 0d;
        }

        public void StepForward()
        {
            var videoSink = (Gst.Element)_playbin["video-sink"];
            if (videoSink == null)
            {
                return;
            }

            videoSink.SendEvent(
                Gst.Event.NewStep(Gst.Format.Buffers, 1, 1d, true, false)
            );
        }

        public void StepBackward()
        {
        }

        public void FastForward()
        {
            SpeedRatio += .5d;
        }

        public void FastBackward()
        {
            SpeedRatio -= .5d;
        }

        private void HandleLoadedEvent(object sender, RoutedEventArgs eventArgs)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.Closing += HandleClosingEvent;
            }
        }

        private void HandleUnloadedEvent(object sender, RoutedEventArgs eventArgs)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.Closing -= HandleClosingEvent;
            }
        }

        private void HandleClosingEvent(object sender, CancelEventArgs eventArgs)
        {
            Unload();
        }

        private void SourceChanged()
        {
            Unload();
            Load();
            Play();
        }

        private void PositionChanged()
        {
            if (Scrubbing)
            {
                _playbin.SeekSimple(Gst.Format.Time, Gst.SeekFlags.Flush | Gst.SeekFlags.KeyUnit, Position.Ticks * 100);
            }
        }

        private void SpeedRatioChanged()
        {
            if (!_playbin.QueryPosition(Gst.Format.Time, out var position))
            {
                position = 0;
            }

            if (SpeedRatio > 0d)
            {
                _playbin.SetState(Gst.State.Playing);

                var videoSink = (Gst.Element)_playbin["video-sink"];
                if (videoSink != null)
                {
                    videoSink.SendEvent(
                        Gst.Event.NewSeek(SpeedRatio,
                            Gst.Format.Time,
                            Gst.SeekFlags.Flush | Gst.SeekFlags.Accurate,
                            Gst.SeekType.Set,
                            position,
                            Gst.SeekType.End,
                            0)
                        );
                }
            }
            else if (SpeedRatio < 0d)
            {
                _playbin.SetState(Gst.State.Playing);

                var videoSink = (Gst.Element)_playbin["video-sink"];
                if (videoSink != null)
                {
                    videoSink.SendEvent(
                    Gst.Event.NewSeek(SpeedRatio,
                        Gst.Format.Time,
                        Gst.SeekFlags.Flush | Gst.SeekFlags.Accurate,
                        Gst.SeekType.Set,
                        0,
                        Gst.SeekType.Set,
                        position)
                    );
                }
            }
            else
            {
                _playbin.SetState(Gst.State.Paused);
            }
        }

        private void PopBusMessages()
        {
            try
            {
                while (true)
                {
                    var message = _playbin.Bus.TimedPopFiltered(100 * Gst.Constants.MSECOND, Gst.MessageType.StateChanged | Gst.MessageType.Error | Gst.MessageType.Eos);
                    if (message != null)
                    {
                        switch (message.Type)
                        {
                            case Gst.MessageType.Error:
                                message.ParseError(out GLib.GException exception, out string debug);
                                Console.Error.WriteLine($"Error message: {exception.Message} ({debug})");
                                break;
                            case Gst.MessageType.Eos:
                                _playbin.SetState(Gst.State.Ready);
                                break;
                            case Gst.MessageType.StateChanged:
                                break;
                        }
                    }
                    else
                    {
                        if (_playbin.QueryDuration(Gst.Format.Time, out var duration) &&
                            _playbin.QueryPosition(Gst.Format.Time, out var position))
                        {
                            Dispatcher.Invoke(() =>
                            {
                                Duration = new TimeSpan(duration / 100);
                                Position = new TimeSpan(position / 100);
                            });
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
        }

        private void BusSyncMessageReceived(object sender, Gst.SyncMessageArgs eventArgs)
        {
            var message = eventArgs.Message;
            if (Gst.Video.Global.IsVideoOverlayPrepareWindowHandleMessage(message))
            {
                VideoOverlayPrepareWindowsHandleMessageReceived(message);
            }
        }

        private void VideoOverlayPrepareWindowsHandleMessageReceived(Gst.Message message)
        {
            if (message.Src is Gst.Element source)
            {
                try
                {
                    source["force-aspect-ratio"] = true;
                }
                catch (Gst.PropertyNotFoundException)
                {
                    /* Don't care */
                }
            }

            if (message.Src is Gst.Bin bin)
            {
                var overlay = bin.GetByInterface(Gst.Video.VideoOverlayAdapter.GType);
                if (overlay != null)
                {
                    _videoOverlayAdapter = new Gst.Video.VideoOverlayAdapter(overlay.Handle);
                    _videoOverlayAdapter.WindowHandle = _videoHwndHost.Handle;
                    _videoOverlayAdapter.Expose();
                }
            }
        }

        private static void ExecutePlay(object source, ExecutedRoutedEventArgs eventArgs)
        {
            if (source is MediaControl mediaControl)
            {
                mediaControl.Play();
            }
        }

        private static void CanExecutePlay(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = true;
        }

        private static void ExecuteStop(object source, ExecutedRoutedEventArgs eventArgs)
        {
            if (source is MediaControl mediaControl)
            {
                mediaControl.Stop();
            }
        }

        private static void CanExecuteStop(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = true;
        }

        private static void ExecutePause(object source, ExecutedRoutedEventArgs eventArgs)
        {
            if (source is MediaControl mediaControl)
            {
                mediaControl.Pause();
            }
        }

        private static void CanExecutePause(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = true;
        }

        private static void ExecuteStepForward(object source, ExecutedRoutedEventArgs eventArgs)
        {
            if (source is MediaControl mediaControl)
            {
                mediaControl.StepForward();
            }
        }

        private static void CanExecuteStepForward(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = true;
        }

        private static void ExecuteStepBackward(object source, ExecutedRoutedEventArgs eventArgs)
        {
            if (source is MediaControl mediaControl)
            {
                mediaControl.StepBackward();
            }
        }

        private static void CanExecuteStepBackward(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = true;
        }

        private static void ExecuteFastForward(object source, ExecutedRoutedEventArgs eventArgs)
        {
            if (source is MediaControl mediaControl)
            {
                mediaControl.FastForward();
            }
        }

        private static void CanExecuteFastForward(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = true;
        }

        private static void ExecuteFastBackward(object source, ExecutedRoutedEventArgs eventArgs)
        {
            if (source is MediaControl mediaControl)
            {
                mediaControl.FastBackward();
            }
        }

        private static void CanExecuteFastBackward(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = true;
        }

        private static void SourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            if (dependencyObject is MediaControl mediaControl)
            {
                mediaControl.SourceChanged();
            }
        }

        private static void PositionChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            if (dependencyObject is MediaControl mediaControl)
            {
                mediaControl.PositionChanged();
            }
        }

        private static void SpeedRatioChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            if (dependencyObject is MediaControl mediaControl)
            {
                mediaControl.SpeedRatioChanged();
            }
        }
    }
}
