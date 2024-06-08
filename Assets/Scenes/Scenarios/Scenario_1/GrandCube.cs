using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class GrandCube : NetworkBehaviour
{
    public Vector3 targ;
    public float radius;
    public float speed = 5f;
    IEnumerator coroutine;

    private void Start()
    {
        if (IsServer)
        {
            coroutine = UpdateCube();
            StartCoroutine(coroutine);
        }
    }

    [Rpc(SendTo.Server)]
    public void SetTargetRpc(Vector3 position)
    {
        StopCoroutine(coroutine);
        coroutine = SetTarget(position);
        StartCoroutine(coroutine);
    }

    IEnumerator SetTarget(Vector3 position)
    {

        Vector3 target = Vector3.ProjectOnPlane(position, Vector3.up) + 5f * Vector3.up;
        targ = target;
        Vector3 displacement = target - transform.position;
        Vector3 startPos = transform.position;

        float time = displacement.magnitude / speed;

        float t = 0f;

        while (t < time)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, target, t / time);

            yield return null;
        }

        coroutine = UpdateCube();
        StartCoroutine(coroutine);
    }

    IEnumerator UpdateCube()
    {
        Vector3 target = GetNextTarget();
        targ = target;
        Vector3 displacement = target - transform.position;
        Vector3 startPos = transform.position;

        float time = displacement.magnitude / speed;

        float t = 0f;

        while(t < time)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, target, t / time);

            yield return null;
        }

        coroutine = UpdateCube();
        StartCoroutine(coroutine);
    }


    Vector3 GetNextTarget()
    {
        Vector3 target = new Vector3(Random.value, Random.value, Random.value);
        

        int choice = Random.Range(0, 3);
        if (choice == 0)
        {
            target.x = Mathf.Round(target.x);
        }
        if (choice == 1)
        {
            target.y = Mathf.Round(target.y);
        }
        else if (choice == 2)
        {
            target.z = Mathf.Round(target.z);
        }

        target *= 2;
        target -= Vector3.one;
        target.y /= 2;
        target.y += .5f;
        target = target * radius;

        return target + Vector3.up * 5f;
    }
}
