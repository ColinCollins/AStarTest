using System;
using System.IO;
using UnityEngine;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

public class IO {

	#region 
	public class MapObj {
		public List<int[]> mapList;

		public MapObj() {
			mapList = new List<int[]>();
		}
	}

	#endregion

		private static String path =
	#if UNITY_WINDWO || UNITY_EDITOR
		 Application.dataPath;
	#else
		Application.persistentDataPath;
	#endif

	public static void SaveDataInLocal(int[] mapData) {
		MapObj obj = new MapObj();
		obj.mapList.Add(mapData);
		string res = JsonConvert.SerializeObject(obj);

		WriteTextToPath(String.Concat(path, "/map.json"), res, true);
	}

	public static string ReadTextFromPath(string path) {
		FileInfo info = new FileInfo(path);
		string result = "";
		if (!info.Exists) {
			Debug.LogWarning("IOSystem can't load specified file.");
		}
		else {
			FileStream fs = info.OpenRead();
			byte[] b = new byte[1024];
			while (fs.Read(b, 0, b.Length) > 0) {
				string cs = new UTF8Encoding(true).GetString(b);
				result += cs;
				Array.Clear(b, 0, b.Length);
			}
			fs.Close();
		}

		return result;
	}

	public static void WriteTextToPath(string path, string data, bool isCreate) {
		FileInfo info = new FileInfo(path);
		if (!info.Exists) {
			if (isCreate) {
				info.Create().Dispose();
			}
			else {
				Debug.Log("IOSystem WriteTextToPath can't find specified file.");
				return;
			}
		}

		FileStream fs = info.OpenWrite();
		// clear file text
		fs.SetLength(0);
		byte[] d = new UTF8Encoding(true).GetBytes(data);
		fs.Write(d, 0, d.Length);

		fs.Close();
	}

	public static bool DeleteSpecifiedFile(string path) {
		FileInfo info = new FileInfo(path);
		info.Delete();
		return !info.Exists;
	}

	public static bool isFileExists(string path) {
		FileInfo info = new FileInfo(path);
		return info.Exists;
	}
}
