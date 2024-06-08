using System.Collections;
using System.Collections.Generic;
using UIIS;
using UnityEngine;
using UnityEngine.VFX;
using Utilities;


public class HerdAnimal : HerdBehaviour, IHealthComponent
{
    public float speed = 1f;
    public float runSpeed = 3f;
    IEnumerator rotate;

    public VisualEffect bloodeffect;
    public VisualEffect spitEffect;
    public float health = 100f;
    public bool isBleeding = false;
    public MeshRenderer faceRender;
    public SoundSource sound;

    // states: 0 = normal, 1 = bleeding started, 2 = permanant panicked fleeing, 3 panicked fleeing

    public Vector3 fleeFrom = Vector3.zero;

    private void Awake()
    {
        Death += HerdMemberDeathTrigger;
        faceRender.material.mainTextureOffset = new Vector2((float)Random.Range(0, ArtUtils.FaceAtlasCount) / ArtUtils.FaceAtlasCount, .5f);
        sound.audioSource.pitch = Mathf.Pow(2f, Mathf.Pow(Random.Range(-1f, 1f), 3f));
    }
    private void OnDestroy()
    {
        Death -= HerdMemberDeathTrigger;
    }

    public override void UpdateTask()
    {

        if (State == 0)
            NormalBehavior();
        else
            Panic();

        base.UpdateTask();

        if (State == 1) { State = 2;}

        Random.InitState(rndState);
        float chance = Random.value;
        if (State != 0) chance *= .5f;
        float spitChance = Random.value;
        if (chance < .05f)
        {
            sound.UpdateFilter();
            if (State == 0) sound.audioSource.PlayOneShot(SoundBank.Singleton.GetRandomIdleClip());
            else sound.audioSource.PlayOneShot(SoundBank.Singleton.GetRandomScaredeClip());
        }
        if (spitChance < .01f)
        {
            spitEffect.Play();
            spitEffect.GetComponent<AudioSource>().Play();
        }

        if (transform.position.magnitude > 1000f)
        {
            AddWound(10f);
        }
    }

    void NormalBehavior()
    {
        Random.InitState(rndState);
        float choice = Random.value;

        if (choice < .5f)
            MoveTo();
        else
            WaitFor();

    }



    public override void OnStateChange(int previous, int current)
    {
        base.OnStateChange(previous, current);

        if (current == 1)
        {
            Wound(5f);
        }
        if (current == 2)
        {
            arguments = 1f;
        }
        if (current != 0 && previous == 0)
        {
            fleeFrom = transform.position;
            Vector2 offset = faceRender.material.mainTextureOffset;
            offset.y = 0f;
            faceRender.material.mainTextureOffset = offset;
        }
        if (current == 0 && previous != 0) 
        {
            Vector2 offset = faceRender.material.mainTextureOffset;
            offset.y = .5f;
            faceRender.material.mainTextureOffset = offset;
        }  

    }



    public override void Rectify(Vector3 prevPosition, Vector3 currentTarget, float taskTime, int rnd, int state, float args)
    {
        base.Rectify(prevPosition, currentTarget, taskTime, rnd, state, args);

        if (targetPosition == previousPosition)
            return;
        if (rotate != null)
            StopCoroutine(rotate);
        rotate = RotateTowardsVector(targetPosition - previousPosition);
        StartCoroutine(rotate);
    }

    private void MoveTo()
    {
        Vector3 displacement = 10 * new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        if (GameManager.Singleton.musicPlaying) displacement -= transform.position / 100f;
        targetPosition = transform.position + displacement;
        timeToCompleteTask = displacement.magnitude / speed;

        if (rotate != null)
            StopCoroutine(rotate);
        rotate = RotateTowardsVector(targetPosition - previousPosition);
        StartCoroutine(rotate);
        
    }

    private void WaitFor()
    {
        timeToCompleteTask = 10f * Random.value;
    }

    void Panic()
    {
        Random.InitState(rndState);

        if (arguments < 0)
        {
            arguments = 0;
            State = 0;
            NormalBehavior();
            return;
        }

        Vector3 displacement = Vector3.zero;

        // move away from source of fear

        displacement += 5f * (transform.position - fleeFrom).normalized;

        // flock to other fleeing agents, cohesion, align, seperate

        Vector3 cohesionDisplacement = Vector3.zero;
        Vector3 alignDisplacement = Vector3.zero;
        Vector3 flockDisplacement = Vector3.zero;

        int _x = Mathf.FloorToInt(transform.position.x / herd.hashWidth);
        int _z = Mathf.FloorToInt(transform.position.z / herd.hashWidth);
        int oCount = 0;

        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                var hash = MathUtils.PairingF(_x+i,_z+j);
                if (herd.hashGrid.ContainsKey(hash))
                {
                    foreach(ulong mid in herd.hashGrid[hash])
                    {
                        if (mid == id) continue;

                        var other = herd.members[mid];

                        // spread fear to non-fleeing agents
                        if (other.State == 0) 
                        {
                            if ((transform.position - other.transform.position).magnitude > 5f || Random.value < (1f-.01f*arguments))
                                continue;

                            (other as HerdAnimal).arguments = .75f * arguments;
                            arguments *= .90f;
                            other.State = 3;
                            other.HaltTask();
                            continue; 
                        };

                        // cohesion
                        cohesionDisplacement += (other.transform.position - transform.position).normalized;
                        // alignment
                        alignDisplacement += other.transform.forward;
                        // seperate
                        flockDisplacement += 2f * (transform.position - other.transform.position).normalized / (transform.position - other.transform.position).magnitude;
                        oCount++;
                    }
                }
            }
        }

        if (oCount > 0)
        {
            flockDisplacement += 10f * cohesionDisplacement / oCount + 10f * alignDisplacement / oCount;
        }

        displacement += flockDisplacement + new Vector3(Random.Range(-1f,1f), 0, Random.Range(-1f, 1f));

        displacement = Vector3.ClampMagnitude(displacement, 10f);

        targetPosition = transform.position += displacement;
        timeToCompleteTask = displacement.magnitude / runSpeed;

        // fear gradually diminishes until a threshold is reached where agent transitions to normal
        if (State == 3)
            arguments -= timeToCompleteTask;

        if (rotate != null)
            StopCoroutine(rotate);
        rotate = RotateTowardsVector(targetPosition - previousPosition);
        StartCoroutine(rotate);
    }

    IEnumerator RotateTowardsVector(Vector3 v)
    {
        float rotateTime = .007f * Vector3.Angle(transform.forward, v);
        float t = 0f;
        Quaternion lookRotation = Quaternion.LookRotation(v, Vector3.up);
        Quaternion startRotation = transform.rotation;
        while (t < rotateTime)
        {
            t += Time.deltaTime;
            transform.rotation = Quaternion.Lerp(startRotation, lookRotation, t / rotateTime);
            yield return null;
        }
    }

    public void OnFearEvent(Vector3 position, float fearAmount)
    {
        TempAuthority = true;
        HaltTask();
        arguments = fearAmount;
        State = 3;
        fleeFrom = Vector3.ProjectOnPlane(position, Vector3.up);
    }

    void HerdMemberDeathTrigger(Vector3 flingForce) 
    {
        GetComponent<DeathComponent>().flingForce = flingForce;
        GetComponent<DeathComponent>().TriggerDeath();
        Destroy(gameObject);
    }

    public void AddWound(float bleedrate)
    {
        TempAuthority = true;
        HaltTask();
        State = 1;
    }

    public void Wound(float bleedRate)
    {
        
        isBleeding = true;
        bloodeffect.Play();
        BloodSplatterManager.Singleton.SpawnBloodPuddle(transform.position, 200f);
        health -= bleedRate;
        if (health < 0)
        {
            InstantKill(Vector3.zero);
            return;
        }
        StartCoroutine(BleedCoroutine(bleedRate));
    }

    public void InstantKill(Vector3 killForce)
    {
        herd.MemberDeathRpc(id, killForce);
    }

    IEnumerator BleedCoroutine(float bleedRate)
    {


        yield return new WaitForSeconds(10f);

        if (isBleeding)
        {
            bloodeffect.Play();
            BloodSplatterManager.Singleton.SpawnBloodPuddle(transform.position, 200f);

            health -= bleedRate;
            if (health < 0)
            {
                InstantKill(Vector3.zero);
            }

            StartCoroutine(BleedCoroutine(bleedRate));
        }
    }
}
