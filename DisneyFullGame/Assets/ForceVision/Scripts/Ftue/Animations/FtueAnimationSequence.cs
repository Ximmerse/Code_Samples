using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Disney.ForceVision
{
	public enum FtueAnimationState
	{
		NotStarted,
		Playing,
		Waiting
	}

	[System.Serializable]
	public class FtueAnimationSequence
	{
		#region Events

		public static Action<FtueType> OnFtueAnimationSequenceComplete;
		public static Action<FtueAgentAnimationEvent> OnFtueAnimationWaiting;
		public static Action<FtueAgentAnimationEvent> OnFtueAnimationClipComplete;

		#endregion

		#region Properties

		public string Version;
		public List<FtueAnimationStep> Intro3D;
		public List<FtueAnimationStep> GalaxyMap;
		public List<FtueAnimationStep> DarkSide;
		public bool IsErrorState
		{
			get
			{
				return isErrorState;
			}

			set
			{
				isErrorState = value;

				if (!isErrorState && animationState == FtueAnimationState.Waiting)
				{
					Play();
				}
			}
		}


		protected List<FtueAnimationStep> CurrentSequence
		{
			get
			{
				List<FtueAnimationStep> sequence = null;

				if (ftueType == FtueType.Intro)
				{
					sequence = Intro3D;
				}
				else if (ftueType == FtueType.GalaxyMap)
				{
					sequence = GalaxyMap;
				}
				else if (ftueType == FtueType.DarkSide)
				{
					sequence = DarkSide;
				}

				return sequence;
			}
		}

		// starting at step 1 (and skipping step 0) since holocron enter animation happens automatically
		private int currentStepNumber = 0;
		private FtueAgent[] agents;
		private GameObject ftueAudioSource;
		private FtueType ftueType;
		private FtueAnimationState animationState = FtueAnimationState.NotStarted;
		private bool isErrorState;

		#endregion

		#region Class Methods

		public void Init(FtueAgent[] allAgents, GameObject audioSource, FtueType type = FtueType.Intro)
		{
			// saving reference to FtueAgent instances
			agents = allAgents;

			// saving reference to GameObject instance that will be the audio source
			ftueAudioSource = audioSource;

			// starting with intro ftue
			ftueType = type;

			// signing up for animation completed events
			FtueAgentAnimationEvent.OnFtueAgentAnimationComplete += OnFtueAnimationCompleteHandler;
		}

		public void Init(GameObject audioSource, FtueType type)
		{
			// saving reference to GameObject instance that will be the audio source
			ftueAudioSource = audioSource;

			// starting with intro ftue
			ftueType = type;

			// signing up for animation completed events
			FtueAgentAnimationEvent.OnFtueAgentAnimationComplete += OnFtueAnimationCompleteHandler;

			Debug.Log ("DarkSide: " + DarkSide);
			if (DarkSide != null)
			{
				Debug.Log ("DarkSide.Count: " + DarkSide.Count);
			}
		}

		public void Destroy()
		{
			// removing listener for animation completed events
			FtueAgentAnimationEvent.OnFtueAgentAnimationComplete -= OnFtueAnimationCompleteHandler;
		}

		public void Play()
		{
			animationState = FtueAnimationState.Playing;

			FtueAnimationStep currentStep = CurrentSequence[currentStepNumber];

			Log.Debug(string.Format("Playing animation #{0} - {1} for {2}.", currentStepNumber, currentStep.Clip, currentStep.Agent));

			PlayClip(currentStep);
		}

		public void Play(FtueType state, string clipName)
		{
			// saving ftue state
			ftueType = state;

			// getting animation clip data
			FtueAnimationStep stepToPlay = CurrentSequence.Where(step => string.Equals(step.Clip, clipName)).First();

			// getting step number
			currentStepNumber = CurrentSequence.IndexOf(stepToPlay);

			// playing animation
			PlayClip(stepToPlay);
		}

		private void PlayClip(FtueAnimationStep stepToPlay)
		{
			DOVirtual.DelayedCall(stepToPlay.Delay, () => {
				// playing the animation clip
				FtueAgent currentAgent = agents[(int)stepToPlay.Agent];
				currentAgent.PlayAnimation(stepToPlay.Clip);

				// playing the audio clip
				if (!string.IsNullOrEmpty(stepToPlay.SoundEvent))
				{
					AudioEvent.Play(stepToPlay.SoundEvent, ftueAudioSource);
				}
			});
		}

		#endregion

		#region Event Handlers

		protected void OnFtueAnimationCompleteHandler(object sender, FtueAgentAnimationEvent eventArgs)
		{
			// setting state
			animationState = FtueAnimationState.Waiting;

			// getting the completed step
			FtueAnimationStep completedStep = CurrentSequence[currentStepNumber];

			// incrementing the event number
			currentStepNumber++;

			// checking if there are next steps
			if (currentStepNumber < CurrentSequence.Count)
			{
				if (completedStep.AutoPlay)
				{
					if (OnFtueAnimationClipComplete != null)
					{
						OnFtueAnimationClipComplete(new FtueAgentAnimationEvent(completedStep.Agent, -1, completedStep.Clip));
					}

					if (!IsErrorState)
					{
						Play();
					}
				}
				else
				{
					if (OnFtueAnimationWaiting != null)
					{
						OnFtueAnimationWaiting(new FtueAgentAnimationEvent(completedStep.Agent, -1, completedStep.Clip));
					}
				}
			}
			else
			{
				// dispatching the event
				if (OnFtueAnimationSequenceComplete != null)
				{
					OnFtueAnimationSequenceComplete(ftueType);
				}
			}
		}

		#endregion
	}
}