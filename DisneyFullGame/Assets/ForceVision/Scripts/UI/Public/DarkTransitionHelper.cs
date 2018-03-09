using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Disney.ForceVision
{
	[ExecuteInEditMode]
	public class DarkTransitionHelper : MonoBehaviour 
	{
		// Allows animator to control values on a mesh that has a separate Animator

		public Color AnimatedColor;
		[Range(0.0f, 1.0f)]
		public float DarkTransition = 1f;
		public SkinnedMeshRenderer SkinnedMesh;
		public MeshRenderer Mesh;

		private void Update () 
		{
			try
			{
				if (Mesh)
				{
					Mesh.sharedMaterial.SetColor("_RimColor", AnimatedColor);
					Mesh.sharedMaterial.SetFloat("_TransitionPower", DarkTransition);
				}

				if (SkinnedMesh)
				{
					SkinnedMesh.sharedMaterial.SetColor("_RimColor", AnimatedColor);
					SkinnedMesh.sharedMaterial.SetFloat("_TransitionPower", DarkTransition);
				}
			}
			catch (Exception e)
			{
				return;
			}

		}
	}
}