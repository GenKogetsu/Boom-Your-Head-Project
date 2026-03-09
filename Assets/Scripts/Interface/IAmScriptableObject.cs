using UnityEngine;

public interface IAmScriptableObject
{
    string ScriptName { get; }
    void ResetScripts();
}
