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
using System.Windows;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;

namespace MediaPlayer
{
    public sealed class MainWindowViewModel : ObservableObject
    {
        private RelayCommand _openFileCommand = null;

        private RelayCommand _exitCommand = null;

        private Uri _mediaSource = null;

        public ICommand OpenFileCommand
        {
            get
            {
                if (_openFileCommand == null)
                {
                    _openFileCommand = new RelayCommand(OpenFile);
                }
                return _openFileCommand;
            }
        }

        public ICommand ExitCommand
        {
            get
            {
                if (_exitCommand == null)
                {
                    _exitCommand = new RelayCommand(Exit);
                }
                return _exitCommand;
            }
        }

        public Uri MediaSource
        {
            get => _mediaSource;
            set
            {
                OnPropertyChanging(nameof(MediaSource));
                _mediaSource = value;
                OnPropertyChanged(nameof(MediaSource));
            }
        }

        private void OpenFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Video Files (*.asf;*.mkv;*.mov;*.mp4;*.wma;*.wmv)|*.asf;*.mkv;*.mov;*.mp4;*.wma;*.wmv|"
                       + "Audio Files (*.flac;*.mp3;*.wav)|*.flac;*.mp3;*.wav|"
                       + "All files (*.*)|*.*",
                Multiselect = false
            };

            var result = openFileDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                MediaSource = new Uri(openFileDialog.FileName);
            }
        }

        private void Exit()
        {
            Application.Current.Shutdown();
        }
    }
}
