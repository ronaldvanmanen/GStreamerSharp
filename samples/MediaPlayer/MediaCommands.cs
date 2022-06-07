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

using System.Windows.Input;

namespace GStreamerSharp
{
    internal static class MediaCommands
    {
        static MediaCommands()
        {
            Play = new RoutedCommand("Play", typeof(MediaCommands));
            Stop = new RoutedCommand("Stop", typeof(MediaCommands));
            Pause = new RoutedCommand("Pause", typeof(MediaCommands));
            StepForward = new RoutedCommand("StepForward", typeof(MediaCommands));
            StepBackward = new RoutedCommand("StepBackward", typeof(MediaCommands));
            FastForward = new RoutedCommand("FastForward", typeof(MediaCommands));
            FastBackward = new RoutedCommand("FastBackward", typeof(MediaCommands));
        }

        public static RoutedCommand Play
        {
            get; private set;
        }

        public static RoutedCommand Stop
        {
            get; private set;
        }

        public static RoutedCommand Pause
        {
            get; private set;
        }

        public static RoutedCommand StepForward
        {
            get; private set;
        }

        public static RoutedCommand StepBackward
        {
            get; private set;
        }

        public static RoutedCommand FastForward
        {
            get; private set;
        }

        public static RoutedCommand FastBackward
        {
            get; private set;
        }
    }
}
