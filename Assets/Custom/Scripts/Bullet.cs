using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{    
    // Start is called before the first frame update
    void Start()
    {     
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Game.Instance.CheckObjectForWarp(transform, true);
    }

    private void OnTriggerEnter(Collider other)
    {
        Asteroid asteroid;
        asteroid = other.gameObject.GetComponent<Asteroid>();
        if (asteroid != null)
        {
            Destroy(this.gameObject);
            asteroid.Explode();
        }        
    }
}
