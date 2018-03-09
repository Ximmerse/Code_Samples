using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	public class SithLightning : MonoBehaviour 
	{

		public SithLightningNode Start;
		public SithLightningNode End;
		public LineRenderer Line;
		public int NumPoints;
		public float Speed = 1f;
		public float Strength = 1f;
		public float LerpSpeed = 10f;

		private float randX, randY, randZ, randOffset;

		private Vector3[] positionArray;
		private float[] pointInfluences;

		private void Awake()
		{
			Line.positionCount = NumPoints;
			positionArray = new Vector3[NumPoints];
			pointInfluences = new float[NumPoints];

			// Randomize wave direction
			randX = Random.Range(-1f, 1f);
			randY = Random.Range(-1f, 1f);
			randZ = Random.Range(-1f, 1f);
			randOffset = Random.Range(-1f, 1f);

			// Randomize influence per point
			for (int i = 0; i < NumPoints; i++)
			{
				pointInfluences[i] = Random.Range(0f, 1f);
			}

		}

		private void Update()
		{
			for (int i = 0; i < NumPoints; i++)
			{
				if (i == 0)
				{
					positionArray[i] = Vector3.Lerp(ConvertToWorldSpace(positionArray[i]),
						ConvertToWorldSpace(Start.transform.position), Time.deltaTime * LerpSpeed);
				}
				else if (i == NumPoints - 1)
				{
					positionArray[i] = Vector3.Lerp(ConvertToWorldSpace(positionArray[i]),
						ConvertToWorldSpace(End.transform.position), Time.deltaTime * LerpSpeed);
				}
				else 
				{
					float sinWave = Mathf.Sin((Time.time + i / 10f + randOffset) * Speed) * Strength;
					Vector3 vector3Wave = new Vector3(sinWave * randX * pointInfluences[i], sinWave * randY * pointInfluences[i], sinWave * randZ * pointInfluences[i]);
					positionArray[i] = ConvertToWorldSpace(Vector3.Lerp(positionArray[0], positionArray[NumPoints - 1], 1f / NumPoints * i ) + vector3Wave);
				}
			}

			Line.SetPositions(positionArray);
		}

		private Vector3 ConvertToWorldSpace(Vector3 original)
		{
			return original - transform.position;
		}

	}
}
