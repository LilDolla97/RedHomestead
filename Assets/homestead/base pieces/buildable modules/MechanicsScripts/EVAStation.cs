﻿using System;
using System.Collections;
using System.Collections.Generic;
using RedHomestead.Buildings;
using UnityEngine;
using RedHomestead.EVA;
using RedHomestead.Simulation;
using RedHomestead.Industry;
using RedHomestead.Electricity;

public class EVAStation : Converter, IPowerConsumer
{
    private ISink OxygenIn;
    public SpriteRenderer Lung, LungOnOff, Power, PowerOnOff;
    public Color OnColor, OffColor;
    public Sprite OnSprite, OffSprite;
    public GameObject TaggedCollider;

    private bool HasOxygen
    {
        get { return OxygenIn != null; }
    }

    public override float WattsConsumed
    {
        get
        {
            return EVA.PowerResupplyWattsPerSecond;
        }
    }
    
    public bool IsOn { get; set; }

    private void SyncStatusSprites()
    {
        TaggedCollider.tag = (HasOxygen || HasPower) ? "evacharger" : "Untagged";

        Lung.color = LungOnOff.color = HasOxygen ? OnColor : OffColor;
        Power.color = PowerOnOff.color = HasPower ? OnColor : OffColor;

        LungOnOff.sprite = HasOxygen ? OnSprite : OffSprite;
        PowerOnOff.sprite = HasPower ? OnSprite : OffSprite;
    }

    protected override void OnStart()
    {
        base.OnStart();
        this.SyncStatusSprites();
    }

    public override void ClearHooks()
    {
        OxygenIn = null;
        this.SyncStatusSprites();
    }

    float oxygenBuffer;
    public override void Convert()
    {
        if (IsOn)
        {
            if (HasPower)
                SurvivalTimer.Instance.Power.Resupply(EVA.PowerResupplySeconds);

            if (OxygenIn != null)
            {
                if (oxygenBuffer < EVA.OxygenResupplyKilogramsPerUnit)
                    oxygenBuffer += OxygenIn.Get(Matter.Oxygen).Pull(EVA.OxygenResupplyKilogramsPerUnit);

                if (oxygenBuffer >= EVA.OxygenResupplyKilogramsPerUnit)
                {
                    SurvivalTimer.Instance.Oxygen.Resupply(EVA.OxygenResupplySeconds);
                    oxygenBuffer -= EVA.OxygenResupplyKilogramsPerUnit;
                }
            }
        }
    }

    public override Module GetModuleType()
    {
        return Module.EVAStation;
    }

    public override ResourceContainerDictionary GetStartingDataContainers()
    {
        return new ResourceContainerDictionary();
    }

    public override void Report() { }

    public void ToggleUse(bool pumpingToEVA)
    {
        IsOn = pumpingToEVA;
    }

    public override void OnSinkConnected(ISink s)
    {
        if (s.HasContainerFor(Matter.Oxygen))
        {
            OxygenIn = s;
            this.SyncStatusSprites();
        }
    }

    public void OnEmergencyShutdown()
    {
        this.SyncStatusSprites();
    }

    public override void OnPowerChanged()
    {
        this.SyncStatusSprites();
    }
}