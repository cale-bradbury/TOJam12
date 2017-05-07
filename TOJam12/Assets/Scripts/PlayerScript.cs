using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour {
    
    public Inventory inventory;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

	}

    public void GrabGem(GemScript gem)
    {
        if (inventory.hasSpace)
        {
            gem.enabled = false;
            inventory.Add(gem);
        }
    }
}
