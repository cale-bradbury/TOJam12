using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogDisplay : MonoBehaviour {


    public static DialogDisplay instance;
    public Inventory inventory;
    public Animator animator;
    public Dictionary<string, int> dialogVars;
    public Camera playerCamera;
    public Text text;
    public float lineHeight = 25;
    string[] newLine = new string[] { "\n" };

    Dialog currentDialog;
    int index = 0;
    Quaternion lookTarget;
    Quaternion lookStart;
    List<MonoBehaviour> scriptsToLock = new List<MonoBehaviour>();
    bool inDialog = false;
    bool canSkip = true;
    int endedThisFrame = 0;

    public static void AddInventory(Inventory.Items item)
    {
        if (!instance.inventory.inventory.ContainsKey(item))
            instance.inventory.inventory[item] = 0;
        instance.inventory.inventory[item]++;
    }

    public void Awake()
    {
        instance = this;
        text.text = "";
        dialogVars = new Dictionary<string, int>();
        MouseLook[] ml = FindObjectsOfType<MouseLook>();
        foreach (MouseLook m in ml)
            scriptsToLock.Add(m);
        scriptsToLock.Add(FindObjectOfType<FirstPersonDrifter>());
        scriptsToLock.Add(FindObjectOfType<MouseLook>());
        scriptsToLock.Add(FindObjectOfType<HeadBob>());
    }

    public void StartDialog(Dialog log)
    {
        if (endedThisFrame>0)//replace with out animation
            return;
        animator.SetBool("Show", true);
        log.dialogMode = true;
        inDialog = true;
        currentDialog = log;
        index = 0;
        foreach (MonoBehaviour m in scriptsToLock)
            m.enabled = false;
        NextInput();
    }

    public void EndDialog()
    {
        animator.SetBool("Show", false);
        endedThisFrame = 30;
        inDialog = false;
        currentDialog.dialogMode = false;
        foreach (MonoBehaviour m in scriptsToLock)
            m.enabled = true;
        text.text = "";
    }

    void NextInput()
    {
        if (index == currentDialog.dialogs.Count)
        {
            EndDialog();
            return;
        }
        bool fireNext = false;
        DialogElement d = currentDialog.dialogs[index];
        if (d.type == DialogElement.Type.Dialog)
        {
            canSkip = true;
            text.text = d.string1;
            Vector2 v = text.rectTransform.sizeDelta;
            v.y = lineHeight * text.text.Split(newLine, System.StringSplitOptions.RemoveEmptyEntries).Length;
            text.rectTransform.sizeDelta = v;
        }
        else if (d.type == DialogElement.Type.Event)
        {
            Messenger.Broadcast(d.string1);
            fireNext = true;
        }
        else if (d.type == DialogElement.Type.LookAt)
        {
            canSkip = false;
            lookStart = playerCamera.transform.rotation;
            playerCamera.transform.LookAt(d.transform1);
            lookTarget = playerCamera.transform.rotation;
            playerCamera.transform.rotation = lookStart;
            StartCoroutine(Utils.AnimationCoroutine(AnimationCurve.EaseInOut(0, 0, 1, 1), d.float1, LookAt, FinishLookAt));
        }
        else if (d.type == DialogElement.Type.Sound)
        {
            d.audio.Play();
            fireNext = true;
        }
        else if (d.type == DialogElement.Type.End)
        {
            EndDialog();
            return;
        }
        else if (d.type == DialogElement.Type.JumpPoint)
        {
            fireNext = true;
        }
        else if (d.type == DialogElement.Type.JumpIfVar)
        {
            if (!dialogVars.ContainsKey(d.string2))
            {
                dialogVars[d.string2] = 0;
            }
            Debug.Log(d.condition +"   "+d.string2 +"   "+d.float1+"   "+dialogVars[d.string2] +"  "+d.string1);
            if (d.condition == DialogElement.Condition.Equal && dialogVars[d.string2] == d.float1)
                Jump(d.string1);
            else if (d.condition == DialogElement.Condition.Greater && dialogVars[d.string2] > d.float1)
                Jump(d.string1);
            else if (d.condition == DialogElement.Condition.Less && dialogVars[d.string2] < d.float1)
                Jump(d.string1);
            else if (d.condition == DialogElement.Condition.NotEqual && dialogVars[d.string2] != d.float1)
                Jump(d.string1);

            fireNext = true;
        }

        else if (d.type == DialogElement.Type.JumpIfInventory)
        {
            if (!inventory.inventory.ContainsKey(d.item))
            {
                inventory.inventory[d.item] = 0;
            }
            if (d.condition == DialogElement.Condition.Equal && inventory.inventory[d.item] == d.float1)
                Jump(d.string1);
            else if (d.condition == DialogElement.Condition.Greater && inventory.inventory[d.item] > d.float1)
                Jump(d.string1);
            else if (d.condition == DialogElement.Condition.Less && inventory.inventory[d.item] < d.float1)
                Jump(d.string1);
            else if (d.condition == DialogElement.Condition.NotEqual && inventory.inventory[d.item] != d.float1)
                Jump(d.string1);

            fireNext = true;
        }
        else if (d.type == DialogElement.Type.IncreaseVar)
        {
            if (!dialogVars.ContainsKey(d.string1))
                dialogVars.Add(d.string1, 0);
            dialogVars[d.string1] += Mathf.RoundToInt(d.float1);
            Debug.Log(dialogVars[d.string1]);
            fireNext = true;
        }
        else if (d.type == DialogElement.Type.ChangeInventory)
        {
            if (!inventory.inventory.ContainsKey(d.item))
                inventory.inventory.Add(d.item, 0);
            inventory.inventory[d.item] += Mathf.RoundToInt(d.float1);
            fireNext = true;
        }
        index++;
        if (fireNext)
            NextInput();
    }

    void Jump(string tag)
    {
        for (int i = 0; i < currentDialog.dialogs.Count; i++)
        {
            DialogElement d = currentDialog.dialogs[i];
            if (d.type == DialogElement.Type.JumpPoint && d.string2 == tag) 
            {
                index = i;
                return;
            }
        }
    }

    void Update()
    {
        endedThisFrame-- ;
        if (inDialog && canSkip)
        {
            if (Input.GetMouseButtonDown(0))
            {
                NextInput();
            }
        }
    }

    void FinishLookAt()
    {
        canSkip = true;
        NextInput();
    }

    void LookAt(float f)
    {
        playerCamera.transform.rotation = Quaternion.Lerp(lookStart, lookTarget, f);
    }
}

[System.Serializable]
public class DialogElement
{
    public enum Type
    {
        Dialog,
        Event,
        LookAt,
        Sound,
        End,
        JumpPoint,
        JumpIfVar,
        IncreaseVar,
        JumpIfInventory,
        ChangeInventory
    }
    public enum Condition
    {
        Greater,
        Less,
        Equal,
        NotEqual
    }
    public Type type;

    public string string1;
    public string string2;
    public float float1;
    public AudioSource audio;
    public Condition condition;
    public Inventory.Items item;
    public Transform transform1;

    public DialogElement Copy()
    {
        DialogElement d = new DialogElement();
        d.string1 = this.string1;
        d.string2 = this.string2;
        d.float1 = this.float1;
        d.audio = this.audio;
        d.condition = this.condition;
        d.item = this.item;
        d.transform1 = this.transform1;
        d.type = this.type;
        return d;
    }
}
