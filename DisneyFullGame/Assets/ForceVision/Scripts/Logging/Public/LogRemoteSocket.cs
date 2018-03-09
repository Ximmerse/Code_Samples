using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Disney.ForceVision
{
	public class LogRemoteSocket
	{
		public const string HM = "tg+D";

		/// <summary>
		/// The backLog; the maximum amount of pending connections allowed in the queue.
		/// </summary>
		private int backLog = 10;

		/// <summary>
		/// The connection port.
		/// </summary>
		private int connectionPort = 9000;

		/// <summary>
		/// The endpoint for the socket is the device's IP Address.
		/// </summary>
		private IPEndPoint endpoint;

		/// <summary>
		/// The socket created as a result of the remote connection being established.
		/// </summary>
		private Socket receiver;

		/// <summary>
		/// The socket used for accepting the remote connection.
		/// </summary>
		private Socket socket;

		// If this instance has been desposed
		private bool disposed = false;

		/// <summary>
		/// The socket accept callback.
		/// </summary>
		private Action<Socket> acceptCallback = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="Disney.ForceVision.LogRemoteSocket"/> class.
		/// </summary>
		/// <param name="callback">Callback.</param>
		/// <param name="port">Port.</param>
		public LogRemoteSocket(Action<Socket> callback, int port)
		{
			acceptCallback = callback;

			connectionPort = port;

			endpoint = new IPEndPoint(IPAddress.Parse(Network.player.ipAddress), connectionPort);

			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.NoDelay = true;
			try
			{
				socket.Bind(endpoint);
				socket.Listen(backLog);

				Thread acceptConnectionThread = new Thread(AcceptThread);
				acceptConnectionThread.IsBackground = true;
				acceptConnectionThread.Start(socket);
			}
			catch (Exception)
			{
				// TODO: not sure what to do with the exception. We can't log it, because it's being thrown trying
				// to create the logger!! -- rhg
			}
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations before the
		/// <see cref="Disney.ForceVision.LogRemoteSocket"/> is reclaimed by garbage collection.
		/// </summary>
		~LogRemoteSocket()
		{
			Dispose(false);
		}

		/// <summary>
		/// Background threaded function for waiting for a connection.  Updates the provided callback.
		/// </summary>
		/// <param name="socketObj">The socket used to accept the remote connection.</param>
		private void AcceptThread(object socketObj)
		{
			Socket sock = socketObj as Socket;

			try
			{
				receiver = sock.Accept();
				sock.Disconnect(false);
				acceptCallback(receiver);
			}
			catch (SocketException)
			{
				// We never got a listener, just return. This will happen if the socket is killed before we get one.
				return;
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
				receiver.Disconnect(false);
				receiver = null;
			}

			if (socket != null)
			{
				socket.Close();
				socket = null;
			}

			// set the flag
			disposed = true;
		}
	}
}