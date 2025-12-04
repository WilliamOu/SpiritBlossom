using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using SpiritBlossom.Items;
using Terraria.ModLoader.IO;
using System.IO;
using Terraria.ID;

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

        public CrowdControl CurrentEffect = CrowdControl.None;

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

        public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            // Sync the effect type so the client knows what is happening
            binaryWriter.Write((byte)CurrentEffect);

            // Only sync the specific variables needed for the active effect
            switch (CurrentEffect)
            {
                case CrowdControl.MortalSteel:
                    binaryWriter.WriteVector2(MortalSteelCurrentPosition);
                    binaryWriter.Write(MortalSteelCurrentVelocity);
                    binaryWriter.Write(MortalSteelKnockupDuration);
                    binaryWriter.Write(MortalSteelStunDuration);
                    binaryWriter.Write(Gravity);
                    break;

                case CrowdControl.FateSealedStun:
                    binaryWriter.WriteVector2(FateSealedStunPosition);
                    break;

                case CrowdControl.FateSealedPull:
                    binaryWriter.WriteVector2(FateSealedCurrentPosition);
                    binaryWriter.WriteVector2(FateSealedEndPosition);
                    binaryWriter.WriteVector2(FateSealedDirectionToEndPosition);
                    binaryWriter.Write(FateSealedNPCPullSpeedPerTick);
                    binaryWriter.Write(FateSealedPullDuration);
                    binaryWriter.Write(FateSealedStunDuration);
                    // Sync the FarthestNPC reference
                    binaryWriter.Write(FarthestNPC != null ? FarthestNPC.whoAmI : -1);
                    break;
            }
        }

        public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
        {
            // Read the effect type
            CurrentEffect = (CrowdControl)binaryReader.ReadByte();

            // Read the variables
            switch (CurrentEffect)
            {
                case CrowdControl.MortalSteel:
                    MortalSteelCurrentPosition = binaryReader.ReadVector2();
                    MortalSteelCurrentVelocity = binaryReader.ReadSingle();
                    MortalSteelKnockupDuration = binaryReader.ReadInt32();
                    MortalSteelStunDuration = binaryReader.ReadInt32();
                    Gravity = binaryReader.ReadSingle();
                    break;

                case CrowdControl.FateSealedStun:
                    FateSealedStunPosition = binaryReader.ReadVector2();
                    break;

                case CrowdControl.FateSealedPull:
                    FateSealedCurrentPosition = binaryReader.ReadVector2();
                    FateSealedEndPosition = binaryReader.ReadVector2();
                    FateSealedDirectionToEndPosition = binaryReader.ReadVector2();
                    FateSealedNPCPullSpeedPerTick = binaryReader.ReadSingle();
                    FateSealedPullDuration = binaryReader.ReadInt32();
                    FateSealedStunDuration = binaryReader.ReadInt32();

                    int npcIndex = binaryReader.ReadInt32();
                    if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
                    {
                        FarthestNPC = Main.npc[npcIndex];
                    }
                    break;
            }
        }
    }
}