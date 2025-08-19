using UnityEngine;

public class ShutUp : MonoBehaviour
{
    [Tooltip("Max normalized time before resetting the state")]
    public float maxNormalizedTime = 1000f;

    void Update()
    {
        // Find all animators in the scene
        Animator[] animators = FindObjectsOfType<Animator>();

        foreach (Animator animator in animators)
        {
            // Skip disabled animators
            if (!animator.enabled) continue;

            int layerCount = animator.layerCount;
            for (int i = 0; i < layerCount; i++)
            {
                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(i);

                if (state.normalizedTime > maxNormalizedTime)
                {
                    // Reset the state to its start
                    animator.Play(state.fullPathHash, i, 0f);

                    // Optional: log which Animator is being reset
                    Debug.Log($"[ShutUp.cs] Reset Animator '{animator.gameObject.name}' layer {i} state {state.shortNameHash}");
                }
            }
        }
    }
}
