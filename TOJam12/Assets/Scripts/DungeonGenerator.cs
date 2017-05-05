using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DungeonGenerator : MonoBehaviour
{

    public float unitSize = 1;
    public GameObject[] floorPrefab;
    public GameObject[] wallPrefab;
    public GameObject[] cornerPrefab;
    public GameObject[] uPrefab;
    List<GameObject> walls;
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
        int[,] map = ArrayUtils.Create2D(1, worldSize+2, worldSize+2);
        map = ArrayUtils.Merge2D(map, worldMap, 1, 1);
        map = GetLargestSection(map);
        DrawMap(map);
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
                    Debug.Log(num+"  "+tSize);
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

    void DrawMap(int[,] worldMap)
    {
        if (walls != null)
        {
            for (int i = transform.childCount-1; i >=0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        walls = new List<GameObject>();
        for (int i = 0; i < worldMap.GetLength(0); i++)
        {
            for (int j = 0; j < worldMap.GetLength(1); j++)
            {
                if (worldMap[i, j] == 0)
                    continue;
                GameObject g = Instantiate<GameObject>(floorPrefab[0]);
                g.transform.parent = transform;
                g.transform.localPosition = new Vector3(i * unitSize, 0, j * unitSize);
                walls.Add(g);
            }
        }
        Debug.Log(transform.childCount);
    }
}
