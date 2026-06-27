using UnityEngine;

using System.Collections.Generic;
using UnityEngine.Events;

using UnityEngine.UI;
using System.Collections;
using NCMS.Utils;
using NCMS;
using ReflectionUtility;
using TuxModLoader.Reflection;
using System.Reflection;
using System;

public class UniverseDestructionManager : MonoBehaviour
{
    private bool showEndScreen = true;
    private Texture2D blackTexture;

    private void Awake()
    {
        blackTexture = new Texture2D(1, 1);
        blackTexture.SetPixel(0, 0, Color.black);
        blackTexture.Apply();
    }

    private void OnGUI()
    {
        if (showEndScreen)
        {
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), blackTexture);

            float windowWidth = 500;
            float windowHeight = 200;
            Rect windowRect = new Rect(
                (Screen.width - windowWidth) / 2,
                (Screen.height - windowHeight) / 2,
                windowWidth,
                windowHeight
            );

            GUILayout.BeginArea(windowRect, GUI.skin.box);

            GUILayout.Label("Suddenly, in the blink of an eye, everything was destroyed in every way it is possible to be destroyed, thousands of galaxies vanished in an instant. The timeline has been destroyed.");
            GUILayout.Space(20);

            if (GUILayout.Button("Quit Game", GUILayout.Height(40)))
            {
                Application.Quit();
            }

            GUILayout.EndArea();
        }
    }
}