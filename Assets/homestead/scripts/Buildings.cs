﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RedHomestead.Simulation;

namespace RedHomestead.Buildings
{
    public enum Module
    {
        Unspecified = -1,
        SolarPanelSmall,
        SabatierReactor,
        SmallGasTank,
        LargeGasTank,
        Workspace,
        Habitat,
        OreExtractor,
        Smelter,
        SmallWaterTank,
        Splitter,
        WaterElectrolyzer,
        AlgaeTank
    }

    public static class Construction
    {
        private const int DefaultBuildTimeSeconds = 10;

        /// <summary>
        /// In seconds
        /// </summary>
        public static Dictionary<Module, int> BuildTimes = new Dictionary<Module, int>
        {
            {
                Module.SolarPanelSmall, DefaultBuildTimeSeconds
            },
            {
                Module.LargeGasTank, DefaultBuildTimeSeconds
            },
            {
                Module.SmallGasTank, DefaultBuildTimeSeconds
            },
            {
                Module.SmallWaterTank, DefaultBuildTimeSeconds
            },
            {
                Module.Splitter, DefaultBuildTimeSeconds
            },
            {
                Module.SabatierReactor, DefaultBuildTimeSeconds
            },
            {
                Module.OreExtractor, DefaultBuildTimeSeconds
            },
            {
                Module.WaterElectrolyzer, DefaultBuildTimeSeconds
            },
            {
                Module.AlgaeTank, DefaultBuildTimeSeconds
            }
        };

        //todo: load from file probably
        //todo: disallow duplicate resource types by using another dict instead of a list
        public static Dictionary<Module, List<ResourceEntry>> Requirements = new Dictionary<Module, List<ResourceEntry>>()
        {
            {
                Module.SolarPanelSmall, new List<ResourceEntry>()
                {
                    new ResourceEntry(2, Matter.Steel),
                    new ResourceEntry(4, Matter.SiliconWafers)
                }
            },
            {
                Module.LargeGasTank, new List<ResourceEntry>()
                {
                    new ResourceEntry(8, Matter.Steel)
                }
            },
            {
                Module.SmallGasTank, new List<ResourceEntry>()
                {
                    new ResourceEntry(1, Matter.Steel)
                }
            },
            {
                Module.SmallWaterTank, new List<ResourceEntry>()
                {
                    new ResourceEntry(1, Matter.Steel)
                }
            },
            {
                Module.Splitter, new List<ResourceEntry>()
                {
                    new ResourceEntry(1, Matter.Steel)
                }
            },
            {
                Module.SabatierReactor, new List<ResourceEntry>()
                {
                    new ResourceEntry(1, Matter.Steel)
                }
            },
            {
                Module.OreExtractor, new List<ResourceEntry>()
                {
                    new ResourceEntry(1, Matter.Steel)
                }
            },
            {
                Module.WaterElectrolyzer, new List<ResourceEntry>()
                {
                    new ResourceEntry(1, Matter.Steel)
                }
            },
            {
                Module.AlgaeTank, new List<ResourceEntry>()
                {
                    new ResourceEntry(2, Matter.Steel),
                    new ResourceEntry(2, Matter.Glass),
                    new ResourceEntry(2, Matter.Biomass),
                }
            }
        };
    }
}
