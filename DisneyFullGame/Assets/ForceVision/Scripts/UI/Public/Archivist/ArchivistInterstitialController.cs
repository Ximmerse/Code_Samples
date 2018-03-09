using System;
using System.Collections.Generic;
using UnityEngine;
using Disney.Vision;
using Disney.ForceVision.Internal;

namespace Disney.ForceVision
{
	public class ArchivistInterstitialController : MonoBehaviour
	{
		public static List<string> Triggers = new List<string>();

		public bool UsePrediction = true;

		public Animator Animator;
		public Animator RigAnimator;
		public VisionSDK Sdk;

		private ContainerAPI container;
		private GazeListener gazeListener;

		private void Start()
		{
			// Our Container API
			container = new ContainerAPI(Game.ForceVision);
			container.NativeBridge.OnLowMemory += OnLowMemory;

			// Setup the SDK
			Sdk.SetLogger(new VisionSdkLoggerProxy());
			OnSDKReady();

			Sdk.StereoCamera.UseMagnetometerCorrection = false;

			if (Triggers.Count > 0)
			{
				PlayAnimation(Triggers[0]);
				Triggers.RemoveAt(0);
			}
		}

		public void Exit()
		{
			container.LoadNextScene(false, false);
		}

		public void PlayAnimation(string animation, bool rigAnimator = false)
		{
			if (rigAnimator)
			{
				RigAnimator.Play(animation);
			}
			else
			{
				Animator.Play(animation);
			}
		}

		public void TriggerAnimation(string trigger)
		{
			RigAnimator.SetTrigger(trigger);
		}

		private void OnDestroy()
		{
			// Remove Input Events
			Sdk.Input.OnButtonUp -= OnButtonUp;

			// Kill SDK
			Sdk = null;

			// Kill the Container
			container.NativeBridge.OnLowMemory -= OnLowMemory;
			container.Dispose();
			container = null;
		}

		private void OnSDKReady()
		{
			// Add the audio Listener
			Sdk.StereoCamera.gameObject.AddComponent<AkAudioListener>();

			// Setup Controllers
			ControllerPeripheral controller = new ControllerPeripheral(VisionSDK.ControllerName, null, null, container.GetSavedSaberColorID());
			controller.UsePositionPrediction = UsePrediction;
			Sdk.Connections.AddPeripheral(controller);

			// Input Events
			Sdk.Input.OnButtonUp += OnButtonUp;
		}

		private void OnButtonUp(object sender, ButtonEventArgs eventArguments)
		{
		}

		private void OnItemGazedAt(object sender, GazeEventArgs eventArguments)
		{
		}

		private void OnItemGazedOff(object sender, GazeEventArgs eventArguments)
		{
		}

		private void OnLowMemory(object sender, EventArgs eventArguments)
		{
			Log.Debug("Low Memory Warning");
		}
	}
}