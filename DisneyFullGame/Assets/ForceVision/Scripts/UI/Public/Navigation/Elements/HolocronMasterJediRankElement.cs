using UnityEngine;

namespace Disney.ForceVision
{
	public class HolocronMasterJediRankElement : MonoBehaviour
	{
		#region Public Properties

		public GameObject Master;
		public GameObject Knight;
		public GameObject Padawan;
		public GameObject Initiate;

		#endregion

		#region Unity Methods

		public void Start()
		{
			Master.SetActive(false);
			Knight.SetActive(false);
			Padawan.SetActive(false);
			Initiate.SetActive(false);

			switch (ContainerAPI.GetJediRank())
			{
				case JediRank.Master:
					Master.SetActive(true);
				break;

				case JediRank.Knight:
					Knight.SetActive(true);
				break;

				case JediRank.Padawan:
					Padawan.SetActive(true);
				break;

				default:
					Initiate.SetActive(true);
				break;
			}
		}

		#endregion
	}
}