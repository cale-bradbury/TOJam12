using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonMaster : MonoBehaviour {
    public static DungeonMaster instance;
    public PlayerScript player;
    public DungeonGenerator generator;
    
	void Awake () {
        instance = this;
        generator.enabled = true;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
