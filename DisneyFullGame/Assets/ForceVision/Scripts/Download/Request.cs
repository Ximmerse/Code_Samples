using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.Networking;

namespace Disney.ForceVision
{
	public class Request
	{
		//		public const int HttpStatusOK = 200;
		//		public const int HttpStatusNotFound = 404;
		//		public const int HttpStatusUnauthorized = 401;

		public int Attempts;
		public UnityWebRequest WWW = null;

		private MonoBehaviour runner;
		private Action<Response> callback;
		private string ID;
		private string url;
		private Dictionary<string, string> headers;
		private Dictionary<string, string> fields;
		private RequestMethod requestMethod;
		private Action<float> fileProgressCallback;

		private enum RequestMethod
		{
			GET,
			POST
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="com.disney.forcevision.CPRequest"/> class.
		/// </summary>
		/// <param name="runner">Runner.</param>
		/// <param name="callback">Callback.</param>
		/// <param name="id">Identifier.</param>
		/// <param name="url">URL.</param>
		/// <param name="headers">Headers.</param>
		/// <param name="fields">Fields.</param>
		public Request(MonoBehaviour runner, Action<Response> callback, string id, string url, Dictionary<string, string> headers = null, Dictionary<string, string> fields = null, Action<float> fileProgressCallback = null)
		{
			this.runner = runner;
			this.callback = callback;
			this.ID = id;
			this.url = url;
			this.headers = headers;
			this.fields = fields;
			this.fileProgressCallback = fileProgressCallback;
		}

		public void Retry()
		{
			switch (requestMethod)
			{
				case RequestMethod.GET:
					GetData();
					break;
				case RequestMethod.POST:
					PostData();
					break;
			}
		}

		/// <summary>
		/// Gets the data.
		/// </summary>
		public void GetData()
		{
			requestMethod = RequestMethod.GET;
			Attempts++;
			runner.StartCoroutine(Get());
		}

		/// <summary>
		/// Posts the data.
		/// </summary>
		public void PostData()
		{
			requestMethod = RequestMethod.POST;
			Attempts++;
			runner.StartCoroutine(Post());
		}

		/// <summary>
		/// Post this instance.
		/// </summary>
		private IEnumerator Post()
		{
//			UnityWebRequest www = null;
			WWWForm form = null;

			if (fields != null)
			{
				form = new WWWForm();
				foreach (KeyValuePair<string, string> field in fields)
				{
					form.AddField(field.Key, field.Value);
				}
			}

			WWW = UnityWebRequest.Post(url, form);

			if (headers != null)
			{
				foreach (KeyValuePair<string, string> header in headers)
				{
					WWW.SetRequestHeader(header.Key, header.Value);
				}
			}

			WWW.Send();

			while (!WWW.isDone)
			{
				UpdateProgress(WWW.downloadProgress);
				yield return null;
			}

			UpdateProgress(WWW.downloadProgress);

			Response res = new Response(this, url, ID, WWW.GetResponseHeaders(), WWW.responseCode);

			callback(res);
		}

		/// <summary>
		/// Get this instance.
		/// </summary>
		private IEnumerator Get()
		{
//			UnityWebRequest www = null;
			WWW = UnityWebRequest.Get(url);

			if (headers != null)
			{
				foreach (KeyValuePair<string, string> header in headers)
				{
					WWW.SetRequestHeader(header.Key, header.Value);
				}
			}

			WWW.Send();

			while (!WWW.isDone)
			{
				UpdateProgress(WWW.downloadProgress);
				yield return null;
			}

			UpdateProgress(WWW.downloadProgress);

			Response res = new Response(this, url, ID, WWW.GetResponseHeaders(), WWW.responseCode);

			callback(res);
		}

		private void UpdateProgress(float progress)
		{
			if (fileProgressCallback != null)
			{
				fileProgressCallback(progress);
			}
		}
	}
}