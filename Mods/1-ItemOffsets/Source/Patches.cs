using System;
using UnityEngine;

namespace _28AL.ItemOffsets;

public static class Patches
{
	[HarmonyPatch(typeof(ItemClass), "CloneModel", new Type[]{
			typeof(World), typeof(ItemValue), typeof(Vector3), typeof(Transform),
			typeof(BlockShape.MeshPurpose), typeof(long)
		})]
	static class Patches__ItemClass__CloneModel
	{
		static void Postfix(
				ItemClass __instance,
				ref BlockShape.MeshPurpose _purpose,
				ref Transform __result)
		{
			if(_purpose != BlockShape.MeshPurpose.Hold){
				return;
			}

			Vector3 posOffset = Vector3.zero;
			Vector3 rotOffset = Vector3.zero;

			if(__instance.Properties.Contains("PositionOffset")){
				posOffset = StringParsers.ParseVector3(
					__instance.Properties.GetString("PositionOffset"));
			}

			if(__instance.Properties.Contains("RotationOffset")){
				rotOffset = StringParsers.ParseVector3(
					__instance.Properties.GetString("RotationOffset"));
			}

			for(int i = 0; i < __result.childCount; i++){
				Transform t = __result.GetChild(i);
				t.localPosition += posOffset;
				t.localEulerAngles += rotOffset;
			}
		}
}
}