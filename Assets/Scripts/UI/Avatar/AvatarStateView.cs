using UnityEngine;
using UnityEngine.UI;

public class AvatarStateView : MonoBehaviour
{
    public Image portrait;
    public Image frame;
    public Image activeGlow;
    public Animator frameAnimator; // na AvatarBg

    public Color frameDefault = new(1,1,1,0.25f);
    public Color frameSelected = new(1f,0.85f,0.25f,1f);
    public Color frameActive = new(0.2f,0.9f,0.5f,1f);
    public Color frameDisabled = new(0.6f,0.6f,0.6f,0.5f);
    public Color portraitDisabled = new(1,1,1,0.45f);
    public Color activeGlowColor = new(0.2f,1f,0.6f,0.25f);

    bool selected, active, disabled;

    public void SetSelected(bool on){
        selected = on;
        if (on) 
        {
            frameAnimator?.SetTrigger("Select");
            // Sound plays via Animation Event
        }
        Apply();
    }
    public void SetActive(bool on){
        active = on;
        
        if (frameAnimator != null)
        {
            frameAnimator.SetBool("IsActive", on);
            
            var currentState = frameAnimator.GetCurrentAnimatorStateInfo(0);
        }
        else
        {
            Debug.LogWarning($"[AvatarStateView] frameAnimator is null for {gameObject.name}");
        }
        
        Apply();
    }
    public void SetDisabled(bool on){
        disabled = on;
        Apply();
    }

    void Apply(){
        if (frame){
            var c = frameDefault;
            if (disabled) c = frameDisabled;
            else if (active) c = frameActive;
            else if (selected) c = frameSelected;
            frame.color = c;
        }
        if (portrait) portrait.color = disabled ? portraitDisabled : Color.white;
        if (activeGlow){
            activeGlow.enabled = active && !disabled;
            if (activeGlow.enabled) activeGlow.color = activeGlowColor;
        }
    }
}
