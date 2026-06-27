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

namespace ModernBox
{
	public class NCMSChecker : MonoBehaviour
	{

		static string dllDirectoryPath = Application.streamingAssetsPath + "/mods/";
		static string dllFileName = "NCMS_memload.dll";

		void Start()
		{

		}

		public static void CheckDLL()
		{

			string dllPath = Path.Combine(dllDirectoryPath, dllFileName);

			if (File.Exists(dllPath))
			{
				Debug.Log("NCMS exists: " + dllPath);
					Windows.ShowWindow("NCMSWindow");

			}
			else
			{
				Debug.LogWarning("NCMS not found: " + dllPath);

			}
		}
	}
}