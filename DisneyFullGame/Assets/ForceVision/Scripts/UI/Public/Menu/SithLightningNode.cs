using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public class SithLightningNode : MonoBehaviour 
	{

		public List<SithLightningNode> Neighbors = new List<SithLightningNode>();

		public SithLightningNode GetRandomNeighbor()
		{
			return Neighbors.Random();
		}
	}
}
