using UnityEngine;
using System.Collections;
using SG.Lonestar;

namespace Disney.ForceVision
{
	public class CorePlanetController : MonoBehaviour
	{
		public Animator CoreAnimator;

		public IEnumerator Start()
		{
			// Unity... Animator wants you to wait for some time.
			// 4 Seconds seems to be the sweet spot for long editor times, 
			// and is quick enough for faster load times, as it doesn't need precision
			yield return new WaitForSeconds(4f);

			/*
			    - First: Starts on
			    - Second: haven’t beat maul on med.
			    - Third: planet garel on med
			    - Four: planet Loth on med
			    - Fifth: planet Hoth on med
			    - Six: beat takadona on med
			*/

			DuelAPI api = ContainerAPI.GetDuelApi();

			if (api.Progress.HasCompleted(DuelAPI.Duelist.KyloRen, 2))
			{
				CoreAnimator.SetTrigger("TriggerLevel6");
			}
			else if (api.Progress.HasCompleted(DuelAPI.Duelist.DarthVader, 2))
			{
				CoreAnimator.SetTrigger("TriggerLevel5");
			}
			else if (api.Progress.HasCompleted(DuelAPI.Duelist.GrandInquisitor, 2))
			{
				CoreAnimator.SetTrigger("TriggerLevel4");
			}
			else if (api.Progress.HasCompleted(DuelAPI.Duelist.SeventhSister, 2))
			{
				CoreAnimator.SetTrigger("TriggerLevel3");
			}
			else if (api.Progress.HasCompleted(DuelAPI.Duelist.DarthMaul, 2))
			{
				CoreAnimator.SetTrigger("TriggerLevel2");
			}
			else
			{
				CoreAnimator.SetTrigger("TriggerLevel1");
			}
		}
	}
}