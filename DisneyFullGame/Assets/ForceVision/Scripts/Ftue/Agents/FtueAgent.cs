using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public class FtueAgent : MonoBehaviour 
	{
		#region Properties

		public Animator AgentAnimator;
		public Animator FbxAnimator;
		public FtueAgentType Agent;

		protected string currentClip;

		#endregion

		#region Class Methods

		public void PlayAnimation(string clip)
		{
			currentClip = clip;

			if (AgentAnimator != null) 
			{
				AgentAnimator.Play(clip);
			}
		}

		public void PlaySubAnimation(string clip)
		{
			if (FbxAnimator != null) 
			{
				FbxAnimator.Play(clip);
			}
		}

		public void PlaySound(string sound)
		{
			// Play WWise sound
			AudioEvent.Play(sound, gameObject);
		}

		public void TriggerSubAnimation(string triggerName)
		{
			if (FbxAnimator != null) 
			{
				FbxAnimator.SetTrigger(triggerName);
			}
		}

		protected void AnimationComplete(int animationType)
		{
			if (FtueAgentAnimationEvent.OnFtueAgentAnimationComplete != null)
			{
				FtueAgentAnimationEvent.OnFtueAgentAnimationComplete(this, new FtueAgentAnimationEvent (Agent, animationType, currentClip));
			}
		}

		private string GetAnimationClipNameFromAgentAndType(int animationType)
		{
			string clipName = "";

			switch(Agent)
			{
			case FtueAgentType.Archivist:
				clipName = ((ArchivistAnimations)animationType).ToString();
				break;
			case FtueAgentType.Droid:
				clipName = ((DroidAnimations)animationType).ToString();
				break;
			case FtueAgentType.Holocron:
				clipName = ((HolocronAnimations)animationType).ToString();
				break;
			case FtueAgentType.Yoda:
				clipName = ((YodaAnimations)animationType).ToString();
				break;
			}

			return clipName;
		}

		#endregion
	}
}