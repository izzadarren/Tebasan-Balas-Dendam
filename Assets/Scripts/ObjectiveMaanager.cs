using System.Collections;
using UnityEngine;
using TMPro;

public class ObjectiveManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private string currentObjective = "Pergi ke lokasi rahasia!";
    private bool objectiveComplete = false;

    [Header("Auto hide settings")]
    [SerializeField] private float hideDelay = 2f;
    [SerializeField] private float fadeDuration = 0.5f;

    void Start()
    {
        UpdateObjectiveText();
    }

    void UpdateObjectiveText()
    {
        if (objectiveText == null) return;
        objectiveText.text = "Objective: " + currentObjective;
    }

    public void ReachDestination()
    {
        if (!objectiveComplete)
        {
            objectiveComplete = true;
            currentObjective = "Objective Complete!";
            UpdateObjectiveText();
            StartCoroutine(HideObjectiveRoutine());
        }
    }

    private IEnumerator HideObjectiveRoutine()
    {
        if (objectiveText == null) yield break;

        // tunggu sejenak sebelum mulai fade
        yield return new WaitForSeconds(hideDelay);

        // coba pakai CanvasGroup di parent untuk fade; kalau tidak ada tambah sementara
        Transform target = objectiveText.transform;
        CanvasGroup cg = target.GetComponentInParent<CanvasGroup>();
        GameObject rootToDisable = cg != null ? cg.gameObject : target.gameObject;

        if (cg == null)
        {
            cg = rootToDisable.AddComponent<CanvasGroup>();
        }

        float start = cg.alpha;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, 0f, Mathf.Clamp01(elapsed / fadeDuration));
            yield return null;
        }
        cg.alpha = 0f;

        // sembunyikan object UI (non-destruktif)
        rootToDisable.SetActive(false);
    }
}
