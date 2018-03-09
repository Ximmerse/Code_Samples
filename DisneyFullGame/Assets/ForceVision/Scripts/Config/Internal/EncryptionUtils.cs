using UnityEngine;

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Disney.ForceVision.Internal
{
	public class EncryptionUtils
	{
		//needs to be blocksize/8, default blocksize is 128 bytes
		private static byte[] defaultIV = { 5, 236, 85, 198, 121, 95, 151, 44, 15, 56, 162, 32, 106, 129, 227, 105 };
		private static byte[] defaultKeyGen = { 58, 252, 112, 146, 243, 132, 130, 28, 195, 152 };

		public static byte[] VerifyKeyLength(string seed, bool encryptedUsingBase64 = true, bool isBase64UrlSafe = false)
		{
			if (seed.Length == 64)
			{
				return HexStringToByteArray_Rev4(seed);
			}
			else if (encryptedUsingBase64 && seed.Length == 44)
			{
				if (isBase64UrlSafe)
				{
					return Convert.FromBase64String(Base64Rfc4686Decode(seed));
				}
				else
				{
					return Convert.FromBase64String(seed);
				}
			}
			else 
			{
				Rfc2898DeriveBytes k1 = new Rfc2898DeriveBytes(seed, defaultKeyGen );
				return k1.GetBytes( 32 );  //32 bytes equal 256 bits
			}
		}

		public static string ByteArrayToHexString(byte[] ba)
		{
			string hex = BitConverter.ToString(ba);
			return hex.Replace("-","");
		}

		//http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa/26304129#26304129
		public static byte[] HexStringToByteArray_Rev4(string input)
		{
			var outputLength = input.Length / 2;
			var output = new byte[outputLength];
			using (var sr = new StringReader(input))
			{
				for (var i = 0; i < outputLength; i++)
					output[i] = Convert.ToByte(new string(new char[2] { (char)sr.Read(), (char)sr.Read() }), 16);
			}
			return output;
		}
		
		public static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV = null)
		{
			// Check arguments. 
			if (plainText == null || plainText.Length <= 0)
				throw new ArgumentNullException("plainText");
			if (Key == null || Key.Length <= 0)
				throw new ArgumentNullException("Key");
	//		if (IV == null || IV.Length <= 0)
	//			throw new ArgumentNullException("IV");
			byte[] encrypted;
			// Create an RijndaelManaged object 
			// with the specified key and IV. 
			using (RijndaelManaged rijAlg = new RijndaelManaged())
			{
				rijAlg.Key = Key;
				if (IV == null || IV.Length <= 0)
				{
					rijAlg.IV = defaultIV;
				}
				else
				{
					rijAlg.IV = IV;
				}
				// Create a decrytor to perform the stream transform.
				ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);
				
				// Create the streams used for encryption. 
				using (MemoryStream msEncrypt = new MemoryStream())
				{
					using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
					{
						using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
						{
							//Write all data to the stream.
							swEncrypt.Write(plainText);
						}
						encrypted = msEncrypt.ToArray();
					}
				}
			}
			
			// Return the encrypted bytes from the memory stream. 
			return encrypted;
			
		}

		public static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV = null)
		{
			// Check arguments. 
			if (cipherText == null || cipherText.Length <= 0)
				throw new ArgumentNullException("cipherText");
			if (Key == null || Key.Length <= 0)
				throw new ArgumentNullException("Key");
	//		if (IV == null || IV.Length <= 0)
	//			throw new ArgumentNullException("IV");
			
			// Declare the string used to hold 
			// the decrypted text. 
			string plaintext = null;
			
			// Create an RijndaelManaged object 
			// with the specified key and IV. 
			using (RijndaelManaged rijAlg = new RijndaelManaged())
			{
				rijAlg.Key = Key;
				if (IV == null || IV.Length <= 0)
				{
					rijAlg.IV = defaultIV;
				}
				else
				{
					rijAlg.IV = IV;
				}
				
				// Create a decrytor to perform the stream transform.
				ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);
				
				// Create the streams used for decryption. 
				using (MemoryStream msDecrypt = new MemoryStream(cipherText))
				{
					using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
					{
						using (StreamReader srDecrypt = new StreamReader(csDecrypt))
						{
							// Read the decrypted bytes from the decrypting stream 
							// and place them in a string.
							plaintext = srDecrypt.ReadToEnd();
						}
					}
				}
			}
			return plaintext;
		}

		public static byte[] VerifySha1KeyLength(string seed)
		{
			if (seed.Length == 24)
			{
				return Convert.FromBase64String(seed);
			}
			else 
			{
				byte[] bytes = ASCIIEncoding.UTF8.GetBytes (seed);
				
				SHA1Managed shhash = new SHA1Managed ();
				byte[] hash = shhash.ComputeHash (bytes);
				
				byte[] resultBytes = new byte[16]; //truncate to 128 bits = 16 bytes
				Array.Copy(hash, resultBytes, resultBytes.Length);
				
				return resultBytes;
			}
		}
		
		public static byte[] EncryptSha1StringToBytes(string aValue, byte[] aKey)
		{
			byte[] content = ASCIIEncoding.UTF8.GetBytes(aValue);
			
			using (AesManaged aes = new AesManaged())
			{
				aes.Key = aKey;
				aes.Mode = CipherMode.ECB;
				aes.Padding = PaddingMode.PKCS7;
				ICryptoTransform crypt = aes.CreateEncryptor();
				
				byte[] encrypted = crypt.TransformFinalBlock(content, 0, content.Length);
				return encrypted;
			}
		}
		
		public static byte[] DecryptSha1StringToBytes(byte[] aCipherText, byte[] aKey)
		{
			using (AesManaged aes = new AesManaged())
			{
				aes.Key = aKey;
				aes.Mode = CipherMode.ECB;
				aes.Padding = PaddingMode.PKCS7;
				ICryptoTransform crypt = aes.CreateDecryptor();

				byte[] decrypted = crypt.TransformFinalBlock(aCipherText, 0, aCipherText.Length);
				return decrypted;
			}
		}

		public static string Base64Rfc4686Encode(string aValue)
		{
			if (string.IsNullOrEmpty(aValue))
			{
				return aValue;
			}
			
			return aValue.Replace("+", "-").Replace("/", "_");
		}
		
		public static string Base64Rfc4686Decode(string aValue)
		{
			if (string.IsNullOrEmpty(aValue))
			{
				return aValue;
			}
			
			return aValue.Replace("-", "+").Replace("_", "/");
		}
	}
}