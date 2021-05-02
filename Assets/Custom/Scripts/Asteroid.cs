using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : WarpObject
{
    public AudioSource BangSmall;
    public AudioSource BangMedium;
    public AudioSource BangLarge;
    public int Size = 3;
    public GameObject[] ClassicObjects;
    public GameObject[] EnhancedObjects;
    public Light ExplosionLight;
    public ParticleSystem ClassicExplosion;
    public ParticleSystem EnhancedExplosion;

    public bool IsHit { get; set; } = false;
    public MeshRenderer ActiveMeshRenderer;
    public Rigidbody Rigidbody { get; set; }
    public int _asteroidVersion = 1;

    // Start is called before the first frame update
    void Start()
    {
        _asteroidVersion = UnityEngine.Random.Range(0, ClassicObjects.Length);
        SwitchRenderer(Game.Instance.RenderType);

        ActiveMeshRenderer = GetComponentInChildren<MeshRenderer>();

        //disable colliders to start with till they disperse
        StartCoroutine(InitColliders());

        if (Size == 3)
        {
            transform.position = new Vector3(Game.Instance.GetRandomFloat(-1.5f, 1.5f), Game.Instance.GetRandomFloat(-0.5f, 2.5f), Game.Instance.GetRandomFloat(-1.5f, 1.5f));
        }

        Rigidbody = GetComponent<Rigidbody>();

        Rigidbody.AddTorque(Game.Instance.GetRandomVector3(0, 360) * Game.Instance.GetRandomFloat(0.000003f * Game.Instance.CurrentSpeedMultiplier, 0.000015f * Game.Instance.CurrentSpeedMultiplier), ForceMode.Impulse);

        Rigidbody.AddForce(Game.Instance.GetRandomVector3(-180, 180) * Game.Instance.GetRandomFloat(0.0003f * Game.Instance.CurrentSpeedMultiplier, 0.0015f * Game.Instance.CurrentSpeedMultiplier), ForceMode.Impulse);

        Rigidbody.drag = Game.Instance.CurrentDrag;
        Rigidbody.angularDrag = Game.Instance.CurrentDrag;

        if (Size < 3)
        {
            transform.localScale = transform.localScale * 0.5f;
        }                
    }    

    // Update is called once per frame
    void FixedUpdate()
    {
        CheckObjectForWarp(transform);
    }

    public void Explode()
    {
        if (!IsHit)
        {
            StartCoroutine(WaitThenDestroy());
        }
    }

    IEnumerator InitColliders()
    {
        GetComponentInChildren<Collider>().enabled = false;

        yield return new WaitForSeconds(1f);

        GetComponentInChildren<Collider>().enabled = true;
    }

    IEnumerator WaitThenDestroy()
    {
        Asteroid child1 = null;
        Asteroid child2 = null;

        Game.Instance.ReduceEnemiesLeft();

        switch (Size.ToString())
        {
            case "1":
                BangSmall.Play();
                Game.Instance.HUD.IncrementScore(100);
                break;
            case "2":
                BangMedium.Play();
                Game.Instance.HUD.IncrementScore(50);
                break;
            case "3":
                BangLarge.Play();
                Game.Instance.HUD.IncrementScore(20);
                break;
            default:
                break;
        }

        IsHit = true;        

        Rigidbody.velocity = Vector3.zero;
        Rigidbody.angularVelocity = Vector3.zero;

        var particleSystem = Game.Instance.RenderType == RenderType.Classic ? ClassicExplosion : EnhancedExplosion;

        if (Size < 3)
        {
            //too small to see!
            particleSystem.transform.localScale = particleSystem.transform.localScale * 1.5f;            
        }

        particleSystem.Play();

        GameObject light = null;
        if (Game.Instance.RenderType == RenderType.Enhanced)
        {
            light = Instantiate(ExplosionLight.gameObject);
            light.SetActive(true);
        }

        if (Size > 1)
        {
            child1 = Instantiate(this);
            child1.Size--;
            //child1.transform.position = new Vector3(child1.transform.position.x + 0.05f, child1.transform.position.y + 0.05f + child1.transform.position.z + 0.05f);
            child1.GetComponentInChildren<Collider>().enabled = false;
            child2 = Instantiate(this);
            child2.Size--;
            //child2.transform.position = new Vector3(child2.transform.position.x - 0.05f, child2.transform.position.y - 0.05f + child2.transform.position.z - 0.05f);
            child1.GetComponentInChildren<Collider>().enabled = false;
        }

        ActiveMeshRenderer.enabled = false;
        GetComponentInChildren<Collider>().enabled = false;

        //yield on a new YieldInstruction that waits for 0.5 seconds.
        yield return new WaitForSeconds(0.75f);

        if (light != null)
        {
            light.SetActive(false);
        }

        if (child1 != null)
        {
            child1.GetComponentInChildren<Collider>().enabled = true;
            child2.GetComponentInChildren<Collider>().enabled = true;
        }

        //yield on a new YieldInstruction that waits for 10 seconds, allows particle system to finish.
        yield return new WaitForSeconds(10f);
        Destroy(this.gameObject);

        
    }

    public void SwitchRenderer(RenderType renderType)
    {
        if (!IsHit)
        {
            foreach (var item in EnhancedObjects)
            {
                if (item.activeSelf)
                {
                    item.SetActive(false);
                }
            }

            foreach (var item in ClassicObjects)
            {
                if (item.activeSelf)
                {
                    item.SetActive(false);
                }
            }

            switch (renderType)
            {
                case RenderType.Classic:
                    EnhancedObjects[_asteroidVersion].SetActive(false);
                    ClassicObjects[_asteroidVersion].SetActive(true);
                    break;
                case RenderType.Enhanced:
                    EnhancedObjects[_asteroidVersion].SetActive(true);
                    ClassicObjects[_asteroidVersion].SetActive(false);
                    break;
                default:
                    break;
            }
            ActiveMeshRenderer = GetComponentInChildren<MeshRenderer>();
        }
    }
}
