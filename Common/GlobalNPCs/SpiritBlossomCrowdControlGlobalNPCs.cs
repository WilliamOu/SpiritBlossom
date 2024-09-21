using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using SpiritBlossom.Items;

namespace SpiritBlossom.Common.GlobalNPCs
{
    internal class SpiritBlossomCrowdControlGlobalNPCs : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public enum CrowdControl
        {
            None,
            MortalSteel,
            FateSealedStun,
            FateSealedPull
        }

        public CrowdControl CurrentEffect;

        // Mortal Steel
        public const int MaxMortalSteelKnockupDuration = 42;
        public const int MaxMortalSteelStunDuration = 45;
        public const float MortalSteelInitialVelocity = -7.5f;
        public float MortalSteelCurrentVelocity;
        public int MortalSteelKnockupDuration;
        public int MortalSteelStunDuration;
        public Vector2 MortalSteelInitialPosition;
        public Vector2 MortalSteelCurrentPosition;
        public float Gravity;

        // Fate Sealed Stun
        public Vector2 FateSealedStunPosition;

        // Fate Sealed Pull
        public const float MaxFateSealedNPCPullSpeedPerTick = 40f;
        public const int MaxFateSealedStunDuration = 32;
        public float FateSealedNPCPullSpeedPerTick;
        public int FateSealedStunDuration;
        public int FateSealedPullDuration;
        public Vector2 FateSealedDirectionToEndPosition;
        public Vector2 FateSealedInitialPosition;
        public Vector2 FateSealedCurrentPosition;
        public Vector2 FateSealedEndPosition;
        public NPC FarthestNPC;

        public void InitializeMortalSteelValues(NPC npc)
        {
            CurrentEffect = CrowdControl.MortalSteel;
            MortalSteelKnockupDuration = MaxMortalSteelKnockupDuration;
            MortalSteelStunDuration = MaxMortalSteelStunDuration;
            MortalSteelCurrentVelocity = MortalSteelInitialVelocity;

            Gravity = (MortalSteelInitialVelocity * 2) / MaxMortalSteelKnockupDuration;
            MortalSteelInitialPosition = npc.position;
            MortalSteelCurrentPosition = MortalSteelInitialPosition;
            npc.netUpdate = true;
        }

        public void InitializeFateSealedStunValues(NPC npc)
        {
            CurrentEffect = CrowdControl.FateSealedStun;
            FateSealedStunPosition = npc.position;
            npc.netUpdate = true;
        }

        public void InitializeFateSealedPullValues(NPC npc, NPC farthestNPC, Vector2 endPosition, Vector2 fateSealedForwardVectorNormalized)
        {
            CurrentEffect = CrowdControl.FateSealedPull;
            FateSealedNPCPullSpeedPerTick = MaxFateSealedNPCPullSpeedPerTick;
            FateSealedStunDuration = MaxFateSealedStunDuration;
            FateSealedInitialPosition = FateSealedCurrentPosition = npc.position;
            FateSealedEndPosition = endPosition;
            FarthestNPC = farthestNPC;

            // By making only the farthest NPC travel to the end point, we make it so that multiple NPCs are not inadvertantly stacked on top of each other on the same point
            // I.e, NPCs will instead form a small cluster instead of being stacked on a single point
            Vector2 travelVector = FateSealedEndPosition - FateSealedInitialPosition;
            if (npc != FarthestNPC)
            {
                travelVector *= 0.9f;
                FateSealedEndPosition = travelVector + FateSealedInitialPosition;
            }

            // Recalculate NPC safe pull position based on the npc's position projected on Fate Sealed's forward vector
            // This prevents NPC travel vectors from fanning out past the original focal point
            Vector2 npcToFateSealedForwardVector = npc.Center - FateSealedInitialPosition;
            float projectionLength = Vector2.Dot(npcToFateSealedForwardVector, fateSealedForwardVectorNormalized);
            Vector2 projectedPosition = FateSealedInitialPosition + projectionLength * fateSealedForwardVectorNormalized;
            Vector2 projectedDirection = Vector2.Normalize(FateSealedEndPosition - projectedPosition);
            float projectedDistance = Vector2.Distance(projectedPosition, FateSealedEndPosition);
            float safeProjectedDistance = SBUtils.SafeLocationLinearSearch(projectedDistance, projectedDirection, projectedPosition, npc.width, npc.height);
            // bool canCluster = projectedDistance == safeProjectedDistance; 
            FateSealedEndPosition = projectedPosition + safeProjectedDistance * projectedDirection;

            float distanceToEnd = Vector2.Distance(FateSealedInitialPosition, FateSealedEndPosition);
            FateSealedPullDuration = (int)(distanceToEnd / MaxFateSealedNPCPullSpeedPerTick);
            FateSealedDirectionToEndPosition = Vector2.Normalize(travelVector);

            npc.netUpdate = true;
        }
    }
}