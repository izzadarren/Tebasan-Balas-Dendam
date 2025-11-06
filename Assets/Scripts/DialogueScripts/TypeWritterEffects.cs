using UnityEngine;
using TMPro;
using System.Collections;
using System;
using UnityEngine.UI;
public class TypeWritterEffects : MonoBehaviour
{
    [SerializeField] private float typeWriteSpeeds = 50f; // Different speeds for typewriter effect 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Coroutine Run(string textToType, TMP_Text textLabel)
    {
        return StartCoroutine(TypeText(textToType, textLabel));
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
