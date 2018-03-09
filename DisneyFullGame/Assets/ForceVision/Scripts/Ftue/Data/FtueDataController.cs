using UnityEngine;

namespace Disney.ForceVision
{
	public enum FtueType
	{
		Intro,
		GalaxyMap,
		Setup,
		SaberCalibration,
		DarkSide
	}

	public class FtueDataController
	{
		#region Constants

		public const string FtueDataFile = "ftue-data.json";

		#endregion

		#region Properties

		public static FtueData Data
		{
			get
			{
				return GetFtueData();
			}
		}

		#endregion

		#region Class Methods

		private static FtueData GetFtueData()
		{
			// creating container
			ContainerAPI container = new ContainerAPI(Game.ForceVision);

			FtueData ftueData = null;
			if (container.PersistentData.FileExists(FtueDataFile))
			{
				string data = container.PersistentData.LoadText(FtueDataFile);
				ftueData = JsonUtility.FromJson<FtueData>(data);
			}

			// cleaning up container
			container.Dispose();

			return ftueData;
		}

		private static bool SaveFtueData(FtueData ftueData)
		{
			// saving ftue data
			string json = JsonUtility.ToJson(ftueData);
			ContainerAPI container = new ContainerAPI(Game.ForceVision);
			bool success = container.PersistentData.SaveText(FtueDataController.FtueDataFile, json, false);

			if (success)
			{
				Log.Debug("FTUE data saved successfully.");
			}
			else
			{
				Log.Warning("FTUE data was not saved!");
			}
			return success;
		}

		public static bool HasPlayerCompletedFtue()
		{
			// getting FTUE data
			FtueData ftueData = GetFtueData();

			if (ftueData == null)
			{
				return false;
			}
			else
			{
				return ftueData.Is3dFtueComplete;
			}
		}

		public static bool IsFtueComplete(FtueType type)
		{
			#if IS_DEMO_BUILD
			Log.Debug("IsFtueComplete forcing True for Demo");
			return true;
			#else
			bool isComplete = false;

			// getting FTUE data
			FtueData ftueData = GetFtueData();

			if (ftueData != null)
			{
				switch(type)
				{
				case FtueType.Intro:
					isComplete = ftueData.Is3dFtueComplete;
					break;
				case FtueType.Setup:
					isComplete = ftueData.IsStereoSetupComplete;
					break;
				case FtueType.SaberCalibration:
					isComplete = ftueData.IsSaberCalibrationComplete;
					break;
				}
			}

			return isComplete;
			#endif
		}

		public static void SetFtueComplete(FtueType type, bool flag = true)
		{
			// getting FTUE data
			FtueData ftueData = GetFtueData();

			if (ftueData == null)
			{
				ftueData = new FtueData();
			}

			switch(type)
			{
			case FtueType.Intro:
				ftueData.Is3dFtueComplete = flag;
				break;
			case FtueType.Setup:
				ftueData.IsStereoSetupComplete = flag;
				break;
			case FtueType.SaberCalibration:
				ftueData.IsSaberCalibrationComplete = flag;
				break;
			}

			SaveFtueData(ftueData);
		}

		#endregion
	}
}