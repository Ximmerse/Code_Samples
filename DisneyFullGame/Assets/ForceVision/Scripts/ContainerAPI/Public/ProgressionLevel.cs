using System;

namespace Disney.ForceVision
{
	[Serializable]
	public class ProgressionLevel
	{
		public int LevelHash;

		public int Difficulty;

		public int HitsTaken;

		public ProgressionLevel(PillarConfig config, int difficulty, int hitsTaken = 0)
		{
			LevelHash = config.GetHashCode();
			Difficulty = difficulty;
			HitsTaken = hitsTaken;
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="Disney.ForceVision.ProgressionLevel"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="Disney.ForceVision.ProgressionLevel"/>.</returns>
		new public string ToString()
		{
			return "LevelHash = " + LevelHash.ToString() + " | Difficulty = " + Difficulty.ToString() + " | HitsTaken = " + HitsTaken;
		}
	}
}