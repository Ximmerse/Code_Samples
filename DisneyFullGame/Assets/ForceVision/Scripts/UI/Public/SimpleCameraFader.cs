using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Disney.ForceVision
{
	public class SimpleCameraFader : MonoBehaviour
	{
		public Disney.Vision.VisionSDK Sdk;
		public LayerMask FadedOutLayer;

		private List<Camera> cameras;

		private Material fadeMaterial = null;
		private float duration = 0.0f;
		private float totalDuration = 0.0f;
		private Color color = Color.black;
		private bool fading = false;
		private Action callback;

		private void Init()
		{
			if (!Sdk)
			{
				Debug.LogError("No SDK connected!");
				enabled = false;
				return;
			}

			cameras = new List<Camera>();
			cameras.Add(Sdk.StereoCamera.LeftCamera);
			cameras.Add(Sdk.StereoCamera.RightCamera);
			cameras.Add(Sdk.StereoCamera.SingleCamera);
		}

		public void Fade(float duration, Action callback)
		{
			if (cameras == null || cameras.Count < 1)
			{
				Init();
			}

			Shader shader = Shader.Find("Hidden/Internal-Colored");
			fadeMaterial = new Material(shader);
			fadeMaterial.hideFlags = HideFlags.HideAndDontSave;
			fadeMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
			fadeMaterial.SetInt("_ZWrite", 0);
			fadeMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);

			fading = true;
			this.callback = callback;
			this.duration = duration;
			totalDuration = duration;

			Camera.onPostRender += PostRender;
		}

		private void OnDestroy()
		{
			Camera.onPostRender -= PostRender;
		}

		private void Update()
		{
			if (fading)
			{
				duration -= Time.unscaledDeltaTime;
				float alpha = Mathf.Max(0.0f, duration) / totalDuration;
				color = new Color(0.0f, 0.0f, 0.0f, 1 - alpha);
			}
		}

		private void PostRender(Camera currentCamera)
		{
			if (cameras == null || cameras.Count < 1)
			{
				Init();
			}

			// Not fading or wrong camera
			if (!fading || !cameras.Contains(currentCamera))
			{
				return;
			}

			// if it is visible
			if (color.a > 0.000001f)
			{
				GL.PushMatrix();
				GL.LoadOrtho();

				fadeMaterial.SetPass(0);

				GL.Begin(GL.QUADS);
				GL.Color(color);
				GL.Vertex3(0, 0, 0);
				GL.Vertex3(1, 0, 0);
				GL.Vertex3(1, 1, 0);
				GL.Vertex3(0, 1, 0);
				GL.End();

				GL.PopMatrix();
			} 

			// fully hidden
			if (color.a > 0.99999f)
			{
				for (int i = 0; i < cameras.Count; i++)
				{
					cameras[i].cullingMask = FadedOutLayer;
					cameras[i].backgroundColor = color;
				}

				fading = false;
				Camera.onPostRender -= PostRender;

				if (callback != null)
				{
					callback();
				}
			}
		}
	}
}