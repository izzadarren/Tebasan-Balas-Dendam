using UnityEngine;
using UnityEngine.SceneManagement;
public class LevelMove : MonoBehaviour
{
    public int sceneBuildIndex;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        print("Loading Scene...");
        Debug.Log("Collided with: " + collision.name + " | Tag: " + collision.tag);


        if(collision.CompareTag("Player"))
        {
            print("Switching Scene...");
            SceneManager.LoadScene(sceneBuildIndex, LoadSceneMode.Single);
        }
    }
}
