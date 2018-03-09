using UnityEngine;
using UnityEditor;

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using Disney.ForceVision.Internal;

public class ConfigFieldEncryption : EditorWindow
{
	// Properties
	private string encryptSeed = "";
	private bool trueBase64 = true;
	private bool base64UrlSafe = false;
	private Vector2 scrollPos;
	private string plainTextConfigPath = "No file selected.";
	private string encryptedConfigPath = "No file selected.";
	private string encryptPlainText = "";
	private string encryptCipherText = "";
	private string decryptPlainText = "";
	private string decryptCipherText = "";

	// API
	
	// Styles
	private GUIStyle labelStyle = new GUIStyle();

	[MenuItem("Tools/DI Custom/Encryption Panel %#e")]
	public static void ShowWindow()
	{
		ConfigFieldEncryption panel = (ConfigFieldEncryption)EditorWindow.GetWindow(typeof(ConfigFieldEncryption));
		panel.Init();
	}
	
	private void Init()
	{
		// Style
		labelStyle.wordWrap = true;
		labelStyle.normal.textColor = Color.grey;
		labelStyle.padding = new RectOffset(5, 5, 5, 5);
	}
	
	private void Update()
	{
		Init();	
	}

	// Use this for initialization
	private void OnGUI()
	{
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(position.height));

		// creating title
		GUILayout.Label("External Config Encrypt/Decrypt", EditorStyles.boldLabel);

		EditorGUILayout.Separator();

		// creating encryption menu
		CreateEncryptMenu();

		EditorGUILayout.Separator();

		// creating decryption menu
		CreateDecryptMenu();

		/*
		mBase64UrlSafe = EditorGUILayout.BeginToggleGroup("Base64 RFC 4648 (url safe)", mBase64UrlSafe);
		EditorGUILayout.EndToggleGroup();
		
		EditorGUILayout.BeginHorizontal(GUIStyle.none);
		GUILayout.Label("**Please note to leave cursor on the Value field to view results", mLabelStyle);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal(GUIStyle.none);
		if (GUILayout.Button("Encrypt", GUILayout.Width(100)))
		{
			encrypt();
		}
		if (GUILayout.Button("Decrypt", GUILayout.Width(100)))
		{
			decrypt();
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal(GUIStyle.none);
		if (GUILayout.Button("Encrypt Sha1", GUILayout.Width(100)))
		{
			encryptSha1();
		}
		if (GUILayout.Button("Decrypt Sha1", GUILayout.Width(100)))
		{
			decryptSha1();
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.TextField(mResult, new GUILayoutOption[] {});
		*/

		EditorGUILayout.EndScrollView();
	}

	protected void CreateEncryptMenu()
	{
		GUILayout.Label("Encryption", EditorStyles.boldLabel);

		EditorGUILayout.BeginHorizontal(GUIStyle.none);
		{
			// creating button to select app config
			if (GUILayout.Button("Select Plain Text Config", GUILayout.Width(150)))
			{
				plainTextConfigPath = EditorUtility.OpenFilePanel("Select plain text config", "", "json");
				encryptPlainText = File.ReadAllText(plainTextConfigPath);
			}

			// creating label to display app config content (json)
			GUILayout.Label(plainTextConfigPath, labelStyle);
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginVertical(GUIStyle.none);
		{
			encryptSeed = EditorGUILayout.TextField("Seed", encryptSeed);
			EditorGUILayout.TextField("Plain Text", encryptPlainText);
			EditorGUILayout.TextField("Cipher Text", encryptCipherText);
		}
		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginHorizontal(GUIStyle.none);
		{
			if (GUILayout.Button("Encrypt", GUILayout.Width(150)))
			{
				Encrypt();
			}

			/*
			if (GUILayout.Button("Encrypt Sha1", GUILayout.Width(150)))
			{
				encryptSha1();
			}
			*/
		}
		EditorGUILayout.EndHorizontal();
	}

	protected void CreateDecryptMenu()
	{
		GUILayout.Label("Decryption", EditorStyles.boldLabel);

		EditorGUILayout.BeginHorizontal(GUIStyle.none);
		{
			// creating button to select app config
			if (GUILayout.Button("Select Encrypted Config", GUILayout.Width(150)))
			{
				encryptedConfigPath = EditorUtility.OpenFilePanel("Select encrypted config", "", "");
			}

			// creating label to display app config content (json)
			GUILayout.Label(encryptedConfigPath, labelStyle);
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginVertical(GUIStyle.none);
		{
			EditorGUILayout.TextField("Seed", encryptSeed);
			EditorGUILayout.TextField("Plain Text", decryptPlainText);
			EditorGUILayout.TextField("Cipher Text", decryptCipherText);
		}
		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginHorizontal(GUIStyle.none);
		{
			if (GUILayout.Button("Decrypt", GUILayout.Width(150)))
			{
				Decrypt();
			}

			/*
			if (GUILayout.Button("Decrypt Sha1", GUILayout.Width(150)))
			{
				decryptSha1();
			}
			*/
		}
		EditorGUILayout.EndHorizontal();
	}

	private void Encrypt()
	{
		// getting the key
		byte[] key = EncryptionUtils.VerifyKeyLength(encryptSeed, trueBase64, base64UrlSafe);

		// using the key to create the seed
		if (trueBase64)
		{
			if (base64UrlSafe)
			{
				encryptSeed = EncryptionUtils.Base64Rfc4686Encode(Convert.ToBase64String(key));
			}
			else
			{
				encryptSeed = Convert.ToBase64String(key);
			}
		}
		else
		{
			encryptSeed = EncryptionUtils.ByteArrayToHexString(key);
		}

		// creating the byte array
		byte[] encryptedString = EncryptionUtils.EncryptStringToBytes(encryptPlainText, key);

		// writing the byte array
		WriteConfig(encryptedString);

		// getting the cipher text
		string result = "";
		if (trueBase64)
		{
			if (base64UrlSafe)
			{
				result = EncryptionUtils.Base64Rfc4686Encode(Convert.ToBase64String(encryptedString));
			}
			else
			{
				result = Convert.ToBase64String(encryptedString);
			}
		}
		else
		{
			result = EncryptionUtils.ByteArrayToHexString(encryptedString);
		}

		// displaying the cipher text
		encryptCipherText = result;
	}
	
	private void Decrypt()
	{
		// getting the key
		byte[] key = EncryptionUtils.VerifyKeyLength(encryptSeed, trueBase64, base64UrlSafe);

		// using the key to create the seed
		if (trueBase64)
		{
			if (base64UrlSafe)
			{
				encryptSeed = EncryptionUtils.Base64Rfc4686Encode(Convert.ToBase64String(key));
			}
			else
			{
				encryptSeed = Convert.ToBase64String(key);
			}
		}
		else
		{
			encryptSeed = EncryptionUtils.ByteArrayToHexString(key);
		}

		// getting the byte[]
		byte[] encryptedString = ReadConfig();

		// getting the cipher text
		string result = "";
		if (trueBase64)
		{
			if (base64UrlSafe)
			{
				result = EncryptionUtils.Base64Rfc4686Encode(Convert.ToBase64String(encryptedString));
			}
			else
			{
				result = Convert.ToBase64String(encryptedString);
			}
		}
		else
		{
			result = EncryptionUtils.ByteArrayToHexString(encryptedString);
		}

		// displaying the cipher text
		decryptCipherText = result;

		/*
		if (mTrueBase64)
		{
			if (mBase64UrlSafe)
			{
				encryptedString = Convert.FromBase64String(EncryptionUtils.base64Rfc4686Decode(mCipherText));
			}
			else
			{
				encryptedString = Convert.FromBase64String(mCipherText);
			}
		}
		else
		{
			encryptedString = EncryptionUtils.hexStringToByteArray_Rev4(mCipherText);
		}
		*/

		// getting the plain text
		string res = EncryptionUtils.DecryptStringFromBytes(encryptedString, key);
		if (base64UrlSafe)
		{
			res = EncryptionUtils.Base64Rfc4686Encode(res);
		}
		decryptPlainText = res;
	}

	/*
	private void EncryptSha1()
	{
		byte[] key = EncryptionUtils.VerifySha1KeyLength(encryptSeed);
		
		byte[] encryptedString = EncryptionUtils.EncryptSha1StringToBytes(plainText, key);

		if (base64UrlSafe)
		{
			encryptSeed = EncryptionUtils.Base64Rfc4686Encode(Convert.ToBase64String(key));
			result = EncryptionUtils.Base64Rfc4686Encode(Convert.ToBase64String(encryptedString));
		}
		else
		{
			encryptSeed = Convert.ToBase64String(key);
			result = Convert.ToBase64String(encryptedString);
		}
	}
	*/

	/*
	private void DecryptSha1()
	{
		byte[] key = EncryptionUtils.VerifySha1KeyLength(encryptSeed);

		byte[] decryptedString;

		if (base64UrlSafe)
		{
			decryptedString = EncryptionUtils.DecryptSha1StringToBytes(Convert.FromBase64String(EncryptionUtils.Base64Rfc4686Decode(cipherText)), key);
			encryptSeed = EncryptionUtils.Base64Rfc4686Encode(Convert.ToBase64String(key));
		}
		else
		{
			decryptedString = EncryptionUtils.DecryptSha1StringToBytes(Convert.FromBase64String(cipherText), key);
			encryptSeed = Convert.ToBase64String(key);
		}
		
		result = Encoding.UTF8.GetString(decryptedString);
	}
	*/

	private void WriteConfig(byte[] encrypted)
	{
		string configPath = Path.Combine(Path.Combine(Application.streamingAssetsPath, "ForceVision"), Config.DefaultConfigFile);
		File.WriteAllBytes(configPath, encrypted);
	}

	private byte[] ReadConfig()
	{
		string configPath = Path.Combine(Path.Combine(Application.streamingAssetsPath, "ForceVision"), Config.DefaultConfigFile);
		return File.ReadAllBytes(configPath);
	}
}