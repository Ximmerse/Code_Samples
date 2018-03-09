using System;
using UnityEngine;

namespace Disney.ForceVision
{
	public class ForceVisionLogEventArgs : EventArgs
	{
		#region Public Properties

		/// <summary>
		/// The message.
		/// </summary>
		public string Message;

		/// <summary>
		/// The channel.
		/// </summary>
		public Log.LogChannel Channel;

		/// <summary>
		/// The type of the log.
		/// </summary>
		public LogType LogType;

		/// <summary>
		/// The exception.
		/// </summary>
		public Exception Exception;

		/// <summary>
		/// The context.
		/// </summary>
		public UnityEngine.Object Context;

		#endregion

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="Disney.ForceVision.ForceVisionEventLogs"/> class.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="channel">Channel.</param>
		/// <param name="logType">Log type.</param>
		/// <param name="exception">Exception.</param>
		/// <param name="context">Context.</param>
		public ForceVisionLogEventArgs(string message, Log.LogChannel channel, LogType logType, Exception exception = null, UnityEngine.Object context = null)
		{
			this.Message = message;
			this.Channel = channel;
			this.LogType = logType;
			this.Exception = exception;
			this.Context = context;
		}

		#endregion
	}
}