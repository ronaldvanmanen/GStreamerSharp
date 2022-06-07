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
    [Flags]
    internal enum GstPlayFlags : uint
    {
        Video = 0x00000001, // Render the video stream
        Audio = 0x00000002, // Render the audio stream
        Text = 0x00000004, //  Render subtitles
        Vis = 0x00000008, // Render visualisation when no video is present
        SoftVolume = 0x00000010, // Use software volume
        NativeAudio = 0x00000020, // Only use native audio formats
        NativeVideo = 0x00000040, // Only use native video formats
        Download = 0x00000080, // Attempt progressive download buffering
        Buffering = 0x00000100, // Buffer demuxed/parsed data
        Deinterlace = 0x00000200, // Deinterlace video if necessary
        SoftColorbalance = 0x00000400, // Use software color balance
        ForceFilters = 0x00000800, // Force audio/video filter(s) to be applied
        ForceSwDecoders = 0x00001000 // Force only software-based decoders (no effect for playbin3)
    }
}
