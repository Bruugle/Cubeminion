using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;


public class HerdBehaviour : MonoBehaviour
{
    public ulong id;
    public ulong hashKey;
    public Herd herd;
    public Vector3 targetPosition;
    public Vector3 previousPosition;
    public Vector3 errorV = Vector3.zero;
    public Vector3 errorVitp = Vector3.zero;
    public bool isMoving = false;
    public float timeToCompleteTask;
    public float timeSinceTaskStart;
    public bool TempAuthority = false;
    public float arguments;

    public Action<Vector3> Death;

    public int rndState;

    [SerializeField]
    int _state;
    public int State
    {
        get { return _state; }
        set
        {
            if (_state != value) 
            {
                OnStateChange(_state, value);
                _state = value;
            }
        }
    }


    public void HerdUpdate()
    {
        
        timeSinceTaskStart += Time.deltaTime;

        if (timeSinceTaskStart >= timeToCompleteTask)
        {
            targetPosition = transform.position;
            previousPosition = transform.position;

            errorV = Vector3.zero;
            errorVitp = Vector3.zero;

            UpdateTask();
        }

        if (targetPosition != previousPosition && timeToCompleteTask > 0)
        {
            isMoving = true;
            errorV = Vector3.Lerp(errorVitp, Vector3.zero, timeSinceTaskStart / timeToCompleteTask);
            transform.position = errorV + Vector3.Lerp(previousPosition, targetPosition, timeSinceTaskStart / timeToCompleteTask);
            HashGridQ();
        }
        else
        {
            isMoving = false;
        }

    }

    public void HaltTask()
    {
        timeToCompleteTask = timeSinceTaskStart;
    }

    public virtual void UpdateTask()
    {
        // always implement custom behaviour before base

        timeSinceTaskStart = 0f;
        int rnd = UnityEngine.Random.Range(1, int.MaxValue);
        herd.SendTask(TempAuthority, id, previousPosition, targetPosition, timeToCompleteTask, rnd, State, arguments);
        rndState = rnd;
        TempAuthority = false;
        
    }

    public virtual void OnStateChange(int previous, int current)
    {

    }

    public virtual void Rectify(Vector3 prevPosition, Vector3 currentTarget, float taskTime, int rnd, int state, float args)
    {
        errorVitp = transform.position - prevPosition;
        targetPosition = currentTarget;
        previousPosition = prevPosition;
        timeToCompleteTask = taskTime;
        rndState = rnd;
        State = state;
        arguments = args;
        timeSinceTaskStart = 0f;
        
    }

    

    public void HashGridQ()
    {
        int _x = Mathf.FloorToInt(transform.position.x / herd.hashWidth);
        int _z = Mathf.FloorToInt(transform.position.z / herd.hashWidth);

        ulong currentHashKey = MathUtils.PairingF(_x, _z);

        if (currentHashKey == hashKey)
            return;

        
        // remove agent from old hash

        List<ulong> hash;
        if (herd.hashGrid.TryGetValue(hashKey, out hash))
        {
            hash.Remove(id);

            // clear out empty hashes
            if (hash.Count == 0)
            {
                herd.hashGrid.Remove(hashKey);
            }
        }

        // add to new hash if it already exists

        if (herd.hashGrid.TryGetValue(currentHashKey, out hash))
        {
            hash.Add(id);
        }
        // or create new one if it does not exist
        else
        {
            List<ulong> newHash = new List<ulong>
            {
                id
            };

            herd.hashGrid.Add(currentHashKey, newHash);
        }

        hashKey = currentHashKey;

    }
}
