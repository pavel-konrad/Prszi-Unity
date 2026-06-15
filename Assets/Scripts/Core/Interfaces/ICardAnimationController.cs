namespace Prsi.Core
{
    /// <summary>
    /// Interface pro ovládání animací karty
    /// </summary>
    public interface ICardAnimationController
    {
        void PlayAnimation(string triggerName);
        void SetBool(string parameterName, bool value);
        void SetFloat(string parameterName, float value);
        void SetInteger(string parameterName, int value);
        bool IsAnimationPlaying(string stateName);
        void OnCardStateChanged(ICardState cardState, CardLocation previousLocation);
    }
}

