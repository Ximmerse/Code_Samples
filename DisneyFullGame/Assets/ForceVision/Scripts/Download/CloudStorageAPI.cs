using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEngine;

namespace Disney.ForceVision
{
	public class CloudData
	{
		public string ServiceAccountEmail = "";
		public string GoogleApiUrl = "";
		public string P12Path = "";
		public string Password = "";
		public string MapFile = "";
	}

	public class CloudStorageAPI
	{

		private CloudData cloudData;

		/// The expiration time of the access token, in seconds. This value is what Google sets
		/// for all provided tokens even if a smaller expiration is requested.
		public const int expiration = 60 * 60;

		private const string HeaderAuthorization = "Authorization";
		private const string AuthorizationTypeBearer = "Bearer";

		private const string ScopeStorageReadOnly = "https://www.googleapis.com/auth/devstorage.read_only";

		private const string ApiEndpoint = "https://www.googleapis.com/oauth2/v3/token";
		private const string ClaimSetSchema = "{{\"iss\":\"{0}\",\"scope\":\"{1}\",\"aud\":\"{2}\",\"iat\":{3},\"exp\":{4}}}";
		private const string ClaimHeader = "{\"alg\":\"RS256\",\"typ\":\"JWT\"}";
		private const string AssertionType = "http://oauth.net/grant_type/jwt/1.0/bearer";

		private MonoBehaviour requestRunner;
		private string buildPhase;

		public static bool CanAttemptToConnectToCDN()
		{
			#if RC_BUILD
			return true;
			#else
			CloudStorageAPI cloudStorageAPI = new CloudStorageAPI(null);
			StreamingAssetsStorage streamingAssetsStorage = new StreamingAssetsStorage(Game.None, null);
			bool ret = true;
			streamingAssetsStorage.LoadStreamingAssetsFile(cloudStorageAPI.cloudData.P12Path, (error, p12Bytes) =>
			{
				if (!string.IsNullOrEmpty(error))
				{
					Log.Warning("Unable to load " + cloudStorageAPI.cloudData.P12Path);
					ret = false;
				}
			}, true);
			if (string.IsNullOrEmpty(cloudStorageAPI.cloudData.Password))
			{
				Log.Warning("No cloudData.Password specified " + cloudStorageAPI.cloudData.Password);
				ret = false;
			}
			if (!ret)
			{
				Log.Warning("You need to have both a .p12 file and password to run this locally");
			}
			return ret;
			#endif
		}

		public CloudStorageAPI(MonoBehaviour runner, string aBuildPhase = "", CloudData aCloudData = null)
		{
			requestRunner = runner;

			if (string.IsNullOrEmpty(buildPhase) == true)
			{
				aBuildPhase = "CI_BUILD";

				#if QA_BUILD
				aBuildPhase = "QA_BUILD";
				#endif

				#if RC_BUILD
				aBuildPhase = "RC_BUILD";
				#endif
			}

			this.buildPhase = aBuildPhase;
			Log.Debug("buildPhase = " + this.buildPhase);

			if (aCloudData == null)
			{
				cloudData = new CloudData();
				#if CI_BUILD
				cloudData.GoogleApiUrl = "https://storage.googleapis.com/bbeed654-6fb3-3ca6-ab2d-fca99072ae52.disney.io";
				cloudData.P12Path = "creds/mirageKeystoreDev.p12";
				cloudData.Password = VaultSecrets.GetSecretValue("MIRAGE_P12_PASSWORD_DEV");
				cloudData.ServiceAccountEmail = "svc-mirage-rw@di-mirage-us-nonprod-1.iam.gserviceaccount.com";
				cloudData.MapFile = "__mapping_mirage_dev1.json";
				#endif

				#if QA_BUILD
				cloudData.GoogleApiUrl = "https://storage.googleapis.com/cff292a6-34d4-44fe-a6f0-126d3dad414f";
				cloudData.P12Path = "creds/mirageKeystoreQA.p12";
				cloudData.Password = VaultSecrets.GetSecretValue("MIRAGE_P12_PASSWORD_QA");
				cloudData.ServiceAccountEmail = "svc-mirage-r@di-mirage-us-nonprod-2.iam.gserviceaccount.com";
				cloudData.MapFile = "__mapping_mirage_qa1.json";
				#endif

				#if RC_BUILD
				cloudData.GoogleApiUrl = "https://de079c2d.content.disney.io";
				cloudData.P12Path = "";
				cloudData.Password = "";
				cloudData.ServiceAccountEmail = "";
				cloudData.MapFile = "__mapping_mirage_prod.json";
				#endif
			}
			else
			{
				cloudData = aCloudData;
			}
		}

		public void AuthenticateWithGAE(Action<Response> callback = null, Action<float> fileProgressCallback = null, Func<string, Dictionary<string, string>, Dictionary<string, string>, int> OverrideRequestMethod = null)
		{
			Log.Debug("[DEBUG-AUTH] CloudStorageAPI AuthenticateWithGAE", Log.LogChannel.General);

			#if RC_BUILD
				callback(null);
				return;
			#endif

			Dictionary<string, string> requestFields = new Dictionary<string, string>();
			Dictionary<string, string> headers = new Dictionary<string, string>();
			X509Certificate2 cert;
			try
			{
				StreamingAssetsStorage streamingAssetsStorage = new StreamingAssetsStorage(Game.None, null);
				byte[] bytes = null;
				streamingAssetsStorage.LoadStreamingAssetsFile(cloudData.P12Path, (success, p12Bytes) =>
				{
					bytes = p12Bytes;
				}, true);

				Log.Debug("p12 bytes length =  " + bytes.Length);

				cert = new X509Certificate2(bytes, cloudData.Password, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
				
				RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
				rsa.FromXmlString(cert.PrivateKey.ToXmlString(true));

				long assertionTime = GetTimeInSeconds();

				string claimSet = string.Format(ClaimSetSchema, cloudData.ServiceAccountEmail, ScopeStorageReadOnly, ApiEndpoint, assertionTime, assertionTime + expiration);

				string headerClaim = string.Format("{0}.{1}", Convert.ToBase64String(Encoding.UTF8.GetBytes(ClaimHeader)), Convert.ToBase64String(Encoding.UTF8.GetBytes(claimSet)));

				byte[] dataToSign = Encoding.UTF8.GetBytes(headerClaim);
				byte[] signedData = rsa.SignData(dataToSign, "SHA256");
				string sgn = Convert.ToBase64String(signedData);
				string jwt = string.Format("{0}.{1}", headerClaim, sgn);

				requestFields["grant_type"] = "assertion";
				requestFields["assertion_type"] = AssertionType;
				requestFields["assertion"] = jwt;

				headers["Content-Type"] = "application/x-www-form-urlencoded";
			}
			catch (Exception e)
			{
				#if CI_BUILD || QA_BUILD || RC_BUILD
				Log.Exception(e);
				#else
				Log.Warning("missing p12 file: " + cloudData.P12Path + e.Message);
				#endif
				callback(null);
				return;
			}
			if (OverrideRequestMethod != null)
			{
				OverrideRequestMethod(ApiEndpoint, headers, requestFields);
			}
			else
			{
				Request authRequest = new Request(requestRunner, callback, "authRequest", ApiEndpoint, headers, requestFields, fileProgressCallback);
				authRequest.PostData();
			}
		}

		public void GetMappingFile(Action<Response> callback, string token, Action<float> fileProgressCallback = null, Func<string, int> OverrideMappingFileRequest = null)
		{
			string mappingURL = cloudData.GoogleApiUrl + "/" + cloudData.MapFile + "?access_token=" + token;

			if (buildPhase.Equals("RC_BUILD"))
			{
				mappingURL = cloudData.GoogleApiUrl + "/" + cloudData.MapFile;
			}

			if (OverrideMappingFileRequest != null)
			{
				OverrideMappingFileRequest(mappingURL);
			}
			else
			{
				Request mappingRequest = new Request(requestRunner, callback, "manifest.json", mappingURL, null, null, fileProgressCallback);
				mappingRequest.GetData();
			}
		}

		public void GetManifest(Action<Response> callback, string token, JSONObject mapping, Action<float> fileProgressCallback = null, Func<string, int> overrideGetManifest = null, string overrideManifestVersion = null)
		{
			string manifestFile = mapping["default"]["n"].str;
			string version = Application.version;
			bool isUsingDefuaultManifest = true;
			if (string.IsNullOrEmpty(version) == false)
			{
				if (mapping[version] != null)
				{
					manifestFile = mapping[version]["n"].str;
					isUsingDefuaultManifest = false;
				}
			}
			Log.Debug(version + " maps to " + manifestFile + ". Mapping with Default = " + isUsingDefuaultManifest);
			if (overrideManifestVersion != null)
			{
				string[] parts = manifestFile.Split('.');
				parts[parts.Length - 2] = overrideManifestVersion;
				manifestFile = string.Join(".", parts);
			}
			Log.Debug("mapping version = " + manifestFile);

			string manifestURL = cloudData.GoogleApiUrl + "/" + manifestFile + "?access_token=" + token;

			if (buildPhase.Equals("RC_BUILD"))
			{
				manifestURL = cloudData.GoogleApiUrl + "/" + manifestFile;
			}

			if(overrideGetManifest != null)
			{
				overrideGetManifest(manifestURL);
			}
			else
			{
			Request manifestRequest = new Request(requestRunner, callback, "manifest.json", manifestURL, null, null, fileProgressCallback);//, headers
			manifestRequest.GetData();
			}
		}

		public void GetFile(Action<Response> callback, string baseUrl, double version, string fileName, string token, Action<float> fileProgressCallback = null)
		{
			string fileUrl = baseUrl + version + "/" + fileName + "?access_token=" + token;
			if (buildPhase.Equals("RC_BUILD"))
			{
				fileUrl = baseUrl + version + "/" + fileName;
			}

			Request fileRequest = new Request(requestRunner, callback, fileName, fileUrl, null, null, fileProgressCallback);
			fileRequest.GetData();
		}

		public void GetContentList(Action<Response> callback, string token, Action<float> fileProgressCallback = null)
		{
			Dictionary<string, string> headers = new Dictionary<string, string>();
			if (buildPhase.Equals("RC_BUILD") == false)
			{
				headers["Content-Type"] = "text/plain";
				headers.Add(HeaderAuthorization, AuthorizationTypeBearer + " " + token);
			}
			string listURL = cloudData.GoogleApiUrl + "/?marker=manifests";
			Request listRequest = new Request(requestRunner, callback, "listRequest", listURL, headers, null, fileProgressCallback);
			listRequest.GetData();
		}

		private long GetTimeInSeconds()
		{
			DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
			TimeSpan span = DateTime.UtcNow - epoch;
			return (long)span.TotalSeconds;               
		}
	}
}