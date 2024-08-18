public class Init : IModApi
{
	public static Mod? ModInstance; 
	public static Harmony? HarmonyInstance;

	public void InitMod(Mod _modInstance)
	{
		ModInstance = _modInstance;
		HarmonyInstance = new(_modInstance.Name + "_" + 
			_modInstance.VersionString.Replace('.', '-'));
		HarmonyInstance.PatchAll(typeof(Init).Assembly);
	}
}
