using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.Networking;

namespace Disney.ForceVision
{
	public class Response
	{
		public const int HttpStatusOK = 200;
		public const int HttpStatusNotFound = 404;
		public const int HttpStatusUnauthorized = 401;

		/** Public Getter for the request associated with this response. */
		public Request Request;

		/** Public Getter for the HTTP status code. */
		public long HttpStatusCode;

		/** Public Getter for the downloaded data as a string. */
		public string Text
		{
			get
			{
				return Request.WWW.downloadHandler.text;
			}
		}

		/** Public Getter for the downloaded data as a byte array. */
		public byte[] Bytes
		{
			get
			{
				return Request.WWW.downloadHandler.data;
			}
		}

		/** Public Getter for the request ID. */
		public string ID;

		/** Public Getter for the request URL. */
		public string Url;

		/** Public Getter for the response headers. */
		public Dictionary<string, string> Headers;

		/// <summary>
		/// Initializes a new instance of the <see cref="com.disney.forcevision.CPResponse"/> class.
		/// </summary>
		/// <param name="request">Request.</param>
		/// <param name="url">URL.</param>
		/// <param name="id">Identifier.</param>
		/// <param name="headers">Headers.</param>
		/// <param name="httpStatusCode">Http status code.</param>
		public Response(Request request, string url, string id, Dictionary<string, string> headers, long httpStatusCode)
		{
			this.HttpStatusCode = httpStatusCode;
			this.ID = id;
			this.Url = url;
			this.Headers = headers;
			this.Request = request;
		}
	}
}