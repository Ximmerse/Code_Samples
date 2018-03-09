using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Disney.ForceVision
{
	[ExecuteInEditMode]
	public class FadeHelper : MonoBehaviour 
	{

		public Transform StartFade;
		public Transform EndFade;
		public SkinnedMeshRenderer SkinnedMesh;
		public MeshRenderer Mesh;

		private void Update () 
		{
			try
			{
				if (Mesh)
				{
					Mesh.sharedMaterial.SetFloat ("_FadeStart", StartFade.position.y);
					Mesh.sharedMaterial.SetFloat ("_FadeEnd", EndFade.position.y);
				}

				if (SkinnedMesh)
				{
					SkinnedMesh.sharedMaterial.SetFloat ("_FadeStart", StartFade.position.y);
					SkinnedMesh.sharedMaterial.SetFloat ("_FadeEnd", EndFade.position.y);
				}
			}
			catch (Exception e)
			{
				return;
			}
					
		}
	}
}
