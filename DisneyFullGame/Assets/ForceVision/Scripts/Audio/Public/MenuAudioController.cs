using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public class MenuAudioController : MonoBehaviour
	{
		public static List<string> Triggers = new List<string>();

		#region Constants

		public const string TG = "rZUe";

		#endregion

		#region Properties

		public static bool DidAutoSelectNode = false;

		public GameObject Galaxy;
		public GameObject Holocron;
		public GameObject ContainerSoundBanks;
		GameObject InSceneContainerSounds;

		private bool pillarSelected = false;
		private object lastSelectedNode;

		#endregion

		#region MonoBehaviour

		private void Awake()
		{
			if (ContainerSoundBanks)
			{
				InSceneContainerSounds = GameObject.FindGameObjectWithTag(ContainerSoundBanks.name);
				if (InSceneContainerSounds == null)
				{
					InSceneContainerSounds = Instantiate(ContainerSoundBanks);
				}
			}
		}

		private void Start()
		{
			Galaxy.GetComponent<AnimationEvents>().OnAnimationComplete += OnAnimationComplete;

			MenuNode.OnNodeSelected += OnNodeSelected;
			MenuNode.OnNodeFocused += OnNodeFocused;
			MenuNode.OnNodeIdle += OnNodeIdle;

			// Currently this is ever only a list of one, not sure if wwise auto queues or not
			Triggers.ForEach(item => 
			{
				AudioEvent.Play(item, gameObject);
			});
			Triggers.Clear();
		}

		private void OnDestroy()
		{
			Galaxy.GetComponent<AnimationEvents>().OnAnimationComplete -= OnAnimationComplete;

			MenuNode.OnNodeSelected -= OnNodeSelected;
			MenuNode.OnNodeFocused -= OnNodeFocused;
			MenuNode.OnNodeIdle -= OnNodeIdle;

			Destroy(InSceneContainerSounds);
		}

		#endregion

		#region Event Handlers

		protected void OnAnimationComplete(object sender, AnimationEventArgs eventArgs)
		{
			if (eventArgs != null)
			{
				if (string.Equals(eventArgs.AnimationName, AnimationEventName.HolocronAppears))
				{
					// TODO: mathh010 - get audio event to play or remove (Audio - playing Holocron appears event)
				}

				if (string.Equals(eventArgs.AnimationName, AnimationEventName.GalaxyMapTurnsOn))
				{
					// Audio - playing galaxy map appears event
					AudioEvent.Play(AudioEventName.GalaxyMap.IntroMusic, Galaxy);
					AudioEvent.Play(AudioEventName.GalaxyMap.Appears, Galaxy);
				}

				if (string.Equals(eventArgs.AnimationName, AnimationEventName.HolocronAllSpinIdleStart))
				{
					// Audio - playing Holocron all spin idle event
					AudioEvent.Play(AudioEventName.Holocron.AllSpinIdle, Holocron);
				}

				if (string.Equals(eventArgs.AnimationName, AnimationEventName.HolocronAllSpinIntroStart))
				{
					// Audio - playing Holocron all spin intro event
					AudioEvent.Play(AudioEventName.Holocron.AllSpinIntro, Holocron);
				}

				if (string.Equals(eventArgs.AnimationName, AnimationEventName.HolocronClosedIdleStart))
				{
					// Audio - playing Holocron closed idle event
					AudioEvent.Play(AudioEventName.Holocron.ClosedIdle, Holocron);
				}

				if (string.Equals(eventArgs.AnimationName, AnimationEventName.HolocronCornerExpandStart))
				{
					// Audio - playing Holocron corner expand event
					AudioEvent.Play(AudioEventName.Holocron.CornerExpand, Holocron);
				}

				if (string.Equals(eventArgs.AnimationName, AnimationEventName.HolocronCornerExpandIdleStart))
				{
					// Audio - playing Holocron corner expand idle event
					AudioEvent.Play(AudioEventName.Holocron.CornerExpandIdle, Holocron);
				}

				if (string.Equals(eventArgs.AnimationName, AnimationEventName.HolocronCornerReturnStart))
				{
					// Audio - playing Holocron corner return event
					AudioEvent.Play(AudioEventName.Holocron.CornerReturn, Holocron);
				}

				if (string.Equals(eventArgs.AnimationName, AnimationEventName.HolocronCornerSpinStopStart))
				{
					// Audio - playing Holocron corner spin stop event
					AudioEvent.Play(AudioEventName.Holocron.CornerSpinStop, Holocron);
				}

				if (string.Equals(eventArgs.AnimationName, AnimationEventName.HolocronCornerStopIdleStart))
				{
					// Audio - playing Holocron corner stop idle event
					AudioEvent.Play(AudioEventName.Holocron.CornerStopIdle, Holocron);
				}

				if (string.Equals(eventArgs.AnimationName, AnimationEventName.HolocronCornerTurnStart))
				{
					// Audio - playing Holocron corner turn event
					AudioEvent.Play(AudioEventName.Holocron.CornerTurn, Holocron);
				}
			}
		}

		protected void OnNodeFocused(object sender, MenuNodeEventArgs eventArgs)
		{
			// Audio - playing the node highlighted audio
			AudioEvent.Play(AudioEventName.GalaxyMap.NodeHighlighted, Galaxy);

			// playing audio clip based on which node type was given focus
			if (eventArgs.NodeType == MenuNodeType.Planet)
			{
				// TODO: mathh010 - implement audio events for following planets (once available)
				if (eventArgs.Planet == PlanetType.Hoth)
				{
					// TODO: mathh010 update how first gaze is played based on design requirements
					//AudioEvent.PlayOnce(AudioEventName.Archivist.GalaxyMap.HothFirstGazeNode, Holocron);
				}
				else if (eventArgs.Planet == PlanetType.Takodana)
				{
					// TODO: mathh010 update how first gaze is played based on design requirements
					//AudioEvent.PlayOnce(AudioEventName.Archivist.GalaxyMap.TakodanaFirstGazeNode, Holocron);
				}
			}
		}

		protected void OnNodeIdle(object sender, MenuNodeEventArgs eventArgs)
		{
			
		}

		protected void OnNodeSelected(object sender, MenuNodeEventArgs eventArgs)
		{
			// Audio - MAP_UI_GalaxyMap_NodeSelect - playing node selected
			AudioEvent.Play(AudioEventName.GalaxyMap.NodeSelected, Galaxy);

			// playing audio clip based on which node type was selected
			if (eventArgs.NodeType == MenuNodeType.Planet)
			{
				bool played = false;

				// checking if the player has selected Crait (bonus planet)
				if (eventArgs.BonusPlanet != null)
				{
					if (lastSelectedNode == null || !(lastSelectedNode is SurfaceMenuNode))
					{
						played = AudioEvent.PlayOnceEver(AudioEventName.GalaxyMap.SelectCraitFirstTime, Holocron);

						if (!played && !DidAutoSelectNode)
						{
							AudioEvent.Play(AudioEventName.GalaxyMap.SelectCrait, Holocron);

							DidAutoSelectNode = false;
						}
					}
				}
				else
				{
					if (!pillarSelected)
					{
						if (eventArgs.Planet != PlanetType.Naboo)
						{
							played = AudioEvent.PlayOnceEver(AudioEventName.Archivist.GalaxyMap.UnlockPlanet.Replace("#planet#", eventArgs.Planet.ToString()), Holocron);
						}

						if (!played)
						{
							AudioEvent.Play(AudioEventName.Archivist.GalaxyMap.FunFactPlanet.Replace("#planet#", eventArgs.Planet.ToString()), Holocron);
						}
					}
				}

				pillarSelected = false;
			}
			else if (eventArgs.NodeType == MenuNodeType.Pillar || eventArgs.NodeType == MenuNodeType.Surface)
			{
				pillarSelected = true;
			}

			lastSelectedNode = sender;
		}

		#endregion
	}
}