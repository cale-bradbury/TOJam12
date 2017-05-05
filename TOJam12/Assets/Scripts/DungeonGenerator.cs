using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DungeonGenerator : MonoBehaviour {

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
        for (int i = 0; i < worldSize; i++){
            for (int j  = 0; j< worldSize; j++){
                worldMap[i, j] = Random.value < mapWallChance?0:1;
            }
        }
        for (int i = 0; i < simulationSteps; i++)
        {
            worldMap = DoStep(worldMap);
        }
        DrawMap(worldMap);
    }

    public int[,] DoStep (int[,] oldMap)
    {
        int[,] newMap = new int[worldSize, worldSize];
        for (int x = 0; x < worldSize; x++)
        {
            for (int y = 0; y < worldSize; y++)
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
                else if (map[neighbour_x, neighbour_y]==1)
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
            for (int i = 0; i < walls.Count; i++)
            {
                DestroyImmediate(walls[i]);
            }
        }

        walls = new List<GameObject>();
        for (int i = 0; i < worldSize; i++)
        {
            for (int j = 0; j < worldSize; j++)
            {
                if (worldMap[i, j] == 0)
                    continue;
                GameObject g = Instantiate<GameObject>(floorPrefab[0]);
                g.transform.parent = transform;
                g.transform.localPosition = new Vector3(i * unitSize, 0, j * unitSize);
                walls.Add(g);
            }
        }
    }
}
