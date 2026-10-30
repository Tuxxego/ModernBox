using System;
using System.IO;
using System.Collections.Generic;
using FMOD;
using FMODUnity;
using NeoModLoader.services;
using UnityEngine;

namespace ModernBox
{
    public class PlayWavDirectly
    {
        public static PlayWavDirectly _instance;
        public static PlayWavDirectly Instance => _instance ??= new PlayWavDirectly();

        private FMOD.System fmodSystem;
        private ChannelGroup masterChannelGroup;
        private readonly Dictionary<string, FMOD.Sound> _soundCache = new Dictionary<string, FMOD.Sound>(StringComparer.OrdinalIgnoreCase);

        public FMOD.VECTOR fmodPosition;
        public FMOD.VECTOR zeroVel;

        public PlayWavDirectly()
        {
            InitializeFMODSystem();
        }

        private void InitializeFMODSystem()
        {
            var result = RuntimeManager.StudioSystem.getCoreSystem(out fmodSystem);
            if (result != FMOD.RESULT.OK)
            {
                LogService.LogError($"Failed to initialize FMOD Core System. Result: {result}");
                return;
            }

            result = fmodSystem.getMasterChannelGroup(out masterChannelGroup);
            if (result != FMOD.RESULT.OK)
            {
                LogService.LogError($"Failed to retrieve master channel group. Result: {result}");
            }
        }

        private bool TryGetOrCreateSound(string filePath, out FMOD.Sound sound)
        {
            if (_soundCache.TryGetValue(filePath, out sound) && sound.hasHandle())
            {
                return true;
            }

            if (!File.Exists(filePath))
            {
                LogService.LogError($"File not found at path: {filePath}");
                sound = default;
                return false;
            }

            RESULT result = fmodSystem.createSound(filePath, MODE.DEFAULT, out sound);
            if (result != RESULT.OK)
            {
                LogService.LogError($"FMOD failed to create sound: {result}");
                return false;
            }

            _soundCache[filePath] = sound;
            return true;
        }

        public void PlaySoundFromFile(string filePath)
        {
            try
            {
                if (!TryGetOrCreateSound(filePath, out FMOD.Sound sound))
                {
                    return;
                }

                Channel channel;
                RESULT result = fmodSystem.playSound(sound, masterChannelGroup, false, out channel);
                if (result != RESULT.OK)
                {
                    LogService.LogError($"FMOD failed to play sound: {result}");
                    return;
                }
            }
            catch (Exception ex)
            {
                LogService.LogError($"Error while playing sound: {ex.Message}");
            }
        }

        public void PlaySoundAtPosition(string filePath, Vector3 position)
        {
            try
            {
                if (!TryGetOrCreateSound(filePath, out FMOD.Sound sound))
                {
                    return;
                }

                Channel channel;
                RESULT result = fmodSystem.playSound(sound, masterChannelGroup, false, out channel);
                if (result != RESULT.OK)
                {
                    LogService.LogError($"FMOD failed to play sound: {result}");
                    return;
                }

                Camera mainCam = Camera.main;
                Vector3 listenerPosition = mainCam != null ? mainCam.transform.position : Vector3.zero;
                Vector3 sourcePosition = listenerPosition;
                zeroVel = new FMOD.VECTOR { x = 0f, y = 0f, z = 0f };

                float normalizedDistance = Mathf.Clamp01(Vector3.Distance(sourcePosition, listenerPosition) / 300.0f);
                channel.setVolume(1f * (1f - normalizedDistance));
            }
            catch (Exception ex)
            {
                LogService.LogError($"Error while playing sound: {ex.Message}");
            }
        }

        public void StopAllSounds()
        {
            var result = masterChannelGroup.stop();
            if (result != RESULT.OK)
            {
                LogService.LogError($"Failed to stop all sounds. Result: {result}");
            }
        }
    }
}
