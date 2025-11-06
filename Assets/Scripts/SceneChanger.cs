using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public float changeTime;
    [Tooltip("Build index of the scene to load (set in Build Settings)")]
    public int sceneIndex;

    private void Update()
    {
        changeTime -= Time.deltaTime;
        if (changeTime <= 0f)
        {
            if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(sceneIndex);
            }
            else
            {
                Debug.LogError($"SceneChanger: sceneIndex {sceneIndex} is out of range. Check Build Settings.");
            }
        }
    }
}
