using UnityEngine;

public class FootstepSound : MonoBehaviour
{
[SerializeField] float speed = 10f;
AudioSource audioSource;
Rigidbody2D rb2D;
float x;

void Start()
{
    audioSource = GetComponent<AudioSource>();
    rb2D = GetComponent<Rigidbody2D>();
}

void Update()
{
    x = Input.GetAxis("Horizontal") * speed;
    rb2D.linearVelocity = new Vector2(x, rb2D.linearVelocity.y);
    
    if (rb2D.linearVelocity.x != 0)
    {
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
    else
    {
        audioSource.Stop();
    }
}

}
