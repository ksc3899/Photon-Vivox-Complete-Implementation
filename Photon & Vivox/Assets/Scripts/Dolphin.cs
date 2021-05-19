using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class Dolphin : MonoBehaviour
{
    public float speed;
    public PathCreator pathCreator;

    private float distanceTravelled = 0f;
    private Animator animator;
    private float repeatRate;

    private void Awake()
    {
        repeatRate = 620f / speed;
        InvokeRepeating("RestoreDistance", 0f, repeatRate);
        InvokeRepeating("AnimateDolphin", 0f, 10f);
        
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        distanceTravelled += speed * Time.deltaTime;
        transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled);
        transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled);
    }

    private void RestoreDistance()
    {
        distanceTravelled = 0f;
    }

    private void AnimateDolphin()
    {
        int random = UnityEngine.Random.Range(0, 100);

        if (random < 25)
            animator.SetTrigger("1");
        else if (random < 50)
            animator.SetTrigger("2");
    }    
}
