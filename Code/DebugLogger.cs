using System;
using System.IO;
using UnityEngine;
using System.Text;
using NCMS.Utils;

namespace ModernBox
{

	public class DebugLogger : MonoBehaviour
	{
		private static DebugLogger instance;
		private string logFilePath;
		private bool isLoggingComplete = false;

		public static bool isLogEnabled;
		
		public void Initialize()
		{
			if (isLogEnabled)
			{
				Awake();
			}
		}
		
			public static void toggleLog()
			{
				Main.modifyBoolOption("Debug_Log", PowerButtons.GetToggleValue("Log_toggle"));
				if (PowerButtons.GetToggleValue("Log_toggle"))
				{
					turnOnLog();
					return;
				}
				turnOffLog();
			}
			

				public static void turnOnLog()
				{
						isLogEnabled = true;

					

				}
				public static void turnOffLog()
				{
						isLogEnabled = false;
					


				}
	
		private void Awake()
		{
			
			if (instance != null)
			{
				Destroy(gameObject);
				return;
			}


			
			instance = this;
			DontDestroyOnLoad(gameObject);

				InitializeLogger();


				Application.logMessageReceived += LogUnityMessage;

		}

		private void OnDestroy()
		{

			Application.logMessageReceived -= LogUnityMessage;
		}

		private void InitializeLogger()
		{
			string logDirectory = Application.dataPath + "/../ModernBox_Logs";
			Directory.CreateDirectory(logDirectory);

			string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
			logFilePath = Path.Combine(logDirectory, "ModernBox_log_" + timestamp + ".txt");

			StringBuilder header = new StringBuilder();
			header.AppendLine("//===============================");
			header.AppendLine("// ModernBox 10.0.0");
			header.AppendLine("// MADE BY TUXXEGO");
			header.AppendLine("//===============================");
			
			// Display the log file path
			header.AppendLine("Path of this file: " + logFilePath);
			
			header.AppendLine("PC Specs:");
			header.AppendLine("OS: " + SystemInfo.operatingSystem);
			header.AppendLine("CPU: " + SystemInfo.processorType);
			header.AppendLine("GPU: " + SystemInfo.graphicsDeviceName);
			header.AppendLine("RAM: " + SystemInfo.systemMemorySize + " MB");
			header.AppendLine("Unity Version: " + Application.unityVersion);
			header.AppendLine("//===============================");

			File.WriteAllText(logFilePath, header.ToString());

			Log("ModernBox: Logging has started.");
		}


		public static void Log(string message)
		{

			Debug.Log(message);


			instance.LogToFile(message);
		}

		private void LogToFile(string message)
		{

			string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

			using (StreamWriter sw = File.AppendText(logFilePath))
			{
				sw.WriteLine(timestamp + " - " + message);
			}
		}

		private void LogUnityMessage(string condition, string stackTrace, LogType type)
		{


				Log(type.ToString() + ": " + condition + "\n" + stackTrace);


				if (type == LogType.Log) 
				{
					ReportSceneInfo();
				}


				if (type == LogType.Error || type == LogType.Exception)
				{

					LogToFile("ERROR: " + condition + "\n" + stackTrace);
				}
		   
		}

		private void ReportSceneInfo()
		{

			// GameObject[] allGameObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();


			// foreach (GameObject go in allGameObjects)
			// {
				// Log($"Object Name: {go.name}, Position: {go.transform.position}");
			// }
		}


		public static void QuitApplication()
		{
			if (instance != null)
			{

				//instance.isLoggingComplete = true;
			}


			Application.Quit();
		}


		private void OnApplicationQuit()
		{

			if (!isLoggingComplete)
			{
				Log("Application exited unexpectedly.");
			}
		}
	}
}