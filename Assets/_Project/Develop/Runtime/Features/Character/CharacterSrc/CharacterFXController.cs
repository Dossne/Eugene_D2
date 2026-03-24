using System;
using Infrastructure.AudioControl;
using R3;
using UnityEngine;

public class CharacterFXController : MonoBehaviour
{
    [Header("Level up")]
    [SerializeField] private ParticleSystem lvlUpVfx;
    [SerializeField] private SfxType lvlUpSfx;

    [Header("Booster size")]
    [SerializeField] private ParticleSystem boosterSizeVfx;
    [SerializeField] private SfxType boosterSfx;

    [Header("Boost bottle")]
    [SerializeField] private ParticleSystem boostBottleParticle;
    [SerializeField] private SfxType boostBottleSfx;

    [Header("Super speed")]
    [SerializeField] private ParticleSystem boostSuperSpeedParticle;
    [SerializeField] private SfxType boostSuperSpeedSfx;

    [Header("Death up")]
    [SerializeField] private ParticleSystem deathParticle;

    private readonly CompositeDisposable disposables = new();
    private Action onDeathCallback;
    private bool isInit;


    public void Initialize()
    {
        if (isInit)
            return;

        isInit = true;
    }


    public void Deinitialize()
    {
        if (!isInit)
            return;

        disposables.Dispose();
        isInit = false;
    }


    public void PlayLevelUpFX()
    {
        lvlUpVfx.Play();
        AudioService.I.PlaySfx(lvlUpSfx);
    }


    public void PlaySizeBoosterFX()
    {
        if (boosterSizeVfx != null)
            boosterSizeVfx.Play();

        AudioService.I.PlaySfx(boosterSfx);
    }


    public void PlayBoostBottleFx()
    {
        boostBottleParticle.Play();
        AudioService.I.PlaySfx(boostBottleSfx);
    }

    public void SetActiveSuperSpeedFX(bool isActive)
    {
        if (boostSuperSpeedParticle != null)
            if (isActive)
                boostSuperSpeedParticle.Play();
            else
                boostSuperSpeedParticle.Stop();

        if (isActive)
            AudioService.I.PlaySfx(boosterSfx);
    }


    public void PlayDeathVFX(Action callback = null)
    {
        deathParticle.Play();

        ParticleSystemUtils.WaitForParticleSystemFinishedOptimized(deathParticle)
                           .Subscribe(_ => callback?.Invoke())
                           .AddTo(disposables);
    }

}