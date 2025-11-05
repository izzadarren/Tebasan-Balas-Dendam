using UnityEngine;

public class DialogueActivator : MonoBehaviour, IInteractable
{
    [SerializeField] private DialogueObject dialogueObject;


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out PlayerController player))
        {
            // set langsung saat pemain masuk trigger
            player.Interactable = this;
            Debug.Log($"DialogueActivator: set Interactable on {player.gameObject.name}");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
         if (other.CompareTag("Player") && other.TryGetComponent(out PlayerController player))
        {
            if ((object)player.Interactable == this)
            {
                player.Interactable = null;
                Debug.Log($"DialogueActivator: cleared Interactable on {player.gameObject.name}");
            }
        }
    }
    
    public void Interact(PlayerController player)
    {
        player.DialogueUI.ShowDialogue(dialogueObject);
        Debug.Log("DialogueActivator: Interact called");
    }
    
}
