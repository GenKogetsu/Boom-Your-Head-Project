using TMPro;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "PlayerUiSlot", menuName = "Scriptable Objects/PlayerUiSlot")]
public class PlayerUiSlot : ScriptableObject
{
    public Image OutImage;

    public TextMeshProUGUI HpDisplay;
    public TextMeshProUGUI BombDisplay;
    public TextMeshProUGUI SpeedDisplay;
    public TextMeshProUGUI RangeDisplay;


}
