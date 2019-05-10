using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Job
{
    Tile tile;
    float timeLeft;
    float speed;

    bool done;

    Action<Tile> jobDone;
    Action<Tile> jobCanceled;

    public Tile Tile { get => tile; set => tile = value; }
    public bool Done { get => done; set => done = value; }

    public Job(Tile tile, float time, float speed)
    {
        this.Tile = tile;
        timeLeft = time;
        this.speed = speed;
        Done = false;
    }

    public void cancelJob()
    {
        jobCanceled?.Invoke(Tile);
    }

    public void performJob()
    {
        timeLeft -= Time.deltaTime * speed;
        if (timeLeft <= 0 && jobDone != null)
        {
            Done = true;
            jobDone(Tile);
        }

    }

    public void registerJobDoneCallback(Action<Tile> callback)
    {
        jobDone += callback;
    }
    public void registerJobCancelledCallback(Action<Tile> callback)
    {
        jobCanceled += callback;
    }

}
