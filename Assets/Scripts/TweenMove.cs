using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween;
using Holoville.HOTween.Plugins;

public class TweenMove : MonoBehaviour 
{
    [HideInInspector]
    public PathManager pathContainer;
    [HideInInspector]
    public static int counter = 0;
    public static bool flag = false;
    public AudioClip walkSound;     //dzwiek chodzenia
    public int walkVolume;
    public PathType pathtype = PathType.Curved;
    public bool orientToPath = false;
    public float sizeToAdd = 0;
    public float speed = 5;
    [HideInInspector]
    public float maxSpeed;
    [HideInInspector]
    public float tScale = 1f;
    private Transform[] waypoints;
    public Tweener tween = null;
    private Vector3[] wpPos;
    private TweenParms tParms;
    private PlugVector3Path plugPath;

    IEnumerator OnSpawn()
    {
        yield return new WaitForEndOfFrame();
        waypoints = pathContainer.waypoints;
        InitWaypoints();
        StartMove();
    }

    internal void InitWaypoints()
    {
        wpPos = new Vector3[waypoints.Length];
        for (int i = 0; i < wpPos.Length; i++)
        {
            wpPos[i] = waypoints[i].position + new Vector3(0, sizeToAdd, 0);
        }
    }

    public void StartMove()
    {
        transform.position = wpPos[0];
        CreateTween();
    }

    internal void CreateTween()
    {
        plugPath = new PlugVector3Path(wpPos, true, pathtype);
        if (orientToPath)
            plugPath.OrientToPath();
        tParms = new TweenParms();
        tParms.Prop("position", plugPath);
        tParms.AutoKill(true);
        tParms.SpeedBased();
        tParms.Ease(EaseType.Linear);
        tParms.OnComplete(OnPathComplete);
        if (!flag)
        {
            flag = true;
            tParms.OnUpdate(OnUpdate);
        }
        //tParms.OnPlay(OnUpdate);
        tween = HOTween.To(transform, maxSpeed, tParms);
    }
    internal void OnUpdate()
    {
        if(walkSound == null)
        {
            return;
        }
        float seconds = walkSound.length;
        int seconds2 = (int)(seconds * 30);
        if(seconds2 < 30)
        {
            seconds2 = 30;
        }
        if (counter % seconds2 == 0)
        {
            counter = 0;
            if (walkVolume > 100) walkVolume = 100;
            if (walkVolume < 0) walkVolume = 0;
            float volume = (float)(walkVolume) / 100.0f;
            AudioManager.Play2D(walkSound, volume);
        }
        counter++;
    }

    internal void OnPathComplete()
    {
        gameObject.SendMessage("PathEnd", SendMessageOptions.DontRequireReceiver);
    }

    public void Slow()
    {
        float newValue = speed / maxSpeed;
        tween.timeScale = tScale * newValue;
    }

    public void Accelerate()
    {
        speed = maxSpeed;
        tween.timeScale = tScale;
    }

    void OnDespawn()
    {
        speed = maxSpeed;
        tScale = 1f;
    }

    [System.Serializable]
    public class ProgMapProps
    {
        public bool enabled = false;
        public GameObject prefab;
        public float updateInterval = 0.25f;
        [HideInInspector]
        public int myID;
    }
}
