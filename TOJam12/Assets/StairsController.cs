using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StairsController : MonoBehaviour {

    public PlayerScript player;
    internal DungeonGenerator gen;
    public bool down = false;
    bool inElevator = false;
    

    // Update is called once per frame
    void OnTriggerEnter(Collider c)
    {
        if (c.gameObject != player.gameObject)
            return;
        inElevator = true;
    }
    void OnTriggerExit(Collider c)
    {
        if (c.gameObject != player.gameObject)
            return;
        inElevator = false;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            ChangeFloors();
    }

    public void ChangeFloors()
    {
        if (!inElevator)
            return;
        if (down)
        {
            gen.NextFloor();
        }
        else
        {
            gen.PrevFloor();
        }
    }
    
    
}
