using UnityEngine;

public class FallDown : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        if (transform.position.y < -5f)
            Destroy(gameObject);
    }
<<<<<<< Updated upstream
}
=======
}
>>>>>>> Stashed changes
