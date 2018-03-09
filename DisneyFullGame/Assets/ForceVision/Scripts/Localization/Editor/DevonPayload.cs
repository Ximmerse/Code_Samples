using System;
using System.Collections.Generic;

namespace Disney.ForceVision.Internal
{
	[Serializable]
	public class DevonPayload
	{
		/// <summary>
		/// The tokens to delete.
		/// </summary>
		public List<string> Deleted = new List<string>();

		/// <summary>
		/// The tokens to add.
		/// </summary>
		public List<DevonToken> Tokens = new List<DevonToken>();

		/// <summary>
		/// The version to update.
		/// </summary>
		public string Version;
	}
}