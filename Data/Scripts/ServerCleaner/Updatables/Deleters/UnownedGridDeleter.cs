﻿using System.Linq;

using VRage.Game.ModAPI;

namespace ServerCleaner.Updatables.Deleters
{
	/// <summary>
	/// Deleter of cubegrids that have few blocks and no owners.
	/// </summary>
	public class UnownedGridDeleter : RepeatedDeleter<IMyCubeGrid, CubeGridDeletionContext>
	{
		public UnownedGridDeleter(double interval, double playerDistanceThreshold, int blockCountThreshold, bool messageAdminsOnly)
			: base(interval, messageAdminsOnly, new CubeGridDeletionContext() { PlayerDistanceThreshold = playerDistanceThreshold })
		{
			BlockCountThreshold = blockCountThreshold;
		}

		protected override bool BeforeDelete(IMyCubeGrid entity, CubeGridDeletionContext context)
		{
			if (entity.SmallOwners.Count > 0)
				return false;

			context.CurrentEntitySlimBlocks.Clear();
			entity.GetBlocksIncludingFromStaticallyAttachedCubeGrids(context.CurrentEntitySlimBlocks);

			if (context.CurrentEntitySlimBlocks.Count > BlockCountThreshold)
				return false;

			if (context.CurrentEntitySlimBlocks.IsAttachedWheelGrid())
				return false;
            // probably dont need
            //if (context.CurrentEntitySlimBlocks.Any(slimBlock => slimBlock.FatBlock != null && (slimBlock.FatBlock is IMyPistonTop || slimBlock.FatBlock is IMyMotorRotor || slimBlock.FatBlock is IMyMotorAdvancedRotor)))
            //    return false;

            return true;
		}

		protected override void AfterDeletion(CubeGridDeletionContext context)
		{
			if (context.EntitiesForDeletion.Count == 0)
				return;

			ShowMessageFromServer("Deleted {0} grid(s) that had fewer than {1} blocks, no owner and no players within {2} m: {3}.",
				context.EntitiesForDeletion.Count, BlockCountThreshold, context.PlayerDistanceThreshold, string.Join(", ", context.EntitiesForDeletionNames));
		}

		public int BlockCountThreshold { get; private set; }
	}
}
