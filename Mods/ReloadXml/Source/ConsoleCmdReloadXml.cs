using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ReloadXml : ConsoleCmdAbstract
{
	protected override string[] getCommands() => 
		new[]{ "reloadxml", "rx" };

	public override bool AllowedInMainMenu => true;

	protected override string getDescription() => """

		Usage: ReloadXML {'all' OR XmlFileName1 XmlFileName2 XmlFileName3 ...}

		Reloads either all XML if 'all' is given as the first parameter, or if XML
		XML file basenames (e.g. vehicles, buffs) are given, the corresponding
		files are reloaded in the given order, e.g. 'rx vehicles buffs misc' will
		reload vehicles, then buffs, then misc.

		Reloading 'all' uses a different method to reloading individual files. The
		'all' method works more consistently, but takes much longer. Conversely,
		not all individual files even have code to reload during the game and
		cannot be reloaded without using 'rx all'. This is why you may get output
		saying that a file couldn't be reloaded.

		PS: If you *do* get error spam trying to load a specific XML, try running
		'rx all' immediately and the spam might stop.

		Aliases: rx
		""";

	public override string GetHelp() => GetDescription();

	public override void Execute(
			List<string> _params,
			CommandSenderInfo _senderInfo)
	{
		if(_params.Count == 0){
			Log.Out(GetDescription());
			return;
		}

		if(_params.Count == 1 && _params[0].EqualsCaseInsensitive("all")){
			Log.Out("Reloading all xml...");
			WorldStaticData.ReloadAllXmlsSync();
			return;
		}

		object[] xmlsToLoad = Traverse.Create(typeof(WorldStaticData)).
			Field("xmlsToLoad").GetValue<object[]>();
		Traverse travXmlsToLoad = Traverse.Create(xmlsToLoad);
		List<string> notFound = new();
		List<string> noReloadMethod = new();
		bool itemsWarning = false;

		foreach(string fn in _params){
			string fnLower = fn.ToLower();

			if(fnLower is "loc"){
				Localization.ReloadBaseLocalization();
				GameManager.Instance.StartCoroutine(ModManager.LoadPatchStuff(false));
				continue;
			}

			if(fnLower is "items"){
				itemsWarning = true;
				continue;
			}

			if(fnLower is "ec" or "ecs" or "entityclasses" or "entity_classes"){
				WorldStaticData.Reset("entityclasses");
				continue;
			}

			if(fnLower is "blocks"){
				WorldStaticData.Reset("blocks");
				continue;
			}

			if(fnLower is "biome" or "biomes"){
				WorldStaticData.Reset("biomes");
				continue;
			}

			// if(fnLower is "items"){
			// 	WorldStaticData.Reset("items");
			// }

			bool found = false;

			foreach(object xmlLoadInfo in xmlsToLoad){
				Traverse trav = Traverse.Create(xmlLoadInfo);
				string name = trav.Field("XmlName").GetValue<string>();

				if(!name.EqualsCaseInsensitive(fn)){
					continue;
				}

				var reloadMethod = trav.Field("ReloadDuringGameMethod").GetValue<
					Action<XmlFile>>();

				if(reloadMethod is null){
					noReloadMethod.Add(name);
					found = true;
					break;
				}

				found = true;

				XmlFile? xmlFile = null;
				Log.Out($"Repatching XML '{fn}'...");
				ThreadManager.RunCoroutineSync(XmlPatcher.LoadAndPatchConfig(name,
					_file => xmlFile = _file));
				Log.Out($"Reloading XML '{fn}'...");

				if(xmlFile is not null){
					reloadMethod(xmlFile);
				}

				break;
			}

			if(!found){
				notFound.Add(fn);
			}
		}

		// Try to reload mod localisations.
		Traverse.Create(typeof(ModManager)).Method("LoadLocalizations", true).GetValue();

		foreach(var str in notFound){
			Log.Error($"Failed to find XML '{str}'.");
		}

		foreach(var str in noReloadMethod){
			Log.Warning($"Couldn't reload XML '{str}' because it has no method for reloading during the game.");
		}

		if(itemsWarning){
			Log.Warning("Even though 'items' has a method for reloading during the game, the method causes error spam. Please reload all if you want to reload items.");
		}
	}

	/// <summary>
	/// Attempt #1 at stopping error spam on Items reload.
	/// </summary>
	// [HarmonyPatch(typeof(EntityPlayerLocal), "SetMoveState")]
	// static class Patches__EntityPlayerLocal__SetMoveState
	// {
	// 	static bool Prefix(Inventory ___inventory) =>
	// 		___inventory?.holdingItem?.HoldType?.Value is not null;
	// }

	/// <summary>
	/// Attempt #2
	/// </summary>
	// [HarmonyPatch(typeof(Inventory), "ModifyValue")]
	// static class Patches__Inventory__ModifyValue
	// {
	// 	static bool Prefix(Inventory __instance)
	// 	{
	// 		var val = __instance.holdingItemItemValue;
	// 		return val?.ItemClass?.ItemTags is {};
	// 	}
	// }
}