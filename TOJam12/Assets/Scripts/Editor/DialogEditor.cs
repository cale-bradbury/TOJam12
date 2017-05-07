using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;
using System;

[CustomEditor(typeof(Dialog))]
public class DialogEditor : Editor
{
    private ReorderableList list;
    Dialog dialog;

    Color dialogColor = new Color(.75f, 1, .75f);
    Color eventColor = new Color(.75f, .75f, 1);
    Color lookColor = new Color(1, .75f, .75f);
    Color audioColor = new Color(1, 1, .75f);
    Color endColor = new Color(.3f, .3f, .3f);
    Color jumpColor = new Color(.5f, 1f, .5f);
    Color jumpPointColor = new Color(.0f, 1f, .0f);
    Color varColor = new Color(.5f, .5f, 1f);

    private void OnEnable()
    {
        dialog = target as Dialog;
        list = new ReorderableList(dialog.dialogs, typeof(DialogElement), true, true, true, true);
        list.elementHeight = EditorGUIUtility.singleLineHeight * 2 + 4;
        list.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "dankilog supersystem v4.20 tm (c)");
        };

        list.onAddDropdownCallback = (Rect rect, ReorderableList rl) => {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Dialog"), false, OnMenuClick, CreateElement(DialogElement.Type.Dialog));

            menu.AddItem(new GUIContent("Event"), false, OnMenuClick, CreateElement(DialogElement.Type.Event));

            menu.AddItem(new GUIContent("Look At"), false, OnMenuClick, CreateElement(DialogElement.Type.LookAt));
            menu.AddItem(new GUIContent("Play Audio"), false, OnMenuClick, CreateElement(DialogElement.Type.Sound));
            menu.AddItem(new GUIContent("End"), false, OnMenuClick, CreateElement(DialogElement.Type.End));
            menu.AddItem(new GUIContent("Jump Point"), false, OnMenuClick, CreateElement(DialogElement.Type.JumpPoint));
            menu.AddItem(new GUIContent("Jump If"), false, OnMenuClick, CreateElement(DialogElement.Type.JumpIfVar));
            menu.AddItem(new GUIContent("Increment"), false, OnMenuClick, CreateElement(DialogElement.Type.IncreaseVar));
            menu.AddItem(new GUIContent("Jump If Item"), false, OnMenuClick, CreateElement(DialogElement.Type.JumpIfInventory));
            menu.AddItem(new GUIContent("ChangeInventory"), false, OnMenuClick, CreateElement(DialogElement.Type.ChangeInventory));

            menu.ShowAsContext();
        };

        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            DialogElement e = dialog.dialogs[index] as DialogElement;
            DialogElement.Type t = e.type;
            rect.y += 2;
            if (t == DialogElement.Type.Dialog)
            {
                ChangeColor(rect, dialogColor);
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), "dialog");
                rect.x += 60;
                rect.height = EditorGUIUtility.singleLineHeight * 2;
                e.string1 = EditorGUI.TextArea(new Rect(rect.x, rect.y, rect.width - 60, rect.height),
                    e.string1);
            }
            else if (t == DialogElement.Type.Event)
            {
                ChangeColor(rect, eventColor);
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), "event");
                rect.x += 60;
                e.string1 = EditorGUI.TextField(new Rect(rect.x, rect.y, rect.width - 60, EditorGUIUtility.singleLineHeight),
                    e.string1);
            }
            else if (t == DialogElement.Type.LookAt)
            {
                ChangeColor(rect, lookColor);
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), "look at");
                rect.x += 60;
                e.transform1 = EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width - 60, EditorGUIUtility.singleLineHeight),
                    "object", e.transform1, typeof(Transform), true) as Transform;
                rect.y += EditorGUIUtility.singleLineHeight;
                e.float1 = EditorGUI.FloatField(new Rect(rect.x, rect.y, rect.width - 60, EditorGUIUtility.singleLineHeight),
                    "time", e.float1);
            }
            else if (t == DialogElement.Type.Sound)
            {
                ChangeColor(rect, audioColor);
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), "play sound");
                rect.x += 60;
                e.audio = EditorGUI.ObjectField(new Rect(rect.x, rect.y, rect.width - 60, EditorGUIUtility.singleLineHeight),
                    "audio source", e.audio, typeof(AudioSource), true) as AudioSource;

            }
            else if (t == DialogElement.Type.End)
            {
                ChangeColor(rect, endColor);
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 80, EditorGUIUtility.singleLineHeight), "end dialog");
            }
            else if (t == DialogElement.Type.JumpIfVar)
            {
                ChangeColor(rect, jumpColor);
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), "jump if");
                rect.x += 60;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), "jumpTag");
                e.string1 = EditorGUI.TextField(new Rect(rect.x + 60, rect.y, rect.width - 120, EditorGUIUtility.singleLineHeight), e.string1);
                rect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), "varName");
                e.string2 = EditorGUI.TextField(new Rect(rect.x + 60, rect.y, rect.width - 120, EditorGUIUtility.singleLineHeight), e.string2);
                rect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), "condition");
                e.condition = (DialogElement.Condition)EditorGUI.EnumPopup(new Rect(rect.x + 60, rect.y, rect.width - 120, EditorGUIUtility.singleLineHeight), e.condition);
                rect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), "value");
                e.float1 = EditorGUI.FloatField(new Rect(rect.x + 60, rect.y, rect.width - 120, EditorGUIUtility.singleLineHeight), e.float1);
            }

            else if (t == DialogElement.Type.JumpIfInventory)
            {
                ChangeColor(rect, jumpColor);
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), "jump if");
                rect.x += 60;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), "jumpTag");
                e.string1 = EditorGUI.TextField(new Rect(rect.x + 60, rect.y, rect.width - 120, EditorGUIUtility.singleLineHeight), e.string1);
                rect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), "item");
                e.item = (Inventory.Items)EditorGUI.EnumPopup(new Rect(rect.x + 60, rect.y, rect.width - 120, EditorGUIUtility.singleLineHeight), e.item);
                rect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), "condition");
                e.condition = (DialogElement.Condition)EditorGUI.EnumPopup(new Rect(rect.x + 60, rect.y, rect.width - 120, EditorGUIUtility.singleLineHeight), e.condition);
                rect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), "value");
                e.float1 = EditorGUI.FloatField(new Rect(rect.x + 60, rect.y, rect.width - 120, EditorGUIUtility.singleLineHeight), e.float1);
            }
            else if (t == DialogElement.Type.JumpPoint)
            {
                ChangeColor(rect, jumpPointColor);
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), "jump tag");
                rect.x += 60;
                e.string2 = EditorGUI.TextField(new Rect(rect.x + 60, rect.y, rect.width - 120, EditorGUIUtility.singleLineHeight), e.string2);
            }
            else if (t == DialogElement.Type.IncreaseVar)
            {
                ChangeColor(rect, varColor);
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 120, EditorGUIUtility.singleLineHeight), "increment var");
                rect.x += 60;
                e.string1 = EditorGUI.TextField(new Rect(rect.x + 60, rect.y, rect.width - 120, EditorGUIUtility.singleLineHeight), e.string1);
                rect.y += EditorGUIUtility.singleLineHeight;
                e.float1 = EditorGUI.FloatField(new Rect(rect.x - 60, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "amount", e.float1);
            }
            else if (t == DialogElement.Type.ChangeInventory)
            {
                ChangeColor(rect, varColor);
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 120, EditorGUIUtility.singleLineHeight), "item");
                rect.x += 60;
                e.item = (Inventory.Items)EditorGUI.EnumPopup(new Rect(rect.x + 60, rect.y, rect.width - 120, EditorGUIUtility.singleLineHeight), e.item);
                rect.y += EditorGUIUtility.singleLineHeight;
                e.float1 = EditorGUI.FloatField(new Rect(rect.x - 60, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "amount", e.float1);
            }
        };

        list.elementHeightCallback = (index) =>
        {
            DialogElement e = dialog.dialogs[index] as DialogElement;
            DialogElement.Type t = e.type;
            float f = 2;
            if (t == DialogElement.Type.JumpIfVar || t == DialogElement.Type.JumpIfInventory)
                f = 4;
            return EditorGUIUtility.singleLineHeight * f+8;
        };
    }

    void ChangeColor(Rect rect, Color c)
    {
        EditorGUI.DrawRect(new Rect(rect.x, rect.y - 1, rect.width, rect.height - 4), c);
    }

    private DialogElement CreateElement(DialogElement.Type t)
    {
        DialogElement e = new DialogElement();
        e.type = t;
        return e;
    }

    private void OnMenuClick(object target)
    {
        dialog.dialogs.Add(target as DialogElement);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        list.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(dialog);
    }
}