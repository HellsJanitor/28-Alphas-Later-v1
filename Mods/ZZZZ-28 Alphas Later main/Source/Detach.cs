using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

namespace _28AL.ParticleSystems;

[Preserve]
public class Detach : MinEventActionTargetedBase
{
	public override void Execute(MinEventParams _params)
	{
		EntityAlive? target = null;

		if(targetType == TargetTypes.self){
			target = _params.Self;
		} else if(targetType == TargetTypes.other){
			target = _params.Other;
		}

		if(target is null || peName is not string { Length: > 0 }){
			return;
		}

		GameManager.Instance.StartCoroutine(
			CoFadeOutParticleEffect(peName, _params, target));
	}

	public override bool CanExecute(
			MinEventTypes _eventType,
			MinEventParams _params) =>
		base.CanExecute(_eventType, _params)
			&& _params.Self != null
			&& peName != null;

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		if (_attribute.Name.LocalName == "particle"){
			peName = _attribute.Value;
			return true;
		}

		return base.ParseXmlAttribute(_attribute);
	}

	static IEnumerator CoFadeOutParticleEffect(
			string name,
			MinEventParams @params,
			EntityAlive target)
	{
		List<ParticleSystem> systems = new();

		if(target.GetComponentsInChildren<ParticleSystem>() is {} pSystems){
			foreach(var ps in pSystems.Where(a => a.gameObject.name == "Ptl_" + name)){
				ps.Stop();
				ps.transform.SetParent(null);
				systems.Add(ps);
			}
		}

		float timeout = Time.time + 5f;

		while(Time.time < timeout){
			for(int i = 0; i < systems.Count; i++){
				if(systems[i].particleCount == 0 && XUi.IsGameRunning()){
					if(SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer){
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
							NetPackageManager.GetPackage<NetPackageAddRemoveBuffParticles>().
								SetupRemove(target.entityId, name),
									false, -1, target.entityId, -1, -1);
					} else {
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
							NetPackageManager.GetPackage<NetPackageAddRemoveBuffParticles>().
								SetupRemove(target.entityId, name), false);
					}
				}
			}

			yield return null;
		}
	}

	string? peName;
}
