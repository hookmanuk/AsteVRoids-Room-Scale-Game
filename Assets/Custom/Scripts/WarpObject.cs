using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WarpObject : MonoBehaviour
{
    public bool IsWarping { get; set; }

    public bool CheckObjectForWarp(Transform transform, bool blnDestroyIfOutside = false)
    {        
        bool warped = false;

        if (!IsWarping)
        {
            if (transform.position.x > Game.Instance.BoundMaxX)
            {
                warped = true;
                if (blnDestroyIfOutside)
                {
                    Destroy(transform.gameObject);
                }
                else
                {
                    StartCoroutine(DissolveThenWarp(transform, new Vector3(Game.Instance.BoundMinX, transform.position.y, transform.position.z)));
                }
            }
            else if (transform.position.x < Game.Instance.BoundMinX)
            {
                warped = true;
                if (blnDestroyIfOutside)
                {
                    Destroy(transform.gameObject);
                }
                else
                {
                    StartCoroutine(DissolveThenWarp(transform, new Vector3(Game.Instance.BoundMaxX, transform.position.y, transform.position.z)));
                }
            }

            if (transform.position.y > Game.Instance.BoundMaxY)
            {
                warped = true;
                if (blnDestroyIfOutside)
                {
                    Destroy(transform.gameObject);
                }
                else
                {
                    StartCoroutine(DissolveThenWarp(transform, new Vector3(transform.position.x, Game.Instance.BoundMinY, transform.position.z)));
                }
            }
            else if (transform.position.y < Game.Instance.BoundMinY)
            {
                warped = true;
                if (blnDestroyIfOutside)
                {
                    Destroy(transform.gameObject);
                }
                else
                {
                    StartCoroutine(DissolveThenWarp(transform, new Vector3(transform.position.x, Game.Instance.BoundMaxY, transform.position.z)));
                }
            }

            if (transform.position.z > Game.Instance.BoundMaxZ)
            {
                warped = true;
                if (blnDestroyIfOutside)
                {
                    Destroy(transform.gameObject);
                }
                else
                {
                    StartCoroutine(DissolveThenWarp(transform, new Vector3(transform.position.x, transform.position.y, Game.Instance.BoundMinZ)));
                }
            }
            else if (transform.position.z < Game.Instance.BoundMinZ)
            {
                warped = true;
                if (blnDestroyIfOutside)
                {
                    Destroy(transform.gameObject);
                }
                else
                {
                    StartCoroutine(DissolveThenWarp(transform, new Vector3(transform.position.x, transform.position.y, Game.Instance.BoundMaxZ)));
                }
            }
        }

        return warped;
    }

    IEnumerator DissolveThenWarp(Transform transform, Vector3 warpPosition)
    {
        float t = 0;
        List<Material> dissolveMaterials = new List<Material>();

        IsWarping = true;

        //dissolveMaterials.Add(transform.GetComponent<MeshRenderer>().material);
        foreach (var item in transform.GetComponentsInChildren<MeshRenderer>())
        {
            dissolveMaterials.AddRange(item.materials.ToList());
        }

        while (t < 1f)
        {
            t += Time.deltaTime / 1f;

            foreach (var item in dissolveMaterials)
            {
                item.SetFloat("HIDDEN_RATIO", 1 - t / 1f);
            }

            yield return null;
        }

        transform.position = warpPosition;

        t = 0;
        while (t < 0.5f)
        {
            t += Time.deltaTime / 0.5f;

            foreach (var item in dissolveMaterials)
            {
                item.SetFloat("HIDDEN_RATIO", t / 0.5f);
            }

            yield return null;
        }

        IsWarping = false;
    }
}
