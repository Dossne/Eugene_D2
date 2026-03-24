using System;
using R3;
using UnityEngine;

public static class ParticleSystemUtils
{
    public static Observable<Unit> WaitForParticleSystem(ParticleSystem ps)
    {
        return Observable.EveryUpdate()
            .Where(_ => ps.isPlaying)
            .Take(1)
            .SelectMany(_ => 
                Observable.EveryUpdate()
                    .Where(_ => !ps.isEmitting && ps.particleCount == 0)
                    .Take(1)
            );
    }
    
    // По идее будет с лучшей производительностью тип не каждый кадр, а 10 раз в сек
    public static Observable<Unit> WaitForParticleSystemFinishedOptimized(ParticleSystem ps)
    {
        return Observable.Interval(TimeSpan.FromSeconds(0.1f))
            .Where(_ => ps.isPlaying)
            .Take(1)
            .SelectMany(_ => 
                Observable.Interval(TimeSpan.FromSeconds(0.1f))
                    .Where(_ => !ps.isEmitting && ps.particleCount == 0)
                    .Take(1)
            );
    }
}