using System.Xml.Linq;
using UnityEngine;

public class Init : IModApi
{
	public static Mod? ModInstance { get; set; }
	public static Harmony? HarmonyInstance { get; set; }

	public void InitMod(Mod _modInstance)
	{
		ModInstance = _modInstance;
		HarmonyInstance = new(_modInstance.Name + "." + 
			_modInstance.VersionString.Replace('.', '-'));

		HarmonyInstance.PatchAll(typeof(Init).Assembly);
	}
}

[HarmonyPatch(typeof(MinEventActionAttachParticleEffectToEntity))]
public static class PatchesAttachParticleEffectToEntity
{
	[HarmonyPatch(nameof(MinEventActionAttachParticleEffectToEntity.ParseXmlAttribute))]
	[HarmonyPrefix]
	public static bool ParseXmlAttributePrefix(
			MinEventActionAttachParticleEffectToEntity __instance,
			XAttribute _attribute,
			ref bool __result)
	{
		bool baseRet = PatchesMinEventActionTargetedBase.ParseXmlAttributeRP(
				__instance, _attribute);

		if(!baseRet) {
			string localName = _attribute.Name.LocalName;

			if (localName == "particle") {
				GameObject? addressable = LoadManager.LoadAssetFromAddressables<GameObject>(
					$"ParticleEffects/{_attribute.Value}.prefab", null, null, false, true).Asset;

				if(addressable == null){
					var path = DataLoader.ParseDataPathIdentifier(_attribute.Value);

					if(path.IsBundle){
						__instance.goToInstantiate = DataLoader.LoadAsset<GameObject>(path);
						__result = true;
						return false;
					}

					__result = false;
					return false;
				}

				__instance.goToInstantiate = addressable;
				__result = true;
				return false;
			}

			if (localName == "parent_transform") {
				__instance.parent_transform_path = _attribute.Value;
				__result = true;
				return false;
			}

			if (localName == "local_offset") {
				__instance.local_offset = StringParsers.ParseVector3(_attribute.Value, 0, -1);
				__result = true;
				return false;
			}

			if (localName == "local_rotation") {
				__instance.local_rotation = StringParsers.ParseVector3(_attribute.Value, 0, -1);
				__result = true;
				return false;
			}

			if (localName == "shape_mesh") {
				__instance.setShapeMesh = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
				__result = true;
				return false;
			}
		}

		__result = baseRet;
		return false;
	}
}

[HarmonyPatch(typeof(MinEventActionTargetedBase))]
public static class PatchesMinEventActionTargetedBase
{
	[HarmonyPatch(nameof(MinEventActionTargetedBase.ParseXmlAttribute))]
	[HarmonyReversePatch]
	public static bool ParseXmlAttributeRP(
			MinEventActionTargetedBase inst,
			XAttribute attr)
	{
		_ = inst;
		_ = attr;
		return false;
	}
}