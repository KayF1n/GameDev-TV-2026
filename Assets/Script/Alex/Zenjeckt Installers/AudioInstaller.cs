using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using Zenject;

public class AudioInstaller : MonoInstaller {
    [SerializeField] private AudioStateConfig audioConfig;

    public override void InstallBindings() {
        Container.BindInterfacesAndSelfTo<AudioSystem>().AsSingle().NonLazy();

        Container.BindInterfacesAndSelfTo<FmodAudioService>()
            .AsSingle()
            .WithArguments(audioConfig);
    }

}




