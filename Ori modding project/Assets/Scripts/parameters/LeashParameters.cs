using NaughtyAttributes;
using System;
using UnityEngine;

[ExecuteInEditMode]
public class LeashParameters : ElementParameter
{
    public hookType HookType;

    [Foldout("Editor Sprites")]
    public GameObject FlingLeashSprite;
    [Foldout("Editor Sprites")]
    public GameObject StickyLeashSprite;

    void Update()
    {
        SetString("HookType", Enum.GetName(typeof(hookType), HookType));

        FlingLeashSprite.SetActive(HookType == hookType.Fling);
        StickyLeashSprite.SetActive(HookType == hookType.Sticky);
    }

    public enum hookType
    {
        Sticky,
        Fling
    }
}
