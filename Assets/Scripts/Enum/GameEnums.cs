namespace BombGame.EnumSpace;

/// <summary> (TH) : ตัวละครในเกม | (EN) : Characters in the game </summary>
public enum Character { None, Ryuwen, Baboon, Edigan, Terbi, All }

/// <summary> (TH) : ประเภทของตัวละคร | (EN) : Character types </summary>
public enum Charactertype { Player, Bot }

/// <summary> (TH) : ประเภทของการกระทำ | (EN) : Action types </summary>
public enum ActionType { PlaceBomb, Move }

/// <summary> (TH) : ประเภทของแผ่นกระเบื้อง | (EN) : Tile types </summary>
public enum TileType { Ground, Wall, Destructible, Prop, Empty }

/// <summary> (TH) : สถานะของระเบิด | (EN) : Bomb states </summary>
public enum BombState { Moving, NonCritical, Critical }

/// <summary> (TH) : ส่วนของระเบิดที่ถูกระเบิด | (EN) : Parts of the bomb explosion </summary>
public enum BombPart { Start, Middle, End }