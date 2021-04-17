using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Ship : WarpObject
{
    public Bullet Bullet;
    public AudioSource Fire;
    public AudioSource Thrust;
    public AudioSource Death;
    public GameObject SourceClassicThrusters;
    public GameObject SourceEnhancedThrusters;
    public GameObject ClassicObject;
    public GameObject EnhancedObject;
    public GameObject Thrusters { get; set; }    
    public InputDevice RightController = default(InputDevice);
    public MeshRenderer ActiveMeshRenderer { get; set; }

    private DateTime _lastShot = DateTime.MinValue;
    private DateTime _lastHyperspace = DateTime.MinValue;
    private bool IsHit = false;

    public Rigidbody Rigidbody { get; set; }
    // Start is called before the first frame update
    void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();

        ActiveMeshRenderer = GetComponentInChildren<MeshRenderer>();
        Thrusters = SourceClassicThrusters;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        SetShipRotation();

        HyperspaceCheck();

        ShootCheck();        

        if (!Game.Instance.IsScoreEntry && IsThrusting())
        {
            Thrusters.SetActive(true);
            if (!Thrust.isPlaying)
            {
                Thrust.Play();                
            }
            Rigidbody.AddForce(transform.forward * 0.5f, ForceMode.Acceleration);
        }
        else
        {
            Thrusters.SetActive(false);
        }

        CheckObjectForWarp(transform);
    }

    public bool GetRightController(out InputDevice rightController)
    {
        if (!RightController.isValid)
        {
            var rightHandDevices = new List<UnityEngine.XR.InputDevice>();
            UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, rightHandDevices);

            if (rightHandDevices.Count > 0)
            {
                RightController = rightHandDevices[0];
            }
        }

        rightController = RightController;

        return (rightController.isValid);
    }

    private bool IsThrusting()
    {
        InputDevice inputDevice;
        float thrustAmount = 0f;
        bool isThrusting = false;
        bool isGripped = false;

        if (!IsHit)
        {
            if (GetRightController(out inputDevice))
            {
                inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out thrustAmount);
                inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out isGripped);
            }            

            if (thrustAmount >= 1f || thrustAmount == 0 && isGripped)
            {
                isThrusting = true;
            }
        }

        return isThrusting;
    }

    private void SetShipRotation()
    {
        InputDevice inputDevice;
        Quaternion rotation;

        if (GetRightController(out inputDevice))
        {
            inputDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out rotation);

            transform.rotation = rotation;
        }
    }

    private void HyperspaceCheck()
    {
        if (WillHyperspace())
        {
            Hyperspace();
        }
    }

    private bool WillHyperspace()
    {
        InputDevice inputDevice;
        bool isPrimaryDown = false;

        if ((DateTime.Now - _lastHyperspace).TotalMilliseconds > 2000)
        {
            if (GetRightController(out inputDevice))
            {
                inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out isPrimaryDown);

                if (isPrimaryDown)
                {
                    _lastHyperspace = DateTime.Now;
                }
            }
        }

        return isPrimaryDown;
    }

    private void Hyperspace()
    {
        //make ship invisible/invincible
        IsHit = true;
        ActiveMeshRenderer.enabled = false;

        //wait then move ship somewhere else
        StartCoroutine(HyperspaceFinish());        
    }

    IEnumerator HyperspaceFinish()
    {
        yield return new WaitForSeconds(0.5f);

        Reset();

        transform.position = new Vector3(Game.Instance.GetRandomFloat(-1.3f, 1.3f), Game.Instance.GetRandomFloat(0.5f, 1.5f), Game.Instance.GetRandomFloat(-1.3f, 1.3f));
    }

    private void ShootCheck()
    {
        if (WillShoot())
        {
            Shoot();
        }
    }

    private bool WillShoot()
    {
        InputDevice inputDevice;
        bool isTriggerDown = false;

        if ((DateTime.Now - _lastShot).TotalMilliseconds > 200)
        {
            if (GetRightController(out inputDevice))
            {
                inputDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out isTriggerDown);

                if (isTriggerDown)
                {
                    _lastShot = DateTime.Now;
                }
            }            
        }

        return isTriggerDown;
    }

    private void Shoot()
    {
        //pew pew
        Fire.Play();
        var newBullet = Instantiate(Bullet);

        newBullet.transform.position = transform.position + transform.forward * 0.055f;
        newBullet.GetComponent<Rigidbody>().AddForce(transform.forward, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {        
        Asteroid asteroid;
        asteroid = other.gameObject.GetComponentInParent<Asteroid>();
        if (asteroid != null)
        {
            Explode();
        }
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
        GetComponentInChildren<ParticleSystem>().Play();
        Death.Play();

        IsHit = true;
        ActiveMeshRenderer.enabled = false;        

        Game.Instance.ReduceLivesLeft();

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSeconds(0.5f);        

        Reset();
    }

    public void Reset()
    {
        ActiveMeshRenderer.enabled = true;
        
        Thrusters.SetActive(true);
        transform.position = Vector3.zero + Vector3.up;
        Rigidbody.velocity = Vector3.zero;
        Rigidbody.angularVelocity = Vector3.zero;

        IsHit = false;
    }

    public void SetForScoreEntry()
    {
        transform.position = Vector3.zero + Vector3.up + Vector3.forward * 1.5f;
    }

    public void SwitchRenderer(RenderType renderType)
    {
        switch (renderType)
        {
            case RenderType.Classic:
                EnhancedObject.SetActive(false);
                ClassicObject.SetActive(true);
                Thrusters = SourceClassicThrusters;
                break;
            case RenderType.Enhanced:
                EnhancedObject.SetActive(true);
                ClassicObject.SetActive(false);
                Thrusters = SourceEnhancedThrusters;
                break;
            default:
                break;
        }
        ActiveMeshRenderer = GetComponentInChildren<MeshRenderer>();        
    }
}
