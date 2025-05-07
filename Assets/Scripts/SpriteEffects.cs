using DG.Tweening;
using UnityEngine;

public class SpriteEffects : MonoBehaviour {
    [SerializeField]
    private float shineDuration = 1.5f; // Duration for the shine effect
    [SerializeField]
    private float zoomDuration = 1f; // Speed of the shine effect
    [SerializeField]
    private float glowDuration = 1f; // Speed of the glow effect
    private Material material;

    private void Awake() {
        material = GetComponent<Renderer>().material;
    }

    private void OnEnable() {
        material.SetFloat("_ShineLocation", 0f);
        material.SetFloat("_ZoomUvAmount", 1.2f);
        material.SetFloat("_Glow", 0f);

        Sequence dtSequence = DOTween.Sequence();

        dtSequence
            .Append(DOTween.To(
                () => material.GetFloat("_ZoomUvAmount"),
                x => material.SetFloat("_ZoomUvAmount", x),
                1, // target value
                zoomDuration
                )
                .SetEase(Ease.OutBack)
            )
            .Append(DOTween.To(
                () => material.GetFloat("_Glow"),
                x => material.SetFloat("_Glow", x),
                1f, // target value
                glowDuration
                )
            )
            .Join(DOTween.To(
                () => material.GetFloat("_ShineLocation"),
                x => material.SetFloat("_ShineLocation", x),
                1f, // target value
                shineDuration
                )
            );
    }
}
