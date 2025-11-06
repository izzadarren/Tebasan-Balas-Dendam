    using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NUnit.Framework;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TextMeshProUGUI textLabel;

    public bool IsOpen {get; private set; }
    private ResponseHandler responseHandler;
    private TypeWritterEffects typewriterEffects;   
    


    void Start()
    {

        typewriterEffects = GetComponent<TypeWritterEffects>();
        responseHandler = GetComponent<ResponseHandler>();  
        CloseDialogueBox();
    }
    public void ShowDialogue(DialogueObject dialogueObject)
    {
        IsOpen = true;
        dialogueBox.SetActive(true);
        StartCoroutine(StepThroughDialogue(dialogueObject));
    }
    private IEnumerator StepThroughDialogue(DialogueObject dialogueObject)
    {
        foreach (string dialogue in dialogueObject.Dialogue)
        {
            yield return typewriterEffects.Run(dialogue, textLabel);
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
            
        }
        CloseDialogueBox();
    //  if (dialogueObject.HasResponses)
    //      {
    //      responseHandler.ShowResponses(dialogueObject.Responses);
    //     }
    //      else
    //      {
    //          CloseDialogueBox(); 
    //      }
     }
    private void CloseDialogueBox()
    {
        IsOpen = false;
        dialogueBox.SetActive(false);
        textLabel.text = string.Empty;
    }
}
