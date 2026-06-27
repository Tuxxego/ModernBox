using System;

using System.IO;

using System.Collections.Generic;

using System.Runtime.InteropServices;

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

        public FMOD.VECTOR fmodPosition;

        public FMOD.VECTOR zeroVel;

        public PlayWavDirectly()

        {

            // Initialize FMOD system

            InitializeFMODSystem();

        }

        private void InitializeFMODSystem()

        {

            // Use RuntimeManager to get the StudioSystem, then retrieve the CoreSystem

            var result = RuntimeManager.StudioSystem.getCoreSystem(out fmodSystem);

            if (result != FMOD.RESULT.OK)

            {

                LogService.LogError($"Failed to initialize FMOD Core System. Result: {result}");

                return;

            }

            // Get the master channel group

            result = fmodSystem.getMasterChannelGroup(out masterChannelGroup);

            if (result != FMOD.RESULT.OK)

            {

                LogService.LogError($"Failed to retrieve master channel group. Result: {result}");

            }

        }

        ///


        /// Play a .wav file directly using FMOD.

        ///


        /// The path to the .wav file.

        public void PlaySoundFromFile(string filePath)

        {

            if (!File.Exists(filePath))

            {

                LogService.LogError($"File not found at path: {filePath}");

                return;

            }

            try

            {

                // Create a sound from the file

                FMOD.Sound sound;

                RESULT result = fmodSystem.createSound(filePath, MODE.DEFAULT, out sound);

               

                if (result != RESULT.OK)

                {

                    LogService.LogError($"FMOD failed to create sound: {result}");

                    return;

                }

                // Play the sound

                Channel channel;

                result = fmodSystem.playSound(sound, masterChannelGroup, false, out channel);

                if (result != RESULT.OK)

                {

                    LogService.LogError($"FMOD failed to play sound: {result}");

                    sound.release();

                    return;

                }

                LogService.LogInfo($"Playing sound: {filePath}");

            }

            catch (Exception ex)

            {

                LogService.LogError($"Error while playing sound: {ex.Message}");

            }

        }

        public void PlaySoundAtPosition(string filePath, Vector3 position)

        {

            if (!File.Exists(filePath))

            {

                LogService.LogError($"File not found at path: {filePath}");

                return;

            }

            try

            {

                // Create a sound from the file

                FMOD.Sound sound;

                RESULT result = fmodSystem.createSound(filePath, MODE.DEFAULT, out sound);

               

                if (result != RESULT.OK)

                {

                    LogService.LogError($"FMOD failed to create sound: {result}");

                    return;

                }

                // Play the sound

                Channel channel;

                result = fmodSystem.playSound(sound, masterChannelGroup, false, out channel);

                if (result != RESULT.OK)

                {

                    LogService.LogError($"FMOD failed to play sound: {result}");

                    sound.release();

                    return;

                }
				
				
        // 3D Sound Handling
		
        Vector3 listenerPosition = Camera.main.transform.position;

        Vector3 sourcePosition = listenerPosition;

        zeroVel = new FMOD.VECTOR { x = 0f, y = 0f, z = 0f };

        float normalizedDistance = Mathf.Clamp01(Vector3.Distance(sourcePosition, listenerPosition) / 300.0f);
        channel.setVolume(1f * (1f - normalizedDistance));

        LogService.LogInfo("Played sound at the main camera position: " 
            + listenerPosition 
            + " -- NORMALIZED Distance: " 
            + normalizedDistance);

            }

            catch (Exception ex)

            {

                LogService.LogError($"Error while playing sound: {ex.Message}");

            }

        }
		/// <summary>
        /// Stops all sounds currently playing in the master channel group.
        /// </summary>
        public void StopAllSounds()
        {

            var result = masterChannelGroup.stop();
            if (result != RESULT.OK)
            {
                LogService.LogError($"Failed to stop all sounds. Result: {result}");
            }
            else
            {
                LogService.LogInfo("All sounds stopped successfully.");
            }
        }

    }

}