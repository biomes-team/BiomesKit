﻿using HarmonyLib;
using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BiomesKit
{

    public class BiomesKitWorldLayer : RimWorld.Planet.WorldLayer // Let's paint some worldmaterials.
	{
		private static readonly IntVec2 TexturesInAtlas = new IntVec2(2, 2); // two by two, meaning four variants for each worldmaterial.
		public override IEnumerable Regenerate()
		{
			foreach (object obj in base.Regenerate())
			{
				yield return obj;
			}
			Rand.PushState();
			Rand.Seed = Find.World.info.Seed;
			WorldGrid worldGrid = Find.WorldGrid;
			for (int tileID = 0; tileID < Find.WorldGrid.TilesCount; tileID++)
			{
				Tile tile = Find.WorldGrid[tileID];
				if (tile.biome.HasModExtension<BiomesKitControls>())
				{
					bool noRoad = tile.Roads.NullOrEmpty();
					bool noRiver = tile.Rivers.NullOrEmpty();
					BiomesKitControls biomesKit = tile.biome.GetModExtension<BiomesKitControls>();
					Vector3 vector = worldGrid.GetTileCenter(tileID);
					if (biomesKit.uniqueHills)
					{
						if (noRiver && noRoad)
						{
							Material hillMaterial;
							string hillPath = "WorldMaterials/BiomesKit/" + tile.biome.defName + "/Hills/";
                            switch (tile.hilliness)
                            {
                                case Hilliness.Flat:
                                    hillPath = null;
                                    break;
                                case Hilliness.SmallHills:
									if (tile.temperature < biomesKit.snowpilesBelow)
										hillPath += "SmallSnowpiles";
									else
										hillPath += "SmallHills";
                                    break;
                                case Hilliness.LargeHills:
									if (tile.temperature < biomesKit.snowpilesBelow)
										hillPath += "LargeSnowpiles";
									else
										hillPath += "LargeHills";
									break;
                                case Hilliness.Mountainous:
                                    hillPath += "Mountains";
									if (tile.temperature < biomesKit.mountainsFullySnowyBelow)
										hillPath += "_FullySnowy";
									else if (tile.temperature < biomesKit.mountainsSnowyBelow)
										hillPath += "_Snowy";
									else if (tile.temperature < biomesKit.mountainsSemiSnowyBelow)
										hillPath += "_SemiSnowy";
									break;
                                case Hilliness.Impassable:
                                    hillPath += "Impassable";
									if (tile.temperature < biomesKit.impassableFullySnowyBelow)
										hillPath += "_FullySnowy";
									else if (tile.temperature < biomesKit.impassableSnowyBelow)
										hillPath += "_Snowy";
									else if (tile.temperature < biomesKit.impassableSemiSnowyBelow)
										hillPath += "_SemiSnowy";
									break;

							}
							if (hillPath != null)
							{
                                hillMaterial = MaterialPool.MatFrom(hillPath, ShaderDatabase.WorldOverlayTransparentLit, biomesKit.materialLayer);
								LayerSubMesh subMeshHill = GetSubMesh(hillMaterial);
								WorldRendererUtility.PrintQuadTangentialToPlanet(vector, vector, (worldGrid.averageTileSize * biomesKit.impassableSizeMultiplier), 0.01f, subMeshHill, false, biomesKit.materialRandomRotation, false);
								WorldRendererUtility.PrintTextureAtlasUVs(Rand.Range(0, TexturesInAtlas.x), Rand.Range(0, TexturesInAtlas.z), TexturesInAtlas.x, TexturesInAtlas.z, subMeshHill);
							}
						}
					}
					if (biomesKit.forested && tile.hilliness == Hilliness.Flat && noRiver && noRoad)
					{
						string forestPath = "WorldMaterials/BiomesKit/" + tile.biome.defName + "/Forest/Forest_";
						bool pathHasChanged = false;
						switch (tile.temperature)
						{
							case float temp when temp < biomesKit.forestSnowyBelow:
								forestPath += "Snowy";
								pathHasChanged = true;
								break;
						}
						switch (tile.rainfall)
						{
							case float rain when rain < biomesKit.forestSparseBelow:
								forestPath += "Sparse";
								pathHasChanged = true;
								break;
							case float rain when rain > biomesKit.forestDenseAbove:
								forestPath += "Dense";
								pathHasChanged = true;
								break;
						}
						if (!pathHasChanged)
							forestPath = forestPath.Remove(forestPath.Length - 1, 1);
						Material forestMaterial = MaterialPool.MatFrom(forestPath, ShaderDatabase.WorldOverlayTransparentLit, biomesKit.materialLayer);
						LayerSubMesh subMeshForest = GetSubMesh(forestMaterial);
						WorldRendererUtility.PrintQuadTangentialToPlanet(vector, vector, (worldGrid.averageTileSize * biomesKit.materialSizeMultiplier), 0.01f, subMeshForest, false, biomesKit.materialRandomRotation, false);
						WorldRendererUtility.PrintTextureAtlasUVs(Rand.Range(0, TexturesInAtlas.x), Rand.Range(0, TexturesInAtlas.z), TexturesInAtlas.x, TexturesInAtlas.z, subMeshForest);
					}
					if (biomesKit.materialPath != "World/MapGraphics/Default")
					{
						Material material = MaterialPool.MatFrom(biomesKit.materialPath, ShaderDatabase.WorldOverlayTransparentLit, biomesKit.materialLayer);
						LayerSubMesh subMesh = GetSubMesh(material);
						WorldRendererUtility.PrintQuadTangentialToPlanet(vector, vector, (worldGrid.averageTileSize * biomesKit.materialSizeMultiplier), 0.01f, subMesh, false, biomesKit.materialRandomRotation, false);
						WorldRendererUtility.PrintTextureAtlasUVs(Rand.Range(0, TexturesInAtlas.x), Rand.Range(0, TexturesInAtlas.z), TexturesInAtlas.x, TexturesInAtlas.z, subMesh);
					}
				}
			}
			Rand.PopState();
			base.FinalizeMesh(MeshParts.All);
			yield break;
		}
	}
}
