using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GemScript : MonoBehaviour {

    public enum GemType
    {
        Fluorite,
        Amethyst
    }
    public GemType gemType;
    public string description;
    
    void Start()
    {

    }

    void OnMouseEnter()
    {
        if (!enabled)
            return;
    }

    void OnMouseExit()
    {
        if (!enabled)
            return;
    }

    void OnMouseUp()
    {
        if(!enabled)
            return;
        DungeonMaster.instance.player.GrabGem(this);
    }
}
