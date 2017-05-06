using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DungeonGenerator : MonoBehaviour
{
    FloorMaster[] floors;
    public int floorCount = 6;
    public float unitSize = 1;
    public GameObject floorPrefab;
    public GameObject wallPrefab;
    public GameObject cornerPrefab;
    public GameObject uPrefab;
    public GameObject oPrefab;
    public GameObject stairsDownPrefab;
    public GameObject stairsUpPrefab;

    public int worldSize = 20;
    public int birthLimit = 4;
    public int deathLimit = 3;
    public int simulationSteps = 10;
    public float mapWallChance = .6f;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
            Generate();
    }

    [ContextMenu("Generate~")]
    public void Generate()
    {

        for (int i = transform.childCount-1; i>=0 ; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        floors = new FloorMaster[floorCount];
        for (int i = 0; i < floorCount; i++)
        {
            int[,] worldMap = GenerateMap();

            GameObject g = new GameObject();
            FloorMaster floor = g.AddComponent<FloorMaster>();
            floor.map = worldMap;
            floor.transform.parent = transform;
            DrawMap(floor);
            floors[i] = floor;
            if (i != 0)
            {
                Vector3 offset = floors[i - 1].stairsUp.transform.position - floor.stairsDown.transform.position;
                offset.y = -i*10;
                floor.transform.position = offset;
            }
        }
    }

    int[,] GenerateMap()
    {
        int[,] worldMap = new int[worldSize, worldSize];
        for (int i = 0; i < worldSize; i++)
        {
            for (int j = 0; j < worldSize; j++)
            {
                worldMap[i, j] = Random.value < mapWallChance ? 0 : 1;
            }
        }
        for (int i = 0; i < simulationSteps; i++)
        {
            worldMap = DoStep(worldMap);
        }
        int[,] map = ArrayUtils.Create2D(1, worldSize + 2, worldSize + 2);
        map = ArrayUtils.Merge2D(map, worldMap, 1, 1);
        map = GetLargestSection(map);
        return map;
    }

    public int[,] GetLargestSection(int[,] map)
    {
        int largestCount = 0;
        int num = 2;
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (map[x, y]==0)
                {
                    int tSize = 0;
                    ArrayUtils.Flood2D(ref map, x, y, 0, 3, ref tSize);
                    num++;
                    if (tSize > largestCount)
                    {
                        map = ArrayUtils.SwapValue2D(map, 2, 1);
                        map = ArrayUtils.SwapValue2D(map, 3, 2);
                        largestCount = tSize;
                    }else
                    {
                        map = ArrayUtils.SwapValue2D(map, 3, 1);
                    }
                }
            }
        }
        map = ArrayUtils.SwapValue2D(map, 2, 0);
        return map;
    }

    public int[,] DoStep(int[,] oldMap)
    {
        int[,] newMap = new int[oldMap.GetLength(0), oldMap.GetLength(1)];
        for (int x = 0; x < oldMap.GetLength(0); x++)
        {
            for (int y = 0; y < oldMap.GetLength(1); y++)
            {
                int nbs = CountAliveNeighbours(oldMap, x, y);
                if (oldMap[x, y] == 1)
                {
                    if (nbs < deathLimit)
                    {
                        newMap[x, y] = 0;
                    }
                    else
                    {
                        newMap[x, y] = 1;
                    }
                }
                else
                {
                    if (nbs > birthLimit)
                    {
                        newMap[x, y] = 1;
                    }
                    else
                    {
                        newMap[x, y] = 0;
                    }
                }
            }
        }
        return newMap;
    }

    public int CountAliveNeighbours(int[,] map, int x, int y)
    {
        int count = 0;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                int neighbour_x = x + i;
                int neighbour_y = y + j;
                if (i == 0 && j == 0)
                    continue;
                else if (neighbour_x < 0 || neighbour_y < 0 || neighbour_x >= map.GetLength(0) || neighbour_y >= map.GetLength(1))
                {
                    count = count + 1;
                }
                else if (map[neighbour_x, neighbour_y] == 1)
                {
                    count = count + 1;
                }
            }
        }
        return count;
    }

    void DrawMap(FloorMaster floor)
    {
        floor.walls = new List<GameObject>();
        for (int i = 1; i < floor.map.GetLength(0)-1; i++)
        {
            for (int j = 1; j < floor.map.GetLength(1)-1; j++)
            {
                if (floor.map[i, j] == 1)
                    continue;
                GameObject prefab = GetPrefab(floor.map, i,j);
                GameObject g = Instantiate<GameObject>(prefab);
                g.transform.parent = floor.transform;
                g.transform.localPosition = new Vector3(i * unitSize, 0, j * unitSize);
                floor.walls.Add(g);
            }
        }
        PlaceStairsUp(floor);
        PlaceStairsDown(floor);
    }

    GameObject GetPrefab(int[,] map, int i, int j)
    {
        if (map[i - 1, j] == 1 && map[i + 1, j] == 0 && map[i, j - 1] == 0 && map[i, j + 1] == 0)
        {
            return wallPrefab;
        }
        else if (map[i - 1, j] == 0 && map[i + 1, j] == 1 && map[i, j - 1] == 0 && map[i, j + 1] == 0)
        {
            return wallPrefab;
        }
        else if (map[i - 1, j] == 0 && map[i + 1, j] == 0 && map[i, j - 1] == 1 && map[i, j + 1] == 0)
        {
            return wallPrefab;
        }
        else if (map[i - 1, j] == 0 && map[i + 1, j] == 0 && map[i, j - 1] == 0 && map[i, j + 1] == 1)
        {
            return wallPrefab;
        }
        else if (map[i - 1, j] == 1 && map[i + 1, j] == 0 && map[i, j - 1] == 1 && map[i, j + 1] == 0)
        {
            return cornerPrefab;
        }
        else if (map[i - 1, j] == 1 && map[i + 1, j] == 0 && map[i, j - 1] == 0 && map[i, j + 1] == 1)
        {
            return cornerPrefab;
        }
        else if (map[i - 1, j] == 0 && map[i + 1, j] == 1 && map[i, j - 1] == 1 && map[i, j + 1] == 0)
        {
            return cornerPrefab;
        }
        else if (map[i - 1, j] == 0 && map[i + 1, j] == 1 && map[i, j - 1] == 0 && map[i, j + 1] == 1)
        {
            return cornerPrefab;
        }
        else if (map[i - 1, j] == 1 && map[i + 1, j] == 1 && map[i, j - 1] == 1 && map[i, j + 1] == 0)
        {
            return uPrefab;
        }
        else if (map[i - 1, j] == 1 && map[i + 1, j] == 1 && map[i, j - 1] == 0 && map[i, j + 1] == 1)
        {
            return uPrefab;
        }
        else if (map[i - 1, j] == 1 && map[i + 1, j] == 0 && map[i, j - 1] == 1 && map[i, j + 1] == 1)
        {
            return uPrefab;
        }
        else if (map[i - 1, j] == 0 && map[i + 1, j] == 1 && map[i, j - 1] == 1 && map[i, j + 1] == 1)
        {
            return uPrefab;
        }
        else if (map[i - 1, j] == 1 && map[i + 1, j] == 1 && map[i, j - 1] == 0 && map[i, j + 1] == 0)
        {
            return oPrefab;
        }
        else if (map[i - 1, j] == 0 && map[i + 1, j] == 0 && map[i, j - 1] == 1 && map[i, j + 1] == 1)
        {
            return oPrefab;
        }

        return floorPrefab;
    }

    void PlaceStairsUp(FloorMaster floor)
    {
        int x = 0, y = 0 ;
        while (floor.map[x, y] != 0)
        {
            x = Mathf.FloorToInt(Random.value * worldSize + 1);
            y = Mathf.FloorToInt(Random.value * worldSize + 1);
        }
        floor.stairsUp = Instantiate<GameObject>(stairsUpPrefab);
        floor.stairsUp.transform.parent = floor.transform;
        floor.stairsUp.transform.localPosition = new Vector3(x, 0, y);
    }

    void PlaceStairsDown(FloorMaster floor, int tries = 25)
    {
        Vector3 pos = floor.stairsUp.transform.localPosition;
        float dist = 0;
        for (int i = 0; i < tries; i++)
        {
            Vector3 v = new Vector3(Random.value * worldSize + 1, 0, Random.value * worldSize + 1);
            if(floor.map[Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.z)] == 0)
            {
                float d = Vector3.Distance(v, pos);
                if (d > dist)
                {
                    pos = v;
                    dist = d;
                }
            }else
            {
                tries--;
            }
        }
        floor.stairsDown = Instantiate<GameObject>(stairsDownPrefab);
        floor.stairsDown.transform.parent = floor.transform;
        pos.x = Mathf.FloorToInt(pos.x);
        pos.z = Mathf.FloorToInt(pos.z);
        floor.stairsDown.transform.localPosition = pos;
    }

}
