using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyInstanceAnimator : MonoBehaviour
{
    [Tooltip("Buat salinan AnimatorController per instance agar animasi bisa diubah per musuh")]
    public bool createInstanceController = true;

    Animator animator;
    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError($"{name}: Animator missing!");
            return;
        }

        // pastikan Animator di-enable
        if (!animator.enabled) animator.enabled = true;

        // set culling supaya animasi tetap berjalan meskipun offscreen
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        if (createInstanceController && animator.runtimeAnimatorController != null && !(animator.runtimeAnimatorController is AnimatorOverrideController))
        {
            var aoc = new AnimatorOverrideController(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = aoc;
            Debug.Log($"{name}: Created AnimatorOverrideController instance.");
        }
    }

    // helper: mengganti clip runtime jika perlu
    public void ReplaceClip(string originalName, AnimationClip newClip)
    {
        var aoc = animator.runtimeAnimatorController as AnimatorOverrideController;
        if (aoc == null)
        {
            Debug.LogWarning($"{name}: No AnimatorOverrideController available to replace clips.");
            return;
        }
        aoc[originalName] = newClip;
    }
}
