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

    public void Enter()
    {
        DungeonMaster.instance.player.transform.position = stairsUp.transform.position + Vector3.up * .2f;
    }

    public Vector3 FindEmpty(bool random = false)
    {
        int x = 0, y = 0;
        while (map[x, y] == 1)
        {
            x = Mathf.FloorToInt(Random.value * map.GetLength(0));
            y = Mathf.FloorToInt(Random.value * map.GetLength(1));
        }
        if(random)
            return new Vector3(x - .5f + Random.value, Random.value, y - .5f + Random.value);
        return new Vector3(x, 0, y );
    }
}
