﻿#if CLIENT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntryEngine
{
    public static partial class __AUDIO
    {
        private static EntryEngine.AUDIO __instance { get { return Entry._AUDIO; } }
        public static EntryEngine.IAudioSource Listener
        {
            get { return __instance.Listener; }
            set { __instance.Listener = value; }
        }
        public static float MaxDistance
        {
            get { return __instance.MaxDistance; }
            set { __instance.MaxDistance = value; }
        }
        public static EntryEngine.SOUND BGM
        {
            get { return __instance.BGM; }
        }
        public static float Volume
        {
            get { return __instance.Volume; }
            set { __instance.Volume = value; }
        }
        public static bool Mute
        {
            get { return __instance.Mute; }
            set { __instance.Mute = value; }
        }
        public static EntryEngine.VECTOR2 ListenerLocation
        {
            get { return __instance.ListenerLocation; }
        }
        public static bool IsDisposed
        {
            get { return __instance.IsDisposed; }
        }
        public static EntryEngine.ContentManager Content
        {
            get { return __instance.Content; }
            set { __instance.Content = value; }
        }
        public static void StopMusic()
        {
            __instance.StopMusic();
        }
        public static void StopVoice(object key)
        {
            __instance.StopVoice(key);
        }
        public static void PauseMusic()
        {
            __instance.PauseMusic();
        }
        public static void Pause(EntryEngine.SOUND sound)
        {
            __instance.Pause(sound);
        }
        public static void ResumeMusic()
        {
            __instance.ResumeMusic();
        }
        public static void Resume(EntryEngine.SOUND sound)
        {
            __instance.Resume(sound);
        }
        public static void PlayMusic(string name, System.Action<EntryEngine.SOUND> callback)
        {
            __instance.PlayMusic(name, callback);
        }
        public static void PlayMusic(string name, EntryEngine.IAudioSource source, System.Action<EntryEngine.SOUND> callback)
        {
            __instance.PlayMusic(name, source, callback);
        }
        public static void PlayMusic(string name, float volume, float channel, System.Action<EntryEngine.SOUND> callback)
        {
            __instance.PlayMusic(name, volume, channel, callback);
        }
        public static void PlayMusic(EntryEngine.SOUND sound, EntryEngine.IAudioSource source)
        {
            __instance.PlayMusic(sound, source);
        }
        public static void PlayMusic(EntryEngine.SOUND sound, float volume, float channel)
        {
            __instance.PlayMusic(sound, volume, channel);
        }
        public static void ChangeMusic(string name, System.Action<EntryEngine.SOUND> callback)
        {
            __instance.ChangeMusic(name, callback);
        }
        public static void ChangeMusic(string name, EntryEngine.IAudioSource source, System.Action<EntryEngine.SOUND> callback)
        {
            __instance.ChangeMusic(name, source, callback);
        }
        public static void ChangeMusic(string name, float volume, float channel, System.Action<EntryEngine.SOUND> callback)
        {
            __instance.ChangeMusic(name, volume, channel, callback);
        }
        public static void ChangeMusic(EntryEngine.SOUND sound, EntryEngine.IAudioSource source)
        {
            __instance.ChangeMusic(sound, source);
        }
        public static void ChangeMusic(EntryEngine.SOUND sound, float volume, float channel)
        {
            __instance.ChangeMusic(sound, volume, channel);
        }
        public static void PlayVoice(object obj, string name, System.Action<EntryEngine.SOUND> callback)
        {
            __instance.PlayVoice(obj, name, callback);
        }
        public static void PlayVoice(object obj, string name, EntryEngine.IAudioSource source, System.Action<EntryEngine.SOUND> callback)
        {
            __instance.PlayVoice(obj, name, source, callback);
        }
        public static void PlayVoice(object obj, string name, float volume, float channel, System.Action<EntryEngine.SOUND> callback)
        {
            __instance.PlayVoice(obj, name, volume, channel, callback);
        }
        public static void PlayVoice(object obj, EntryEngine.SOUND sound, EntryEngine.IAudioSource source)
        {
            __instance.PlayVoice(obj, sound, source);
        }
        public static void PlayVoice(object obj, EntryEngine.SOUND sound, float volume, float channel)
        {
            __instance.PlayVoice(obj, sound, volume, channel);
        }
        public static void ClearVoiceStack()
        {
            __instance.ClearVoiceStack();
        }
        public static void PlaySound(string name, System.Action<EntryEngine.SOUND> callback)
        {
            __instance.PlaySound(name, callback);
        }
        public static void PlaySound(string name, EntryEngine.IAudioSource source, System.Action<EntryEngine.SOUND> callback)
        {
            __instance.PlaySound(name, source, callback);
        }
        public static void PlaySound(string name, float volume, float channel, System.Action<EntryEngine.SOUND> callback)
        {
            __instance.PlaySound(name, volume, channel, callback);
        }
        public static void PlaySound(EntryEngine.SOUND sound, EntryEngine.IAudioSource source)
        {
            __instance.PlaySound(sound, source);
        }
        public static void PlaySound(EntryEngine.SOUND sound, float volume, float channel)
        {
            __instance.PlaySound(sound, volume, channel);
        }
        public static void Dispose()
        {
            __instance.Dispose();
        }
    }
}

#endif
