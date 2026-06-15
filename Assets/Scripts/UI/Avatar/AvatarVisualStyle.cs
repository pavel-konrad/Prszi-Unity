using UnityEngine;

[CreateAssetMenu(fileName = "AvatarVisualStyle", menuName = "Prsi/Avatar Visual Style")]
public class AvatarVisualStyle : ScriptableObject {
    [Header("Frame (okraj)")]
    public Color frameDefault = new Color(1,1,1,0.25f);
    public Color frameSelected = new Color(1f, 0.85f, 0.25f, 1f);  // zlatá
    public Color frameActive   = new Color(0.2f, 0.9f, 0.5f, 1f);   // zelená
    public Color frameDisabled = new Color(0.6f, 0.6f, 0.6f, 0.5f);

    [Header("Portrait")]
    public Color portraitNormal   = Color.white;
    public Color portraitDisabled = new Color(1,1,1,0.45f);

    [Header("Active glow (volitelný podklad)")]
    public Color activeGlow = new Color(0.2f, 1f, 0.6f, 0.25f);
}
