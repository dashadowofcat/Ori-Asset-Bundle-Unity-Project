using UnityEngine;

[ExecuteInEditMode]
public class LevelTitleParemeters : ElementParameter
{
    public string Title;

    void Update()
    {
        SetString("Title", Title);
    }
}
