using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorMaster : MonoBehaviour {
    
    public GameObject stairsUp;
    public GameObject stairsDown;
    public List<GameObject> walls;
    internal int[,] map;

    public void Enter()
    {
        DungeonMaster.instance.player.transform.position = stairsUp.transform.position + Vector3.up * .2f;
    }
}
