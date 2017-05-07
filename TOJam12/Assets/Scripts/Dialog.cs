using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Dialog : MonoBehaviour
{
    public List<DialogElement> dialogs = new List<DialogElement>();
    
    internal bool dialogMode = false;
    
    

    void OnMouseUp()
    {
        if (!dialogMode)
        {
            DialogDisplay.instance.StartDialog(this);
        }
    }
}