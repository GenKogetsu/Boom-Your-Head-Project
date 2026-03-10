using TMPro;

[System.Serializable]
public class PlayerUiSlot
{
    [Header("Identity")]
    public Character CharacterType; // ตั้งค่าใน Inspector ว่าช่องนี้เป็นของใคร

    [Header("UI Elements")]
    public GameObject OutImage; // ใช้ GameObject เลยจะได้สั่ง SetActive ง่ายๆ
    public TextMeshProUGUI HpDisplay;
    public TextMeshProUGUI BombDisplay;
    public TextMeshProUGUI SpeedDisplay;
    public TextMeshProUGUI RangeDisplay;
}