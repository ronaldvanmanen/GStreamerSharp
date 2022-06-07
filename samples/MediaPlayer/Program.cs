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

namespace GStreamerSharp
{
    internal static class Program
    {
        [STAThread]
        public static int Main()
        {
            if (Environment.Is64BitProcess)
            {
                var path = Environment.GetEnvironmentVariable("PATH");
                path += ";C:\\GStreamer\\1.0\\msvc_x86_64\\bin";
                Environment.SetEnvironmentVariable("PATH", path);
            }
            else
            {
                var path = Environment.GetEnvironmentVariable("PATH");
                path += ";C:\\GStreamer\\1.0\\msvc_x86\\bin";
                Environment.SetEnvironmentVariable("PATH", path);
            }

            Gst.Application.Init();

            GtkSharp.GstreamerSharp.ObjectManager.Initialize();

            var application = new App();
            var mainWindow = new MainWindow();
            return application.Run(mainWindow);
        }
    }
}
