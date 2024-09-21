using SpiritBlossom.Common.GlobalNPCs;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;
using Terraria;
using SpiritBlossom.Items;
using System;
using Terraria.Audio;

namespace SpiritBlossom.Buffs
{
    public class MortalSteelCooldown : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
        }
    }
}