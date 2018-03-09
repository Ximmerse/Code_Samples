using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Disney.ForceVision
{

	public static class Log
	{

		#region Enums

		/// <summary>
		/// Log state.
		/// </summary>
		public enum LogState
		{
			Quiet,
			Verbose
		}

		/// <summary>
		/// Log channel.
		/// </summary>
		public enum LogChannel
		{
			General,
			Storage,
			Settings,
			VisionSdk,
			Holochess,
			Assault,
			AssaultAI,
			AssaultAudio,
			AssaultLevel,
			Duel,
			Download,
			MultiplayerSaber
		}

		#endregion

		#region Private Properties

		/// <summary>
		/// The log levels. Set LogState to Verbose to get all logs on a Channel.
		/// </summary>
		private static Dictionary<LogChannel, LogState> logLevels = new Dictionary<LogChannel, LogState> {
			{ Log.LogChannel.General, Log.LogState.Verbose },
			{ Log.LogChannel.Storage, Log.LogState.Quiet },
			{ Log.LogChannel.Settings, Log.LogState.Quiet },
			{ Log.LogChannel.VisionSdk, Log.LogState.Quiet },
			{ Log.LogChannel.Holochess, Log.LogState.Quiet },
			{ Log.LogChannel.Assault, Log.LogState.Quiet },
			{ Log.LogChannel.AssaultAI, Log.LogState.Quiet },
			{ Log.LogChannel.AssaultAudio, Log.LogState.Quiet },
			{ Log.LogChannel.AssaultLevel, Log.LogState.Quiet },
			{ Log.LogChannel.Duel, Log.LogState.Quiet },
			{ Log.LogChannel.Download, Log.LogState.Quiet },
			{ Log.LogChannel.MultiplayerSaber, Log.LogState.Verbose },
		};

		/// <summary>
		/// Stores logs made to targetUI.
		/// </summary>
		private static string uiLog;

		/// <summary>
		/// Writes logs to a Unity Text field.
		/// </summary>
		private static Text targetUI;

		#endregion

		#region Public Properties

		/// <summary>
		/// Number of chars to keep in uiLog when writing to targetUI.
		/// </summary>
		public static int MaxUILogLength = 10000;

		/// <summary>
		/// Gets or sets the on force vision log event arguments.
		/// </summary>
		/// <value>The on force vision log event arguments.</value>
		public static EventHandler<ForceVisionLogEventArgs> OnForceVisionLogEvent { get; set; }

		#endregion

		#region Public Methods

		/// <summary>
		/// Set a log channel and state on the log system.
		/// </summary>
		/// <param name="channel">Channel.</param>
		/// <param name="logState">Logstate.</param>
		[System.Diagnostics.Conditional("ENABLE_LOGS")]
		public static void SetLogChannelAndState(LogChannel channel = LogChannel.General, LogState logState = LogState.Verbose)
		{
			if (logLevels.ContainsKey(channel) == false)
			{
				logLevels.Add(channel, logState);
			}
			else
			{
				logLevels[channel] = logState;
			}
		}

		/// <summary>
		/// Debug the specified message, channel and context.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="context">Context.</param>
		[System.Diagnostics.Conditional("ENABLE_LOGS")]
		public static void Debug(string message, LogChannel channel = LogChannel.General, UnityEngine.Object context = null)
		{
			if (logLevels[channel] == LogState.Verbose)
			{
				DoLog(message, channel, LogType.Log, null, context);
			}
		}

		/// <summary>
		/// Warning the specified message, channel and context.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="context">Context.</param>
		[System.Diagnostics.Conditional("ENABLE_LOGS")]
		public static void Warning(string message, LogChannel channel = LogChannel.General, UnityEngine.Object context = null)
		{
			DoLog(message, channel, LogType.Warning, null, context);
		}

		/// <summary>
		/// Error the specified message, channel and context.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="context">Context.</param>
		[System.Diagnostics.Conditional("ENABLE_LOGS")]
		public static void Error(string message, LogChannel channel = LogChannel.General, UnityEngine.Object context = null)
		{
			DoLog(message, channel, LogType.Error, null, context);
		}

		/// <summary>
		/// Exception the specified exception, message, channel and context.
		/// </summary>
		/// <param name="exception">Exception.</param>
		/// <param name="message">Message.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="context">Context.</param>
		public static void Exception(Exception exception, string message = "", LogChannel channel = LogChannel.General, UnityEngine.Object context = null)
		{
			DoLog(message, channel, LogType.Exception, exception, context);
			Crittercism.LogHandledException(exception);
		}

		/// <summary>
		/// Assert the specified message, channel and context.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="context">Context.</param>
		[System.Diagnostics.Conditional("ENABLE_LOGS")]
		public static void Assert(bool condition, string message, LogChannel channel = LogChannel.General, UnityEngine.Object context = null)
		{
			if (condition)
			{
				DoLog(message, channel, LogType.Assert, null, context);
			}
		}

		/// <summary>
		/// Write the log.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="logType">Log type.</param>
		/// <param name="exception">Exception.</param>
		/// <param name="context">Context.</param>
		private static void DoLog(string message, LogChannel channel, LogType logType, Exception exception = null, UnityEngine.Object context = null)
		{
			//log to ui
			if (targetUI != null)
			{
				uiLog = message + "\n" + uiLog;
				if (uiLog.Length > MaxUILogLength)
				{
					uiLog = uiLog.Substring(0, MaxUILogLength);
				}
				if (targetUI != null)
				{
					targetUI.text = uiLog;
				}
			}

			switch (logType)
			{
				case LogType.Log:
					UnityEngine.Debug.Log(message, context);
					break;
				case LogType.Warning:
					UnityEngine.Debug.LogWarning(message, context);
					break;
				case LogType.Error:
					UnityEngine.Debug.LogError(message, context);
					break;
				case LogType.Exception:
					if (string.IsNullOrEmpty(message) == false)
					{
						UnityEngine.Debug.LogError(message, context);
					}
					UnityEngine.Debug.LogException(exception, context);
					break;
			//TODO: lisum001 stop playback?
				case LogType.Assert:
					UnityEngine.Debug.LogError(message, context);
					break;
			}

			if (OnForceVisionLogEvent != null)
			{
				OnForceVisionLogEvent(null, new ForceVisionLogEventArgs(message, channel, logType, exception, context));
			}
		}

		/// <summary>
		/// Set a Unity Text field log target.
		/// </summary>
		/// <param name="textField">Text field.</param>
		public static void SetUILogTarget(Text textField)
		{
			targetUI = textField;
		}

		/// <summary>
		/// Removes the Unity Text field log target.
		/// </summary>
		public static void RemoveUILogTarget()
		{
			targetUI = null;
		}

		#endregion

	}
}
