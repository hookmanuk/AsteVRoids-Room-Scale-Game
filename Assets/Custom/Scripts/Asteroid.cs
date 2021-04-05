using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    public AudioSource BangSmall;
    public AudioSource BangMedium;
    public AudioSource BangLarge;
    public int Size = 3;
    public ParticleSystem Explosion;
    public bool IsHit { get; set; } = false;

    // Start is called before the first frame update
    void Start()
    {
        if (Size == 3)
        { 
            transform.position = new Vector3(Game.Instance.GetRandomFloat(-1.5f, 1.5f), Game.Instance.GetRandomFloat(-0.5f, 2.5f), Game.Instance.GetRandomFloat(-1.5f, 1.5f));
        }

        var rigidBody = GetComponent<Rigidbody>();
        rigidBody.AddTorque(Game.Instance.GetRandomVector3(0, 360) * Game.Instance.GetRandomFloat(0.000003f, 0.000015f), ForceMode.Impulse);        

        rigidBody.AddForce(Game.Instance.GetRandomVector3(-180, 180) * Game.Instance.GetRandomFloat(0.0003f, 0.0015f), ForceMode.Impulse);

        if (Size < 3)
        {
            transform.localScale = transform.localScale * 0.5f;
        }
    }    

    // Update is called once per frame
    void FixedUpdate()
    {
        Game.Instance.CheckObjectForWarp(transform);
    }

    public void Explode()
    {
        if (!IsHit)
        {
            StartCoroutine(WaitThenDestroy());
        }
    }

    IEnumerator WaitThenDestroy()
    {
        Asteroid child1 = null;
        Asteroid child2 = null;

        Game.Instance.EnemiesLeft--;

        switch (Size.ToString())
        {
            case "1":
                BangSmall.Play();
                break;
            case "2":
                BangMedium.Play();
                break;
            case "3":
                BangLarge.Play();                
                break;
            default:
                break;
        }
        
        Explosion.Play();

        if (Size > 1)
        {
            child1 = Instantiate(this);
            child1.Size--;
            //child1.transform.position = new Vector3(child1.transform.position.x + 0.05f, child1.transform.position.y + 0.05f + child1.transform.position.z + 0.05f);
            child1.GetComponent<Collider>().enabled = false;
            child2 = Instantiate(this);
            child2.Size--;
            //child2.transform.position = new Vector3(child2.transform.position.x - 0.05f, child2.transform.position.y - 0.05f + child2.transform.position.z - 0.05f);
            child1.GetComponent<Collider>().enabled = false;
        }

        IsHit = true;
        GetComponent<MeshRenderer>().enabled = false;

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(0.5f);

        Destroy(this.gameObject);

        if (child1 != null)
        {
            child1.GetComponent<Collider>().enabled = true;
            child2.GetComponent<Collider>().enabled = true;
        }
    }
}
