using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour {

    public List<GameObject> inventory;
    public int space = 16;

    public bool hasSpace
    {
        get
        {
            return inventory.Count < space;
        }
    }


    public void Add(GameObject g)
    {
        inventory.Add(g);
        g.transform.parent = transform;
        g.transform.localPosition = Vector3.zero;
        g.transform.localEulerAngles = Vector3.zero;
        g.GetComponent<Collider>().enabled = false;
    }
}
