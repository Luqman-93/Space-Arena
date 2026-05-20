using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightEnemy : MonoBehaviour
{
    Player _player;
    void Start()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
    }
    // Update is called once per frame
    void Update()
    {
        movement();
    }
    void movement()
    {
        float speed = 4f;
        Vector3 move = Vector3.left * speed * Time.deltaTime;
        transform.Translate(move);

        if (transform.position.x <= -9)
        {
            Destroy(this.gameObject);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PLAYER"))
        {
            if (_player != null)
            {
                _player.damage();
            }
            Destroy(gameObject);
            return;
        }

        if (collision.CompareTag("LASER"))
        {
            Destroy(gameObject);
            Destroy(collision.gameObject);
            if (_player != null)
            {
                _player.addScore(10);
            }
        }
    }
}
