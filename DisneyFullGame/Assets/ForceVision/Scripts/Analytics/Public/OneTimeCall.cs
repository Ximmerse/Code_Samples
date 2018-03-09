using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Disney.Vision;
using Ximmerse.InputSystem;

namespace Disney.ForceVision
{
	/// <summary>
	/// One time call to Lenovo for sell-through data.
	/// </summary>
	public class OneTimeCall : MonoBehaviour
	{
		private const char separator = (char)1;
		private const string playerprefsKey = "one time call made";

		private static bool callMadeCached = false;

		// if this is true, the call will be made multiple times.
		// if this is false, the call will only be made once.
		private const bool testCall = false;

		/// <summary>
		/// Gets whether or not the call has already been made.
		/// </summary>
		/// <returns><c>true</c>, if the call has already been made, <c>false</c> otherwise.</returns>
		private static bool GetCallMade()
		{
			if (testCall)
			{
				return false;
			}

			if (callMadeCached)
			{
				return true;
			}

			PlayerPrefsStorage storage = new PlayerPrefsStorage(Game.ForceVision);
			callMadeCached = storage.GetPrefInt(playerprefsKey, 0) > 0;

			return callMadeCached;
		}

		public static void Init(PlatformConfig platformConfig, HmdPeripheral hmd)
		{
			if (GetCallMade())
			{
				return;
			}

			Log.Debug("[Init] " + platformConfig);

			(new GameObject("OneTimeCall")).AddComponent<OneTimeCall>().Send(platformConfig, hmd);
		}

		/// <summary>
		/// Sets that the call has been made.
		/// </summary>
		private static void SetCallMade()
		{
			PlayerPrefsStorage storage = new PlayerPrefsStorage(Game.ForceVision);
			storage.SetPrefInt(playerprefsKey, 1);
			callMadeCached = true;
		}

		/// <summary>
		/// Send the call.
		/// </summary>
		private void Send(PlatformConfig platformConfig, HmdPeripheral hmd)
		{
			if (hmd == null || !hmd.Connected)
			{
				return;
			}

			int handle = XDevicePlugin.GetInputDeviceHandle("VRDevice");

			const string SDKVersion = "3.0";
			const string DeviceIDType = "HMD SN";
			string DeviceID = XDevicePlugin.GetString(handle,
                                                        XDevicePlugin.kField_HMDSN8Object,
			                                          "HMD-SerialNumber-MISSING");
			string OsVersion = XDevicePlugin.GetString(handle,
			                                           XDevicePlugin.kField_FirmwareRevisionObject,
			                                           "HMD-FirmwareVersion-MISSING");
			string Language = Application.systemLanguage.ToString();
			string Country = Language;
			if (System.Globalization.RegionInfo.CurrentRegion != null)
			{
				Country = System.Globalization.RegionInfo.CurrentRegion.TwoLetterISORegionName;
			}
			string DeviceModel = hmd.GetModelName();
			if (string.IsNullOrEmpty(DeviceModel))
			{
				DeviceModel = "HMD-ModelName-MISSING";
			}
			const string Manufacturer = "Lenovo";
			string Resolution = Screen.width.ToString() + "x" + Screen.height.ToString();
			string BundleID = BundleVersion.Get();
			string BundleCode = Application.isEditor ? "1" : BundleVersion.Get(false);
			string Channel = (Application.platform == RuntimePlatform.Android) ? "Google Play" : "iTunes";

			string message = "HLog" + separator +
			                 SDKVersion + separator +
			                 DeviceIDType + separator +
			                 DeviceID + separator +
			                 platformConfig.Key + separator +
			                 OsVersion + separator +
			                 Language + separator +
			                 Country + separator +
			                 DeviceModel + separator +
			                 Manufacturer + separator +
			                 Resolution + separator +
			                 "NULL" + separator +
			                 "NULL" + separator +
			                 "NULL" + separator +
			                 "NULL" + separator +
			                 "NULL" + separator +
			                 "NULL" + separator +
			                 "NULL" +
			                 "\n" +
			                 BundleID + separator +
			                 BundleCode + separator +
			                 Channel +

							// extra junk that OTC wants
			                 "\n" +
			                 "NULL" + separator +
			                 "0" + separator +
			                 "0" + separator +
			                 "0" + separator +
			                 "NULL" + separator +
			                 "NULL" + separator +
			                 "NULL" + separator +
			                 "NULL" + separator +
			                 "NULL" + separator +
			                 "NULL" + separator +
			                 "NULL" + separator +
			                 "NULL" + separator +
			                 "NULL" + separator +
			                 "NULL" + separator +
			                 "NULL" + separator +
			                 "NULL" + separator +
			                 "NULL" + separator +
			                 "NULL" + separator +
			                 "NULL" + separator +
			                 "NULL" + separator +
			                 "0" + separator +
			                 "0" + separator +
			                 "0" + separator +
			                 "0" + separator +
			                 "NULL\u0003" +
			                 "NULL\u0002" +
			                 "NULL\u0003" +
			                 "NULL";

			if (testCall)
			{
				Log.Debug(message);
			}

			StartCoroutine(Upload(message, platformConfig));

			ForceVisionAnalytics.LogOneTimeCall(DeviceID, Channel, Country, BundleCode, OsVersion, SDKVersion);
		}

		/// <summary>
		/// Upload the specified message and platformConfig.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="platformConfig">Platform config.</param>
		private IEnumerator Upload(string message, PlatformConfig platformConfig)
		{
			string URL = platformConfig.Uri;
//			URL = "http://chifsr.lenovomm.com/reaper/server/report3";
			WWWForm formData = new WWWForm();
			formData.AddBinaryData("data", System.Text.Encoding.UTF8.GetBytes(message));
			UnityWebRequest www = UnityWebRequest.Post(URL, formData);
			www.SetRequestHeader("APPTOKEN", platformConfig.Key);
			yield return www.Send();

			#if UNITY_2017
			if (www.isNetworkError)
			#else
			if (www.isError)
			#endif
			{
				Log.Error("Error sending one-time call: " + www.error);
			}
			else
			{
				Log.Debug("call successful! " + www.responseCode.ToString());
				SetCallMade();
			}

			Destroy(gameObject);
		}
	}
}