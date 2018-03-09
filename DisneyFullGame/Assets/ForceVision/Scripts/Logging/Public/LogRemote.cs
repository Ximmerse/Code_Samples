using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Disney.ForceVision
{
	public class LogRemote
	{
		/// <summary>
		/// The connection port.
		/// </summary>
		private int connectionPort = 9000;

		/// <summary>
		/// The messageQueue.  Stores messages logged before an external connection has been made.
		/// </summary>
		private List<string> messageQueue;

		/// <summary>
		/// The socket created as a result of the remote connection being established.
		/// </summary>
		private Socket receiver;

		// If this instance has been desposed
		private bool disposed = false;


		private LogRemoteSocket logRemoteSocket;

		/// <summary>
		/// Instance this instance.
		/// </summary>
		public LogRemote(int port)
		{
			connectionPort = port;

			Log.OnForceVisionLogEvent += LogReceiver;

			logRemoteSocket = new LogRemoteSocket(OnAcceptConnection, connectionPort);

			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="Disney.ForceVision.LogRemote"/> is reclaimed by garbage collection.
		/// </summary>
		~LogRemote()
		{
			Dispose(false);
		}

		/// <summary>
		/// Raises the accept connection event.
		/// </summary>
		/// <param name="sock">Sock.</param>
		private void OnAcceptConnection(Socket sock)
		{
			receiver = sock;

			if (messageQueue != null && this.messageQueue.Count > 0)
			{
				for (int i = 0; i < this.messageQueue.Count; i++)
				{
					receiver.Send(System.Text.Encoding.UTF8.GetBytes(messageQueue[i]));
				}
				messageQueue.Clear();
			}
		}

		/// <summary>
		/// Background threaded function for waiting for a connection.  Sends any queued messages once a connection is accepted.
		/// </summary>
		/// <param name="socketObj">The socket used to accept the remote connection.</param>
		private void AcceptThread(object socketObj)
		{
			Socket sock = socketObj as Socket;

			try
			{
				receiver = sock.Accept();
				sock.Disconnect(false);
			}
			catch (SocketException)
			{
				// We never got a listener, just return. This will happen if the socket is killed before we get one.
				return;
			}

			if (messageQueue != null && this.messageQueue.Count > 0)
			{
				for (int i = 0; i < this.messageQueue.Count; i++)
				{
					receiver.Send(System.Text.Encoding.UTF8.GetBytes(messageQueue[i]));
				}
				messageQueue.Clear();
			}
		}

		/// <summary>
		/// Releases all resource used by the <see cref="Disney.ForceVision.LogRemote"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="Disney.ForceVision.LogRemote"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="Disney.ForceVision.LogRemote"/> in an unusable state. After
		/// calling <see cref="Dispose"/>, you must release all references to the <see cref="Disney.ForceVision.LogRemote"/>
		/// so the garbage collector can reclaim the memory that the <see cref="Disney.ForceVision.LogRemote"/> was occupying.</remarks>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this); 
		}

		/// <summary>
		/// Clean up the socket(s).
		/// </summary>
		/// <param name="disposing">If set to <c>true</c> disposing.</param>
		protected virtual void Dispose(bool disposing)
		{
			// Already been here once.
			if (disposed)
			{
				return;
			}

			// This is only when manually calling dispose(), used to cleanup managed objects (things the GC knows about)
			if (disposing) 
			{

			}

			// Cleanup any unmanaged objects (things like events, file handles, connections) here
			if (receiver != null)
			{
				receiver = null;
			}

			if (logRemoteSocket != null)
			{
				logRemoteSocket.Dispose();
				logRemoteSocket = null;
			}

			// set the flag
			disposed = true;
		}

		/// <summary>
		/// Receives the messages dispatched by Log.
		/// </summary>
		/// <param name="sender">The sender: Log.</param>
		/// <param name="args">The event arguments.</param>
		private void LogReceiver(object sender, ForceVisionLogEventArgs args)
		{
			string message = "[" + args.Channel + "] ";

			switch (args.LogType)
			{
 				case LogType.Assert:
					message += "[Assert] " + args.Message + "\n\tcontext: " + args.Context;
					break;
				case LogType.Error:
					message += "[Error] " + args.Message + "\n\tcontext: " + args.Context;
					break;
				case LogType.Exception:
					message += "[Exception] "+ args.Exception.Message + "\n\texception:" + args.Exception + "\n\tstack:" + args.Exception.StackTrace + "\n\tcontext: " + args.Context;
					break;
				case LogType.Log:
					message += "[Log] " + args.Message + "\n\tcontext: " + args.Context;
					break;
				case LogType.Warning:
					message += "[Warning] " + args.Message + "\n\tcontext: " + args.Context;
					break;
				default:
					break;
			}

			SendMessage(message);
		}

		/// <summary>
		/// Sends the message.
		/// </summary>
		/// <param name="str">The message to be sent through the socket connection.</param>
		private void SendMessage(string str)
		{
			string message = str + "\n";

			if (receiver != null)
			{
				try
				{
					receiver.Send(System.Text.Encoding.UTF8.GetBytes(message));
				}
				catch (SocketException)
				{
					QueueMessage(message);
					receiver.Disconnect(false);
					receiver = null;
					logRemoteSocket.Dispose();
					logRemoteSocket = new LogRemoteSocket(OnAcceptConnection, connectionPort);
				}
			}
			else
			{
				QueueMessage(message);
			}
		}

		/// <summary>
		/// Queues the message.
		/// </summary>
		/// <param name="msg">Message.</param>
		private void QueueMessage(string msg)
		{
			if (messageQueue == null)
			{
				messageQueue = new List<string>();
			}
			messageQueue.Add(msg);
		}

		private static void OnUnhandledException (object sender, UnhandledExceptionEventArgs args)
		{
			if (args == null || args.ExceptionObject == null) 
			{
				return;
			}
			try 
			{
				Exception exception = args.ExceptionObject as Exception;
				if (exception != null) 
				{
					Log.Error(exception.Message + "\n" + exception.StackTrace);
					// don't need to log excpetion because Crittercism will catch and send unhandleds
				}
			} 
			catch 
			{
				if (Debug.isDebugBuild == true) 
				{
					Log.Error ("LogRemote: Failed to log exception");
				}
			}
		}
	}
}
