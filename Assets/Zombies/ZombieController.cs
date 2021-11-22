using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    public AudioSource[] splat;
    public GameObject target;
    public GameObject ragdoll;
    public float walkingSpeed;
    public float runningSpeed;
    public float damageAmount = 5;
    public int shotsRequired = 1;
    public int shotsTaken;
    Animator anim;
    NavMeshAgent agent;

    enum STATE { IDLE, WANDER, ATTACK, CHASE, DEAD};
    STATE state = STATE.IDLE;

    // Start is called before the first frame update
    void Start()
    {
        anim = this.GetComponent<Animator>();
        agent = this.GetComponent<NavMeshAgent>();
    }

    public void TurnOffTriggers() 
    {
        anim.SetBool("isWalking", false);
        anim.SetBool("isRunning", false);
        anim.SetBool("isAttacking", false);
        anim.SetBool("isDead", false);
    }

    float DistanceToPlayer()
    {
        if(GameStats.gameOver == true)
            return Mathf.Infinity;
        else
            return Vector3.Distance(target.transform.position, this.transform.position);
    }

    bool CanSeePlayer()
    {
        if (DistanceToPlayer() <= 10)
            return true;
        else
            return false;
    }

    bool ForgetPlayer()
    {
        if (DistanceToPlayer() > 20)
            return true;
        else 
            return false;
    }

    public void KillZombie()
    {
        TurnOffTriggers();
        anim.SetBool("isDead", true);
        state = STATE.DEAD;
    }

    public void DamagePlayer()
    {
        if (target != null)
        {
            target.GetComponent<FPController>().TakeHit(damageAmount);
            PlaySplatAudio();
        }
    }

    void PlaySplatAudio()
    {
        AudioSource audioSource = new AudioSource();
        int n = Random.Range(1, splat.Length);

        audioSource = splat[n];
        audioSource.Play();
        splat[n] = splat[0];
        splat[0] = audioSource;
      //  playingWalking = true;
    }


    // Update is called once per frame
    void Update()
    {

       /* if(Input.GetKeyDown(KeyCode.P))
        {
            if (Random.Range(0, 10) < 5)
            {
                GameObject rd = Instantiate(ragdoll, this.transform.position, this.transform.rotation);
                rd.transform.Find("Hips").GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * 10000);
                Destroy(this.gameObject);
            }
            else
            {
                TurnOffTriggers();
                anim.SetBool("isDead", true);
                state = STATE.DEAD;
            }
            return;
        } */

        if (target == null && GameStats.gameOver == false)
        {
            target = GameObject.FindWithTag("Player");
            return;
        }

        switch (state)
        {
            case STATE.IDLE:
                if (CanSeePlayer())
                    state = STATE.CHASE;
                else if(Random.Range(0,5000) < 5)
                    state = STATE.WANDER;
                break;

            case STATE.WANDER:
                if(!agent.hasPath)
                { 
                    float newX = this.transform.position.x + Random.Range(-5, 5);
                    float newZ = this.transform.position.z + Random.Range(-5, 5);
                    float newY = Terrain.activeTerrain.SampleHeight(new Vector3(newX, 0, newZ));
                    Vector3 dest = new Vector3(newX, newY, newZ);
                    agent.SetDestination(dest);
                    agent.stoppingDistance = 0;
                    agent.speed = walkingSpeed;
                    TurnOffTriggers();
                    anim.SetBool("isWalking", true);
                }

                if (CanSeePlayer())
                    state = STATE.CHASE;
                else if (Random.Range(0, 5000) < 5)
                {
                    state = STATE.IDLE;
                    TurnOffTriggers();
                    agent.ResetPath();
                }
                break;

            case STATE.CHASE:
                if (GameStats.gameOver == true) { TurnOffTriggers(); state = STATE.WANDER; return;}
                agent.SetDestination(target.transform.position);
                agent.stoppingDistance = 5;
                agent.speed = runningSpeed;
                TurnOffTriggers();
                anim.SetBool("isRunning", true);

                if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
                {
                    state = STATE.ATTACK;
                }

                if(ForgetPlayer())
                {
                    state = STATE.WANDER;
                    agent.ResetPath();
                }

                break;

            case STATE.ATTACK:
                if (GameStats.gameOver == true) { TurnOffTriggers(); state = STATE.WANDER; return; }
                TurnOffTriggers();
                anim.SetBool("isAttacking", true);
                this.transform.LookAt(target.transform.position);

                if (DistanceToPlayer() > agent.stoppingDistance+2)
                {
                    state = STATE.CHASE;
                }

                break;

            case STATE.DEAD:
                Destroy(agent);

                AudioSource[] source = this.GetComponents<AudioSource>();
                foreach (AudioSource s in source)
                    s.volume = 0;

                this.GetComponent<Sink>().StartSink();
                break;
        }

        /*agent.SetDestination(target.transform.position);

        if (agent.remainingDistance > agent.stoppingDistance)
        {
            anim.SetBool("isWalking", true);
            anim.SetBool("isAttacking", false);
        }
        else
        {
            anim.SetBool("isWalking", false);

        }

        if (Input.GetKey(KeyCode.W))
        {
            anim.SetBool("isWalking", true);
        }
        else
            anim.SetBool("isWalking", false);

        if (Input.GetKey(KeyCode.R))
        {
            anim.SetBool("isRunning", true);
        }
        else
            anim.SetBool("isRunning", false);

        if (Input.GetKey(KeyCode.A))
        {
            anim.SetBool("isAttacking", true);
        }
        else
            anim.SetBool("isAttacking", false);

        if (Input.GetKey(KeyCode.D))
        {
            anim.SetBool("isDead", true);
        }*/

    }
}
