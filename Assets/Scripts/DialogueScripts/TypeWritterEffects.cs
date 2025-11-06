using UnityEngine;
using TMPro;
using System.Collections;

public class TypeWritterEffects : MonoBehaviour
{
    [SerializeField] private float typeWriteSpeeds = 50f; // Different speeds for typewriter effect 

    // changed: return IEnumerator so caller (DialogueUI) can StartCoroutine and control it
    public IEnumerator Run(string textToType, TMP_Text textLabel)
    {
        return TypeText(textToType, textLabel);
    }

    private IEnumerator TypeText(string textToType, TMP_Text textLabel)
    {
        textLabel.text = string.Empty;

        float t = 0;
        int charIndex = 0;

        while (charIndex < textToType.Length)
        {
            t += Time.deltaTime * typeWriteSpeeds;
            charIndex = Mathf.FloorToInt(t);
            charIndex = Mathf.Clamp(charIndex, 0, textToType.Length);
            textLabel.text = textToType.Substring(0, charIndex);
            yield return null;
        }
        textLabel.text = textToType;
    }
}
