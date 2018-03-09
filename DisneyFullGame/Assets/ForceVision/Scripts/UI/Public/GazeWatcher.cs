using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Disney.Vision;

namespace Disney.ForceVision
{
	public struct GazeListener
	{
		public Type[] ClassTypes;
		public EventHandler<GazeEventArgs> OnOver;
		public EventHandler<GazeEventArgs> OnOut;

		public GazeListener(Type[] classTypes, EventHandler<GazeEventArgs> onOver, EventHandler<GazeEventArgs> onOut)
		{
			ClassTypes = classTypes;
			OnOver = onOver;
			OnOut = onOut;
		}
	}

	public class GazeWatcher : MonoBehaviour
	{
		public VisionSDK Sdk;
		public GameObject Reticle;
		public bool ShowReticle = false;
		public LayerMask SpecificLayersToHit;

		[HideInInspector]
		public List<Transform> CurrentGazedItems = new List<Transform>();

		public bool RaycastEnabled 
		{
			get
			{
				return raycastEnabled;
			}

			set
			{
				raycastEnabled = value;

				if (!raycastEnabled)
				{
					CurrentGazedItems.Clear();
				}
			}
		}

		private List<GazeListener> listeners = new List<GazeListener>();
		private bool raycastEnabled;

		public void AddListener(GazeListener listener)
		{
			listeners.Add(listener);	
		}

		public void RemoveListener(GazeListener listener)
		{
			listeners.Remove(listener);
		}

		private void Awake()
		{
			RaycastEnabled = true;
		}

		private void Start()
		{
			Reticle.transform.SetParent(Sdk.StereoCamera.transform, false);
			Reticle.SetActive(ShowReticle);
		}

		private void Update()
		{
			if (!RaycastEnabled)
			{
				Reticle.SetActive(false);
				return;
			}

			Reticle.SetActive(ShowReticle);

			RaycastHit[] hits = MiddleEyeRaycastAll();
			Transform[] oldItems = CurrentGazedItems.ToArray();

			CurrentGazedItems.Clear();

			float distance = 100.0f;

			for (int i = 0; i < hits.Length; i++)
			{
				if (hits[i].transform == null)
				{
					continue;
				}

				// Position Crosshair in front of Object Hit
				float hitDistance = Vector3.Distance(Reticle.transform.parent.position, hits[i].point);

				if (hitDistance < distance)
				{
					distance = hitDistance;
				}

				CurrentGazedItems.Add(hits[i].transform);

				if (oldItems.Contains(hits[i].transform))
				{
					continue;
				}

				// For each listener
				for (int index = 0; index < listeners.Count; index++)
				{
					// That has an over listener
					if (listeners[index].OnOver != null)
					{
						// Filter based on the list of types
						for (int classIndex = 0; classIndex < listeners[index].ClassTypes.Length; classIndex++)
						{
							// If this collision has one, inform the listener
							if (hits[i].transform.GetComponent(listeners[index].ClassTypes[classIndex]))
							{
								listeners[index].OnOver(this, new GazeEventArgs(hits[i].transform));
							}
						}
					}
				}
			}

			for (int i = 0; i < oldItems.Length; i++)
			{
				if (!CurrentGazedItems.Contains(oldItems[i]))
				{
					// For each listener
					for (int index = 0; index < listeners.Count; index++)
					{
						// That has an out listener
						if (listeners[index].OnOut != null)
						{
							// Filter based on the list of types
							for (int classIndex = 0; classIndex < listeners[index].ClassTypes.Length; classIndex++)
							{
								// If this collision has one, inform the listener
								if (oldItems[i].transform.GetComponent(listeners[index].ClassTypes[classIndex]))
								{
									listeners[index].OnOut(this, new GazeEventArgs(oldItems[i].transform));
								}
							}
						}
					}
				}
			}

			if (Reticle.activeSelf && hits.Length > 0)
			{
				Reticle.transform.localPosition = new Vector3(0, 0, distance - 0.05f);

				// Scale between 0.03 -> 0.08 for 1.3 -> 3.3.
				float scale = ((0.08f - 0.03f) * (Reticle.transform.localPosition.z - 1.3f) / (3.3f - 1.3f)) + 0.03f;
				Reticle.transform.localScale = new Vector3(scale, scale, scale);
			}
		}

		private RaycastHit[] MiddleEyeRaycastAll()
		{
			int layerMask = SpecificLayersToHit.value;

			if (layerMask != 0)
			{
				return Physics.RaycastAll(Sdk.StereoCamera.transform.position,
				                          Sdk.StereoCamera.transform.forward,
				                          Mathf.Infinity,
				                          layerMask);
			}
			else
			{
				return Physics.RaycastAll(Sdk.StereoCamera.transform.position, Sdk.StereoCamera.transform.forward);
			}
		}
	}
}