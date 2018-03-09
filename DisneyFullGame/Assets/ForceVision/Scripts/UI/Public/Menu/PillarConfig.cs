using UnityEngine;
using SG.Lonestar;
using BSG.SWARTD;

namespace Disney.ForceVision
{
	[CreateAssetMenu(fileName = "Planet_Track_PillarNumber", menuName = "ForceVision/Pillar Config", order = 1)]
	public class PillarConfig : ScriptableObject
	{
		#region All

		public Game Game;

		public PlanetType Planet;

		public BonusPlanetType BonusPlanet;

		public PillarConfig PreviousConfig;

		public bool IsBonusPlanet = false;

		public bool[] Interstitial = { false, false, false };

		public string[] InterstitialTrigger = { "", "", "" };

		public string[] LostVOTrigger = { "", "", "" };

		public int PillarNumber;

		#endregion

		#region Duel

		public DuelAPI.Duelist Duelist;

		#endregion

		#region Tower Defence

		public TDAPI.Battles Battle;

		#endregion

		public static bool operator ==(PillarConfig first, PillarConfig second)
		{
			if (object.ReferenceEquals(first, null) || object.ReferenceEquals(second, null))
			{
				return (object.ReferenceEquals(first, null) && object.ReferenceEquals(second, null));
			}

			if (second.Game != first.Game)
			{
				return false;
			}

			switch (first.Game)
			{
				case Game.Assault:
				case Game.HoloChess:
					if (first.Planet == second.Planet && first.PillarNumber == second.PillarNumber)
					{
						return true;
					}
					break;

				case Game.Duel:
					if (first.Duelist == second.Duelist)
					{
						return true;
					}
					break;

				case Game.TowerDefense:
					if (first.Battle == second.Battle)
					{
						return true;
					}
					break;
			}

			return false;
		}

		public static bool operator !=(PillarConfig first, PillarConfig second)
		{
			return !(first == second);
		}

		public override bool Equals(object other)
		{
			return (other is PillarConfig) && this == (PillarConfig)other;
		}

		public override int GetHashCode()
		{
			switch (Game)
			{
				case Game.Assault:
				case Game.HoloChess:
					return (int)Game + ((int)Planet * 100) + ((int)PillarNumber * 1000);

				case Game.Duel:
					return (int)Game + ((int)Duelist * 10000);

				case Game.TowerDefense:
					return (int)Game + ((int)Battle * 100000);
			}

			return base.GetHashCode();
		}

		public override string ToString()
		{
			switch (Game)
			{
				case Game.Assault:
					return "[Assault Config] " + GetPlanetName() + ": Pillar: " + PillarNumber;

				case Game.Duel:
					return "[Duel Config] " + GetPlanetName() + ": Duelist: " + Duelist;

				case Game.HoloChess:
					return "[HoloChess Config] " + GetPlanetName() + ": Pillar: " + PillarNumber;

				case Game.TowerDefense:
					return "[TowerDefense Config] " + GetPlanetName() + ": Battle: " + Battle;
			}

			return "[Unknwon Config]";
		}

		public string GetTokenString()
		{
			string configString = "";

			switch (Game)
			{
				case Game.Assault:
				case Game.HoloChess:
					configString = Game + "." + GetPlanetName() + "." + PillarNumber;
					break;

				case Game.Duel:
					configString = Game + "." + GetPlanetName() + "." + Duelist;
					break;

				case Game.TowerDefense:
					configString = Game + "." + GetPlanetName() + "." + Battle;
					break;
			}

			return configString;
		}

		/// <summary>
		/// Context string formatted for analytics.
		/// </summary>
		/// <returns>The context string.</returns>
		public string ContextForAnalytics()
		{
			int planetNum = (IsBonusPlanet ? (int)BonusPlanet + System.Enum.GetNames(typeof(PlanetType)).Length : (int)Planet) + 1;
			return planetNum.ToString() + "_" + GetPlanetName() + "_" + Game.ToString();
		}

		private string GetPlanetName()
		{
			return (IsBonusPlanet ? BonusPlanet.ToString() : Planet.ToString());
		}
	}
}