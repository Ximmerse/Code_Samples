using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public enum PlatformType
	{
		iOS = 0,
		Android
	}

	[System.Serializable]
	public class PlatformConfig
	{
		#region Properties

		public PlatformType Type;
		public string Id;
		public string Key;
		public string Sender;
		public string Title;
		public string Uri;

		#endregion

		#region Class Methods

		public override string ToString ()
		{
			return "[PlatformConfig] { Type: " + Type + ", Id: " + Id + ", Key: " + Key + ", Sender: " + Sender + ", Title: " + Title + ", Uri: " + Uri + " }";
		}

		#endregion
	}
}