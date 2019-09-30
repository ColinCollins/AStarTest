using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerate : MonoBehaviour {

	#region color
	private Color yellowPath = new Color(0.97f, 0.86f, 0.44f);
	private Color blueWall = new Color(0.051f, 0.76f, 0.91f);
	private Color redTarget = new Color(0.95f, 0.58f, 0.54f);
	private Color greenInit = new Color(0.28f, 0.79f, 0.69f);
	#endregion

	public Texture floor;

	private Vector3[] p = new Vector3[8];
	private Vector3[] vertices = null;

	private Vector2[] t = new Vector2[4];
	private Vector2[] uvs = null;

	private Vector3[] normals = null;

	private int[] triangles = null;

	private const float length = 1;
	private const float width = 1;
	private const float height = 1;

	private int gridsWidth = 20;
	private int gridsHeight = 20;

	private class GridObj {
		public GameObject _cube;
		public int type;
		public int x;
		public int y;
	}

	private static List<GridObj> gridObjs = null;
	private List<int> path = null;
	private int drawCount = 0;

	void Start() {
		initData();

		gridObjs = new List<GridObj>();

		int[] mapData = GenerateRandomData();

		IO.SaveDataInLocal(mapData);

		int startIndex = 0;
		int destIndex = 0;
		for (int i = 0; i < mapData.Length; i++) {
			if (mapData[i] == (int)GridType.Init)
				startIndex = i;
			if (mapData[i] == (int)GridType.Target)
				destIndex = i;
		}

		GenerateMap(mapData);


		AStar.Init(gridsWidth, gridsHeight);
		path = AStar.CalculatePath(mapData, startIndex, destIndex);
	}

	private int[] GenerateRandomData() {

		bool isInit = false;
		bool isTarget = false;

		int maxCount = gridsWidth * gridsHeight;
		int[] mapData = new int[maxCount];

		Distributor staff = new Distributor();
		staff.init();

		for (int i = 0; i < maxCount; i++) {
			float random = UnityEngine.Random.value;
			if (i < maxCount * 3 / 4) random -= 0.25f;

			GridType type = staff.getResult(random);

			// 只用这个方式生成的起点和重点太近了
			if (type == GridType.Init) {
				if (isInit)
					type = GridType.Floor;
				else
					isInit = true;
			}

			if (type == GridType.Target) {
				if (isTarget)
					type = GridType.Floor;
				else
					isTarget = true;
			}

			mapData[i] = Convert.ToInt32(type);
		}

		// recalculate
		if (!isInit || !isTarget) {
			mapData = GenerateRandomData();
		}

		return mapData;
	}

	#region Responsibility 

	class Distributor {

		public int count = 0;
		public GridType result = GridType.NULL;

		private Counterman header;
		private int CountermanCount = 4;

		public void init() {
			float unit = 1f / CountermanCount;

			header = new Counterman(this, 0, unit, GridType.Floor);
			Counterman man1 = header;

			for (int i = 1; i < CountermanCount; i++) {
				Counterman newMan = new Counterman(this, unit * i, unit * (i + 1), (GridType)i);
				man1.setNextColleague(newMan);
				man1 = newMan;
			}

			man1.setNextColleague(header);
		}

		public GridType getResult(float random) {
			count = 0;
			header.deal(random);
			return result;
		}

		public void handInResult(GridType type) {
			// isCallStop == true;
			count = CountermanCount;

			result = type;
		}

		public bool isCallStop() {
			return count >= CountermanCount;
		}
	}

	class Counterman {

		private float min;
		private float max;
		private GridType type;

		private Distributor manager;
		private Counterman nextColleague;

		public Counterman(Distributor manager, float min, float max, GridType type) {
			this.min = min;
			this.max = max;
			this.manager = manager;
			this.type = type;
		}

		public void setNextColleague(Counterman next) {
			this.nextColleague = next;
		}

		public virtual void deal(float num) {
			if (num < min || num > max) {
				goToNext(num);
				return;
			}

			manager.handInResult(type);
		}

		public void goToNext(float num) {
			if (manager.isCallStop()) return;
			manager.count++;
			nextColleague.deal(num);
		}
	}

	#endregion


	private void Update() {
		if (Input.GetMouseButtonDown(0) && drawCount < path.Count)
			gridObjs[path[drawCount++]]._cube.GetComponent<MeshRenderer>().material.color = yellowPath;
	}


	public void GenerateMap(int[] mapData) {
		for (int i = 0; i < mapData.Length; i++) {
			int x = Convert.ToInt32(i % gridsWidth);
			int y = Convert.ToInt32(i / gridsWidth);

			GridObj gridObj = new GridObj();
			gridObj.type = mapData[i];
			gridObj.x = x;
			gridObj.y = y;

			GenerateCube(gridObj);
			gridObjs.Add(gridObj);
		}
	}


	private void initData() {
		// vertices data
		p[0] = new Vector3(-length * .5f, -width * .5f, height * .5f);
		p[1] = new Vector3(length * .5f, -width * .5f, height * .5f);
		p[2] = new Vector3(length * .5f, -width * .5f, -height * .5f);
		p[3] = new Vector3(-length * .5f, -width * .5f, -height * .5f);

		p[4] = new Vector3(-length * .5f, width * .5f, height * .5f);
		p[5] = new Vector3(length * .5f, width * .5f, height * .5f);
		p[6] = new Vector3(length * .5f, width * .5f, -height * .5f);
		p[7] = new Vector3(-length * .5f, width * .5f, -height * .5f);


		vertices = new Vector3[]
		{
			p[0], p[1], p[2], p[3], // Bottom
	        p[7], p[4], p[0], p[3], // Left
	        p[4], p[5], p[1], p[0], // Front
	        p[6], p[7], p[3], p[2], // Back
	        p[5], p[6], p[2], p[1], // Right
	        p[7], p[6], p[5], p[4]  // Top
		};


		// uvs data
		t[0] = new Vector2(0f, 0f);
		t[1] = new Vector2(1f, 0f);
		t[2] = new Vector2(0f, 1f);
		t[3] = new Vector2(1f, 1f);

		uvs = new Vector2[]
		{
			t[3], t[2], t[0], t[1], // Bottom
	        t[3], t[2], t[0], t[1], // Left
	        t[3], t[2], t[0], t[1], // Front
	        t[3], t[2], t[0], t[1], // Back	        
	        t[3], t[2], t[0], t[1], // Right 
	        t[3], t[2], t[0], t[1]  // Top
		};

		// normals data
		Vector3 up = Vector3.up;
		Vector3 down = Vector3.down;
		Vector3 forward = Vector3.forward;
		Vector3 back = Vector3.back;
		Vector3 left = Vector3.left;
		Vector3 right = Vector3.right;

		normals = new Vector3[]
		{
			down, down, down, down,             // Bottom
	        left, left, left, left,             // Left
	        forward, forward, forward, forward,	// Front
	        back, back, back, back,             // Back
	        right, right, right, right,         // Right
	        up, up, up, up	                    // Top
        };


		// triangles
		triangles = new int[] {
			3, 1, 0,        3, 2, 1,        // Bottom	
	        7, 5, 4,        7, 6, 5,        // Left
	        11, 9, 8,       11, 10, 9,      // Front
	        15, 13, 12,     15, 14, 13,     // Back
	        19, 17, 16,     19, 18, 17,	    // Right
	        23, 21, 20,     23, 22, 21,	    // Top
        };
	}

	private void GenerateCube(GridObj gridObj) {
		GameObject obj = new GameObject("Cube");
		MeshRenderer meshRender = obj.AddComponent<MeshRenderer>();
		MeshFilter filter = obj.AddComponent<MeshFilter>();
		filter.name = "cube";

		Mesh mesh = new Mesh();
		mesh.Clear();

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.normals = normals;
		mesh.uv = uvs;

		filter.mesh = mesh;

		Material cubeMaterial = new Material(Shader.Find("Custom / Floor"));
		cubeMaterial.color = transColor(gridObj.type);
		cubeMaterial.SetTexture("_MainTex", floor);
		meshRender.material = cubeMaterial;

		// Translate
		obj.transform.position = transPos(gridObj.x, gridObj.y);
		gridObj._cube = obj;
	}


	private Color transColor(int type) {
		switch ((GridType)type) {
			case GridType.Wall: return blueWall;
			case GridType.Target: return redTarget;
			case GridType.Init: return greenInit;
			case GridType.Floor:
			default: return new Color(1.0f, 1.0f, 1.0f, 1.0f);
		}
	}

	private Vector3 transPos(int x, int y) {
		float posX = x * width - (gridsWidth / 2f * width);
		float posZ = y * height - (gridsHeight / 2f * height);
		float posY = 0.5f;

		return new Vector3(posX, posY, -posZ);
	}
}
