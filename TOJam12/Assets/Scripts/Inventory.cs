﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour {

    public List<GameObject> inventory;
    public int space = 16;
    public float animateInTime = .5f;

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
        g.GetComponent<Collider>().enabled = false;
        StartCoroutine(AnimateToInventory(g, animateInTime));
    }

    IEnumerator AnimateToInventory(GameObject g, float time)
    {
        float step = 1f / time;
        float t = 1;
        Vector3 startPos = g.transform.localPosition;
        while (t > 0)
        {
            t -= step*Time.deltaTime;
            t = Mathf.Max(t, 0);
            g.transform.localScale = Vector3.one * t;
            g.transform.localPosition = Vector3.Lerp(Vector3.zero, startPos, Mathf.Pow(t, 2));
            yield return new WaitForEndOfFrame();
        }
    }
}