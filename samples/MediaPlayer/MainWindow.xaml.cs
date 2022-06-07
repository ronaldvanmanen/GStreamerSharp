﻿// This file is part of GStreamerSharp.
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
using System.Windows;
using System.Windows.Controls.Primitives;

namespace GStreamerSharp
{
    internal partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void PositionSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            MediaControl.Pause();
            MediaControl.Scrubbing = true;
        }

        private void PositionSlider_DragDelta(object sender, DragDeltaEventArgs e)
        {
            MediaControl.Position = new TimeSpan((long)PositionSlider.Value);
        }

        private void PositionSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            MediaControl.Scrubbing = false;
        }
    }
}