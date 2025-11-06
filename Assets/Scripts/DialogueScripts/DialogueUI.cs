using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TextMeshProUGUI textLabel;

    public bool IsOpen { get; private set; }

    private ResponseHandler responseHandler;
    private TypeWritterEffects typewriterEffects;

    // runtime state
    private Coroutine typingCoroutine;
    private bool isTyping;
    private string currentLineFullText;
    private bool advanceRequested;
    private PlayerController currentPlayer;
    private DialogueObject currentDialogue;

    void Start()
    {
        typewriterEffects = GetComponent<TypeWritterEffects>();
        responseHandler = GetComponent<ResponseHandler>();
        CloseDialogueBox();
    }

    // Overloads: can pass player so DialogueUI can unlock movement when closed.
    public void ShowDialogue(DialogueObject dialogueObject)
    {
        ShowDialogue(dialogueObject, null);
    }

    public void ShowDialogue(DialogueObject dialogueObject, PlayerController player)
    {
        if (dialogueObject == null) return;

        currentDialogue = dialogueObject;
        currentPlayer = player;

        // lock player if provided (safe-guard)
        currentPlayer?.LockMovement();

        IsOpen = true;
        dialogueBox.SetActive(true);
        StartCoroutine(StepThroughDialogue(dialogueObject));
    }

    private IEnumerator StepThroughDialogue(DialogueObject dialogueObject)
    {
        // iterate through lines
        foreach (string dialogue in dialogueObject.Dialogue)
        {
            // start typing
            currentLineFullText = dialogue;
            advanceRequested = false;
            // run typewriter (wrapped so we can stop it)
            yield return StartCoroutine(RunTypewriter(currentLineFullText));

            // wait for player to press advance (space or left click) or external request
            advanceRequested = false;
            yield return new WaitUntil(() => advanceRequested || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0));
        }

        // if there are responses, show them, otherwise close
        if (dialogueObject.HasResponses && responseHandler != null)
        {
            responseHandler.ShowResponses(dialogueObject.Responses);
        }
        else
        {
            CloseDialogueBox();
        }
    }

    private IEnumerator RunTypewriter(string fullText)
    {
        isTyping = true;

        if (typewriterEffects != null)
        {
            // start the typewriter coroutine and keep the Coroutine handle so Skip() can StopCoroutine(typingCoroutine)
            typingCoroutine = StartCoroutine(typewriterEffects.Run(fullText, textLabel));
            // wait until that coroutine finishes
            yield return typingCoroutine;
            typingCoroutine = null;
        }
        else
        {
            // fallback: immediately display full text if no typewriter component
            textLabel.text = fullText;
            yield return null;
        }

        isTyping = false;
    }

    // Called by PlayerController when pressing Skip (e.g. Space) while dialogue open,
    // or by ResponseHandler when a response is chosen.
    public void Skip()
    {
        if (!IsOpen) return;

        // hentikan coroutine mengetik bila berjalan
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // langsung tutup kotak dialog — "skip" berarti menutup dialog sekarang
        CloseDialogueBox();
    }

    // This method can be called by ResponseHandler when a response is picked.
    // Keep signature public so ResponseHandler can call it.
    public void OnResponsePicked(Response response)
    {
        // default behavior: close dialogue (you can extend to route response actions)
        CloseDialogueBox();
        Debug.Log($"Response picked: {(response != null ? response.ResponseText : "null")}");
    }

    private void CloseDialogueBox()
    {
        IsOpen = false;
        dialogueBox.SetActive(false);
        textLabel.text = string.Empty;

        // unlock player if we locked it when opening
        currentPlayer?.UnlockMovement();
        currentPlayer = null;
        currentDialogue = null;

        // clear any typing state
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        isTyping = false;
        advanceRequested = false;
    }
}
