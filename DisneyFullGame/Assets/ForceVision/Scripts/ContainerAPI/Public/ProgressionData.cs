using System;
using System.Collections.Generic;

namespace Disney.ForceVision
{
	[Serializable]
	public class ProgressionData
	{
		public List<ProgressionLevel> LevelsBeat = new List<ProgressionLevel>();
		public List<ProgressionLevel> LevelsLost = new List<ProgressionLevel>();
		public bool DarkArchivistUnlocked = false;
		public bool InsightMedalUnlocked = false;
		public bool LeadershipMedalUnlocked = false;
		public bool CombatUnlocked = false;
		public bool AdvancedCombatUnlocked = false;
		public bool MasteryUnlocked = false;

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="Disney.ForceVision.ProgressionData"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="Disney.ForceVision.ProgressionData"/>.</returns>
		new public string ToString()
		{
			string s = "LevelsBeat:\n";
			for (int i = 0; i < LevelsBeat.Count; i++)
			{
				s += LevelsBeat[i].ToString() + "\n";
			}
			return s;
		}
	}
}