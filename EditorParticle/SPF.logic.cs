using System.Collections.Generic;
using EntryEngine;
using EntryEngine.UI;
using System;
using EntryEditor;
using EntryEngine.Serialize;

public partial class SPF : UIScene
{
    //public PF PF;
    /// <summary>ParticleStreamType打的特性注释</summary>
    public ASummaryP Summary;
    public Type ParticleStreamType;
    public string File;
    public ParticleStream[] Preview;

    public SPF()
    {
        Initialize();
    }
    
    private IEnumerable<ICoroutine> MyLoading()
    {
        return null;
    }

    public ParticleStream GetParticleStream()
    {
        //return (ParticleStream)Activator.CreateInstance(_SERIALIZE.LoadSimpleAQName(PF.TypeName));
        return (ParticleStream)Activator.CreateInstance(ParticleStreamType);
    }
}
