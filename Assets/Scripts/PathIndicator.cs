using UnityEngine;
using System.Collections;
using Holoville.HOTween;
using Holoville.HOTween.Plugins;

public class PathIndicator : MonoBehaviour
{
    private ParticleSystem pSys;
    private TweenMove myMove;
    public PathManager pathToFollow;


    void Start()
    {
        pSys = GetComponentInChildren<ParticleSystem>();
        myMove = GetComponent<TweenMove>();
        myMove.pathContainer = pathToFollow;
        myMove.maxSpeed = myMove.speed;
        myMove.StartCoroutine("OnSpawn");
        StartCoroutine("EmitParticles");
    }

    IEnumerator EmitParticles()
    {
        yield return new WaitForEndOfFrame();
        while (true)
        {
            pSys.Emit(1);
            yield return new WaitForSeconds(0.2f);
        }
    }

    void PathEnd()
    {
        myMove.StartMove();
    }
}
