using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorMaster : MonoBehaviour {
    public Transform gemHolder;
    public Transform wallHolder;
    public StairsController stairsUp;
    public StairsController stairsDown;
    public List<GameObject> walls;
    public int floorNumber = 0;
    internal int[,] map;
    internal List<Vector2> empty;

    public void Enter()
    {
        DungeonMaster.instance.player.transform.position = stairsUp.transform.position + Vector3.up * .2f;
    }

    void OnEnable()
    {
        if (floorNumber == 0)
            stairsUp.gameObject.SetActive(false);
        else if(floorNumber ==3)
            stairsDown.gameObject.SetActive(false);
    }

    public void FindEmpties()
    {
        empty = new List<Vector2>();
        int lenX = map.GetLength(0);
        int lenY = map.GetLength(1);
        for (int j = 0; j < lenY; j++)
        {
            for (int i = 0; i < lenX; i++)
            {
                if (map[i, j] == 0)
                    empty.Add(new Vector2(i, j));
            }
        }
        Debug.Log(empty.Count + " empty spaces found");
    }

    public Vector3 FindEmpty(bool random = false, int set = 0)
    {
        if (empty.Count == 0) 
            return Vector3.one*(-100);
        int i = Mathf.FloorToInt(Random.value * empty.Count);
        Debug.Log(i);
        Vector2 v = empty[i];
        Debug.Log(v);
        if (set != 0)
        {
            map[Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y)] = set;
            empty.RemoveAt(i);
        }
        if (random)
            return new Vector3(v.x - .5f + Random.value, Random.value, v.y - .5f + Random.value);
        return new Vector3(v.x, 0, v.y);
    }

    public Vector3 FindEmptyNoWall(bool random = false, int set = 2)
    {
        int x = 0, y = 0;
        while (map[x, y] == 1 || map[x + 1, y] == 1 || map[x - 1, y] == 1 || map[x, y + 1] == 1 || map[x, y - 1] == 1 || map[x + 1, y+1] == 1 || map[x + 1, y-1] == 1 || map[x-1, y + 1] == 1 || map[x-1, y - 1] == 1)
        {
            x = Mathf.FloorToInt(Random.value * (map.GetLength(0)-2))+1;
            y = Mathf.FloorToInt(Random.value * (map.GetLength(1)-2))+1;
        }
        map[x, y] = set;
        if (random)
            return new Vector3(x - .5f + Random.value, Random.value, y - .5f + Random.value);
        return new Vector3(x, 0, y);
    }
}
