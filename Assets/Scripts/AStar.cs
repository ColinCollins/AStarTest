using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GridType {
	Floor = 0,
	Wall = 1,
	Target = 2,
	Init = 3,
	NULL
}

public enum Direction {
	Up = 0,
	Down = 1,
	Left = 2,
	Right = 3,
	UpRight = 4,
	UpLeft = 5,
	DownLeft = 6,
	DownRight = 7,
	Center = 8
}

public static class AStar {

	#region private struct

	private class stepObj {
		public F f;
		public int x = 0;
		public int y = 0;
		public int index = 0;
		public stepObj parent = null;
		public Direction dir = Direction.Center;
	}

	struct F {
		public float G;
		public float H;
		// dir change cast
		public float D;
	}

	#endregion

	private const int s_cast = 10;
	// bias 斜线, diagonal 对角线
	private const int d_cast = 14;
	// 方向改变损耗
	private const int dir_case = 0;

	private static int width;
	private static int height;

	private static int startIndex;
	private static int destIndex;
	private static stepObj destStepObj;

	public static void Init(int gridsWidth, int gridsHeight) {
		width = gridsWidth;
		height = gridsHeight;
		startIndex = 0;
		destIndex = 0;
	}

	// 对外接口
	public static List<int> CalculatePath(int[] mapData, int start, int dest) {
	
		// 记录基本信息
		startIndex = start;
		destIndex = dest;

		List<stepObj> openList = new List<stepObj>();
		List<stepObj> closeList = new List<stepObj>();

		// 记录目的地的 pos
		destStepObj = new stepObj();
		destStepObj.x = destIndex % width;
		destStepObj.y = destIndex / height;

		stepObj curStep = new stepObj();
		curStep.x = startIndex % width;
		curStep.y = startIndex / height;
		curStep.index = startIndex;

		closeList.Add(curStep);

		calculateNext(mapData, curStep, openList, closeList);

		// back array

		List<int> path = generatePath(startIndex, closeList);
		
		return path;
	}

	private static List<int> generatePath (int startIndex, List<stepObj> closeList) {
		stepObj obj = closeList[closeList.Count - 1];
		List<int> path = new List<int>();
		while (obj.parent != null) {
			path.Add(obj.index);
			obj = obj.parent;
		}
		path.Reverse();

		return path;
	}

	private static void calculateNext(int[] mapData, stepObj curStep, List<stepObj> open, List<stepObj> close) {
		stepObj[] f = new stepObj[8];

		f[0] = calculateF(mapData, curStep, Direction.Right, close);
		f[1] = calculateF(mapData, curStep, Direction.Up, close);
		f[2] = calculateF(mapData, curStep, Direction.Left, close);
		f[3] = calculateF(mapData, curStep, Direction.Down, close);

		// 若正方向的通路被封死，那么将不允许斜挎
		f[4] = (f[0] == null && f[1] == null) ? null : calculateF(mapData, curStep, Direction.UpRight, close);
		f[5] = (f[2] == null && f[1] == null) ? null : calculateF(mapData, curStep, Direction.UpLeft, close);
		f[6] = (f[2] == null && f[3] == null) ? null : calculateF(mapData, curStep, Direction.DownLeft, close);
		f[7] = (f[0] == null && f[3] == null) ? null : calculateF(mapData, curStep, Direction.DownRight, close);

		stepObj nextStep = null;
		for (int i = 0; i < f.Length; i++) {
			// get the min f
			stepObj value = f[i];
			if (value != null) {
				value.parent = curStep;

				if (nextStep == null) {
					nextStep = value;
				}

				if (value.f.D < nextStep.f.D) {
					nextStep = value;
				}

				open.Add(value);
			}
		}

		open.Sort((obj1, obj2) => {
			return obj1.f.D.CompareTo(obj2.f.D);
		});

		if (nextStep == null || nextStep.f.D < open[0].f.D) {
			// Debug.LogError("死路");
			nextStep = open[0];
			open.Remove(open[0]);
		}


		int index = isInClose(open, nextStep.index);
		if (index != -1) {
			nextStep = open[index];
			open.Remove(nextStep);
		}

		close.Add(nextStep);

		if (nextStep.index != destIndex)
			calculateNext(mapData, nextStep, open, close);
	}

	private static stepObj calculateF(int[] mapData, stepObj cueStep, Direction nextDir, List<stepObj> close) {
		stepObj obj = new stepObj();
		int index = checkPos(cueStep, nextDir);

		if (index < 0 || isInClose(close, index) != -1 || mapData[index] == (int)GridType.Wall) return null;

		obj.x = index % width;
		obj.y = index / height;
		obj.index = index;
		obj.dir = nextDir;

		obj.f.H = calculateH(obj);
		obj.f.G = calculateG(cueStep, obj);
		obj.f.D = obj.f.H + obj.f.G;

		// Debug.Log(obj.f.D);

		return obj;
	}

	private static float calculateH(stepObj obj) {
		int dx = Mathf.Abs(destStepObj.x - obj.x);
		int dy = Mathf.Abs(destStepObj.y - obj.y);

		return Mathf.Abs(dx - dy) * s_cast + d_cast * (dx > dy ? dy : dx);
	}

	private static float calculateG(stepObj curStep, stepObj nextStep) {
		return curStep.f.G + (Mathf.Abs(curStep.x - nextStep.x) == Mathf.Abs(curStep.y - nextStep.y) ? d_cast : s_cast); 
	}

	private static int checkPos(stepObj curStep, Direction nextDir) {

		int index = curStep.y * width + curStep.x;

		switch (nextDir) {
			case Direction.Up:
				if (isMovable(curStep.x, curStep.y - 1)) return index - width;
				break;
			case Direction.Down:
				if (isMovable(curStep.x, curStep.y + 1)) return index + width;
				break;
			case Direction.Right:
				if (isMovable(curStep.x + 1, curStep.y)) return index + 1;
				break;
			case Direction.Left:
				if (isMovable(curStep.x - 1, curStep.y)) return index - 1;
				break;
			case Direction.UpRight:
				if (isMovable(curStep.x + 1, curStep.y - 1)) return index + 1 - width;
				break;
			case Direction.UpLeft:
				if (isMovable(curStep.x - 1, curStep.y - 1)) return index - 1 - width;
				break;
			case Direction.DownRight:
				if (isMovable(curStep.x + 1, curStep.y + 1)) return index + 1 + width;
				break;
			case Direction.DownLeft:
				if (isMovable(curStep.x - 1, curStep.y + 1)) return index - 1 + width;
				break;
			default:
				break;
		}

		return -1;
	}

	private static int isInClose(List<stepObj> list, int index) {
		for (int i = 0; i < list.Count; i++) {
			if (list[i].index == index) return i;
		}

		return -1;
	}
	private static bool isMovable(int x, int y) {
		return !(x < 0 || x >= width || y < 0 || y >= height);
	}

}
