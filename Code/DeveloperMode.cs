//========= MODERNBOX 10.0 ============//
//
// Made by Tuxxego
//
//=============================================================================//
using System;
using NCMS.Utils;
using NCMS;
using System.IO;
using UnityEngine;
using ReflectionUtility;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Headers;

namespace ModernBox {
public class DeveloperMode : MonoBehaviour {
  public static bool isDeveloperEnabled;
  private static HashSet<string> loggedErrors = new HashSet<string>();
  private static readonly string discordWebhookUrl = "https://discord.com/api/webhooks/1258246545019764777/lQygzMXBKdRCc-jVEpElZWFzIVi4WuOy7--dsx9xNQOMrsTsFPKKNL4CpfDc98ypWpYc";

  public static void toggleDevMode() {
    Main.modifyBoolOption("Developer_Mode", PowerButtons.GetToggleValue("Devmode"));
    if (PowerButtons.GetToggleValue("Devmode")) {
      turnOnDevMode();
      return;
    }
    turnOffDevMode();
  }

  public static void turnOnDevMode() {
    Windows.ShowWindow("DeveloperWindow");
    isDeveloperEnabled = true;
    Application.logMessageReceived += HandleLog;
  }

  public static void turnOffDevMode() {
    isDeveloperEnabled = false;
    Application.logMessageReceived -= HandleLog;
  }

  private static void HandleLog(string logString, string stackTrace, LogType type) {
    if (type == LogType.Error || type == LogType.Exception) {
      Debug.Log($"Error detected: {logString}");

      if (loggedErrors.Contains(logString)) {
        Debug.Log("Duplicate error detected, not sending to Discord.");

        return;
      }

      loggedErrors.Add(logString);
      SendErrorToDiscord(logString, stackTrace);
    }
  }

  private static void SendErrorToDiscord(string logString, string stackTrace) {
    if (!isDeveloperEnabled) {
      return;
    }

    var payload = new {content = $"**Error:**\n```{logString}```\n**Stack Trace:**\n```{stackTrace}```"};

    string jsonPayload = JsonConvert.SerializeObject(payload);

    using(var webClient = new WebClient()) {
      webClient.Headers[HttpRequestHeader.ContentType] = "application/json";
      webClient.UploadString(discordWebhookUrl, jsonPayload);
    }
  }

        public void Initialize() {
            // Implement your initialization logic here
        }
}
}
