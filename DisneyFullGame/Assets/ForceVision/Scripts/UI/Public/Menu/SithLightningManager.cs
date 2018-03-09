using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public class SithLightningManager : MonoBehaviour 
	{
		public float SwapFrequency = 1f;
		public float SwapDivergence = 1f;
		public List<SithLightning> LightningObjects = new List<SithLightning>();
		public List<SithLightningNode> LeftSideLightningNodes = new List<SithLightningNode>();
		public List<SithLightningNode> RightSideLightningNodes = new List<SithLightningNode>();

		private void Awake()
		{
			for (int i = 0; i < LightningObjects.Count; i++)
			{
				SithLightning lightning = LightningObjects[i];

				lightning.gameObject.SetActive(true);

				ChooseRandomPair(lightning, true);

				StartCoroutine(SwapPairs (lightning));
			}
		}

		private IEnumerator SwapPairs(SithLightning lightning)
		{
			yield return new WaitForSeconds(SwapFrequency + Random.Range(0f, SwapDivergence));
			lightning.Start = LeftSideLightningNodes.Random();
			yield return new WaitForSeconds(SwapFrequency + Random.Range(0f, SwapDivergence));
			lightning.End = RightSideLightningNodes.Random();
			StartCoroutine(SwapPairs(lightning));
		}

		private void ChooseRandomPair(SithLightning lightning, bool initialPair = false)
		{

			if (initialPair)
			{
				lightning.Start = LeftSideLightningNodes.Random();
				lightning.End = RightSideLightningNodes.Random();
			}
			else
			{
				lightning.Start = lightning.Start.GetRandomNeighbor();
				lightning.End = lightning.End.GetRandomNeighbor();
			}

		}
	}
}
