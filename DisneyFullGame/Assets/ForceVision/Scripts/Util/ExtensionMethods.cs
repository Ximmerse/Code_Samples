using System.Collections.Generic;
using UnityEngine;

namespace Disney.ForceVision
{
	/// <summary>
	/// this is Hunter Gough's handy toolbox of extension methods for various data types
	/// </summary>
	public static class IntExtensions
	{
		/// <summary>Returns the number or m, whichever is greater</summary>
		public static int Min(this int n, int m)
		{
			return (n > m) ? n : m;
		}

		/// <summary>Returns the number or m, whichever is lesser</summary>
		public static int Max(this int n, int m)
		{
			return (n < m) ? n : m;
		}

		/// <summary>
		/// return n, clamped between min and max
		/// </summary>
		/// <param name="min">Minimum value.</param>
		/// <param name="max">Maximum value.</param>
		public static int MinMax(this int n, int min, int max)
		{
			return (n < min) ? min : (n > max) ? max : n;
		}
	}

	public static class FloatExtensions
	{
		/// <summary>Returns the square of the number</summary>
		public static float Squared(this float n)
		{
			return n * n;
		}

		/// <summary>Returns the number or m, whichever is greater</summary>
		public static float Min(this float n, float m)
		{
			return (n > m) ? n : m;
		}

		/// <summary>Returns the number or m, whichever is lesser</summary>
		public static float Max(this float n, float m)
		{
			return (n < m) ? n : m;
		}

		/// <summary>
		/// return n, clamped between min and max
		/// </summary>
		/// <param name="min">Minimum value.</param>
		/// <param name="max">Maximum value.</param>
		public static float MinMax(this float n, float min, float max)
		{
			return (n < min) ? min : (n > max) ? max : n;
		}
	}

	public static class VectorExtensions
	{
		/// <summary>Returns the Vector3 with Y and Z swapped</summary>
		public static Vector3 xzy(this Vector3 v)
		{
			return new Vector3(v.x, v.z, v.y);
		}

		/// <summary>Returns a Vector3 with Y used for the Z</summary>
		public static Vector3 xzy(this Vector2 v)
		{
			return new Vector3(v.x, 0, v.y);
		}

		/// <summary>Returns a Vector2 with Y replaced by Z</summary>
		public static Vector2 xz(this Vector3 v)
		{
			return new Vector2(v.x, v.z);
		}

		/// <summary>
		/// Turns a cartesian Vector2 into a polar Vector2 where x = angle and y = distance
		/// </summary>
		public static Vector2 Cart2Pol(this Vector2 cart)
		{
			float angle = -Mathf.Atan2(cart.x, cart.y) * Mathf.Rad2Deg;
			return new Vector2(angle, cart.magnitude);
		}

		/// <summary>
		/// Turns a polar Vector2 where x = angle and y = distance into a cartesian Vector2
		/// </summary>
		public static Vector2 Pol2Cart(this Vector2 pol)
		{
			return new Vector2((float)Mathf.Sin(pol.x * Mathf.Deg2Rad), (float)-Mathf.Cos(pol.x * Mathf.Deg2Rad)) * pol.y;
		}
	}

	public static class StringExtensions
	{
		/// <summary>Shifts the first "length" characters off the beginning of the string and returns them</summary>
		public static string Shift(this string s, int length)
		{
			string sub;

			if (s.Length > length)
			{
				sub = s.Substring(0, length);
				s = s.Substring(length);
			}
			else
			{
				sub = s;
				s = "";
			}

			return sub;
		}

		/// <summary>Parses the string into an int array separated by "separator"</summary>
		public static int[] ToIntArray(this string s, char separator)
		{
			s = s.Trim();
			List<int> n = new List<int>();
			string[] sa = s.Split(separator);
			for (int i = 0; i < sa.Length; i++)
			{
				if (!string.IsNullOrEmpty(sa[i]))
				{
					n.Add(System.Convert.ToInt32(sa[i]));
				}
			}

			return n.ToArray();
		}

		/// <summary>Parses the string into a float array separated by "separator"</summary>
		public static float[] ToFloatArray(this string s, char separator)
		{
			s = s.Trim();
			List<float> n = new List<float>();
			string[] sa = s.Split(separator);
			for (int i = 0; i < sa.Length; i++)
			{
				if (!string.IsNullOrEmpty(sa[i]))
				{
					n.Add(float.Parse(sa[i], System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
				}
			}

			return n.ToArray();
		}

		/// <summary>Insert a breakString at every n characters of string s</summary>
		public static string BreakEveryN(this string s, int n, string breakString = " ")
		{
			if (n < 1)
			{
				return s;
			}
		
			for (int i = n; i < s.Length; i += n)
			{
				s = s.Insert(i, breakString);
				i += breakString.Length;
			}
		
			return s;
		}
	}

	public static class ListExtensions
	{
		/// <summary>Swaps the element at index1 with the element at index2</summary>
		public static void Swap<T>(this List<T> source, int index1, int index2)
		{
			if (index1 < 0 || index1 > source.Count)
				return;
			else if (index2 < 0 || index2 > source.Count)
				return;
			else if (index1 == index2)
				return;

			T temp = source[index1];
			source[index1] = source[index2];
			source[index2] = temp;
		}

		/// <summary>Shuffles all of the elements in the list</summary>
		public static void Shuffle<T>(this List<T> source)
		{
			for (int i = 0; i < source.Count; i++)
			{
				int r = UnityEngine.Random.Range(0, source.Count);
				T temp = source[i];
				source[i] = source[r];
				source[r] = temp;
			}
		}

		/// <summary>Returns the last element in the list</summary>
		public static T Last<T>(this List<T> source)
		{
			if (source.Count < 1)
			{
				return default(T);
			}

			return source[source.Count - 1];
		}

		/// <summary>Returns a random element from the list</summary>
		public static T Random<T>(this List<T> source)
		{
			if (source.Count < 1)
			{
				return default(T);
			}
			else if (source.Count == 1)
			{
				return source[0];
			}
			else
			{
				return source[UnityEngine.Random.Range(0, source.Count)];
			}
		}

		/// <summary>Shifts the first element off the list and returns it. Similar to Queue.Dequeue().
		/// If you are only Adding and Shifting to a List, consider using a Queue instead.
		/// </summary>
		public static T Shift<T>(this List<T> source)
		{
			if (source.Count < 1)
			{
				return default(T);
			}

			T temp = source[0];
			source.RemoveAt(0);
			return temp;
		}

		/// <summary>Pops the last element off the list and returns it.
		/// If you are only Adding and Popping to a List, consider using a Stack instead.
		/// </summary>
		public static T Pop<T>(this List<T> source)
		{
			if (source.Count < 1)
			{
				return default(T);
			}

			T temp = source[source.Count - 1];
			source.RemoveAt(source.Count - 1);
			return temp;
		}

		/// <summary>Adds item to the beginning of the list, moving all other elements down</summary>
		public static void Unshift<T>(this List<T> source, T item)
		{
			source.Insert(0, item);
		}
	}

	public static class CameraExtensions
	{
		/// <summary>Moves the vanishing point of the camera</summary>
		public static void SetVanishingPoint(this Camera cam, Vector2 offset)
		{
			Matrix4x4 m = cam.projectionMatrix;
			float w = 2 * cam.nearClipPlane / m.m00;
			float h = 2 * cam.nearClipPlane / m.m11;

			float left = -w / 2 - offset.x;
			float right = left + w;
			float bottom = -h / 2 - offset.y;
			float top = bottom + h;

			cam.projectionMatrix = PerspectiveOffCenter(left, right, bottom, top, cam.nearClipPlane, cam.farClipPlane);
		}

		/// <summary>Moves the vanishing point of the camera</summary>
		public static void SetVanishingPoint(this Camera cam, float x, float y = 0)
		{
			SetVanishingPoint(cam, new Vector2(x, y));
		}

		// used by SetVanishingPoint()
		static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
		{
			float x = 2.0F * near / (right - left);
			float y = 2.0F * near / (top - bottom);
			float a = (right + left) / (right - left);
			float b = (top + bottom) / (top - bottom);
			float c = -(far + near) / (far - near);
			float d = -(2.0F * far * near) / (far - near);
			float e = -1.0F;
			Matrix4x4 m = new Matrix4x4();
			m[0,0] = x;
			m[0,1] = 0;
			m[0,2] = a;
			m[0,3] = 0;
			m[1,0] = 0;
			m[1,1] = y;
			m[1,2] = b;
			m[1,3] = 0;
			m[2,0] = 0;
			m[2,1] = 0;
			m[2,2] = c;
			m[2,3] = d;
			m[3,0] = 0;
			m[3,1] = 0;
			m[3,2] = e;
			m[3,3] = 0;
			return m;
		}
	}

	public static class GameObjectExtensions
	{
		/// <summary>Sets the layer on the GameObject and all of its children</summary>
		public static void SetLayerRecursively(this GameObject go, int layerNumber)
		{
			go.layer = layerNumber;
                      
			for (int i = 0; i < go.transform.childCount; i++)
			{
				go.transform.GetChild(i).gameObject.SetLayerRecursively(layerNumber);
			}
		}

		/// <summary>
		/// Gets a component of the specified type, adding it if there isn't already one
		/// Also useful for checking to see if a component is present and adding it if it isn't, in one step
		/// </summary>
		/// <returns>The component.</returns>
		public static T GetOrAddComponent<T>(this GameObject go) where T : Component
		{
			return go.GetComponent<T>() ?? go.AddComponent<T>();
		}
	}

	public static class ComponentExtensions
	{
		/// <summary>Instantiates a component with the same parent, position, rotation, and scale as the original</summary>
		public static Component CopyInPlace(this Component source)
		{
			Component temp = Object.Instantiate(source) as Component;
			temp.transform.SetParent(source.transform.parent);
			temp.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
			temp.transform.localScale = source.transform.localScale;

			return temp;
		}
	}

	public static class TransformExtensions
	{
		/// <summary>Snaps the transform's local position and rotation to zero and local scale to one</summary>
		public static void SnapToZero(this Transform source)
		{
			source.localPosition = Vector3.zero;
			source.localScale = Vector3.one;
			source.localRotation = Quaternion.identity;
		}

		/// <summary>
		/// Rotates the transform to look at the target along the XZ plane, keeping "pitch" rotation level
		/// </summary>
		public static void XZLookAt(this Transform source, Vector3 target)
		{
			Vector3 v = new Vector3(target.x, source.position.y, target.z);
			source.LookAt(v);
		}

		/// <summary>
		/// Rotates the transform to look at the target along the XZ plane, keeping "pitch" rotation level
		/// </summary>
		public static void XZLookAt(this Transform source, Transform target)
		{
			source.XZLookAt(target.position);
		}

		/// <summary>
		/// Returns the transform's full path in the scene.
		/// </summary>
		/// <returns>The transform's full path in the scene.</returns>
		public static string PathInScene(this Transform source)
		{
			Transform transform = source;
			string path = source.name;
			while (transform.parent)
			{
				transform = transform.parent;
				path = transform.name + "/" + path;
			}
			return path;
		}
	}
}