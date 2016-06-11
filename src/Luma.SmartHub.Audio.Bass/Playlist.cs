using System;
using System.Collections.Generic;
using System.Linq;
using Luma.SmartHub.Audio.Playback;

namespace Luma.SmartHub.Audio.Bass
{
    public class Playlist : IPlaylist
    {
        private IPlayback _currentTrack;

        public string Id { get; }
        public double? Duration => Tracks.Sum(c => c.Duration);

        public double Position
        {
            get { return Tracks.IndexOf(CurrentTrack); }
            set { GoToIndex((int)value); }
        }

        public string Name { get; set; }

        // TODO: Add valid implementation - this is temporary
        public double Volume
        {
            get { return CurrentTrack.Volume; }
            set { CurrentTrack.Volume = value; }
        }

        public IList<IPlayback> Tracks { get; }

        public bool IsPlaying => CurrentTrack?.IsPlaying == true;

        ICollection<IOutputAudioDevice> OutgoingConnections { get; }

        IEnumerable<IOutputAudioDevice> IPlayback.OutgoingConnections => OutgoingConnections;

        public event EventHandler Ended;

        public IPlayback CurrentTrack
        {
            get { return _currentTrack; }
            set
            {
                if (_currentTrack == value)
                    return;

                var oldTrack = _currentTrack;

                _currentTrack = value;

                OnCurrentTrackChanged(oldTrack, _currentTrack);
            }
        }

        private void OnCurrentTrackChanged(IPlayback oldTrack, IPlayback newTrack)
        {
            oldTrack?.Stop();
            //oldTrack?.ClearOutgoingConnections();

            newTrack?.AddOutgoingConnections(OutgoingConnections);
            newTrack?.Play();
        }

        public Playlist(IList<Uri> tracks, IPlaybackManager playbackManager)
            : this(tracks.ToPlaybackList(playbackManager)) { }

        public Playlist(IList<IPlayback> tracks = null)
        {
            Id = Guid.NewGuid().ToString();
            Tracks = tracks ?? new List<IPlayback>();
            OutgoingConnections = new HashSet<IOutputAudioDevice>();
        }

        public void Pause()
        {
            CurrentTrack?.Pause();
        }

        public void Play()
        {
            if (CurrentTrack == null)
            {
                CurrentTrack = Tracks.FirstOrDefault();
            }

            CurrentTrack?.Play();
        }

        public void Stop()
        {
            CurrentTrack?.Stop();

            CurrentTrack = Tracks.FirstOrDefault();
        }

        public void AddOutgoingConnection(IOutputAudioDevice audioDevice)
        {
            OutgoingConnections.Add(audioDevice);
            CurrentTrack?.AddOutgoingConnection(audioDevice);
        }

        public void RemoveOutgoingConnection(IOutputAudioDevice audioDevice)
        {
            OutgoingConnections.Remove(audioDevice);
            CurrentTrack?.RemoveOutgoingConnection(audioDevice);
        }

        public void Next()
        {
            var currentIndex = Tracks.IndexOf(CurrentTrack);
            GoToIndex(currentIndex + 1);
        }

        public void Prev()
        {
            var currentIndex = Tracks.IndexOf(CurrentTrack);
            GoToIndex(currentIndex - 1);
        }

        private void GoToIndex(int index)
        {
            if (index > Tracks.Count)
                index = 0;

            if (index < 0)
                index = Tracks.Count - 1;

            CurrentTrack = Tracks[index];
        }
    }
}
