using LocalProximityGame.Scripts;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/CoordinateManagerScriptableObject", order = 1)]
public class CoordinateManagerScriptableObject : ScriptableObject
{
    public PresetCoordinates[] presetCoordinates;
}
