using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

namespace _28AL.ParticleSystems;

[Preserve]
public class Attach : MinEventActionTargetedBase
{
	string? particle;
	bool shapeToMesh = false;

	public override void Execute(MinEventParams _params)
	{
		EntityAlive? target = null;

		if(targetType == TargetTypes.self){
			target = _params.Self;
		} else if(targetType == TargetTypes.other){
			target = _params.Other;
		}

		if(target is null){
			return;
		}

		EntityAlive inst = _params.Self;
		string parentTransform = "LOD0";
		Vector3 localOffset = Vector3.zero;
		Vector3 localRotation = Vector3.zero;
		var connManager = SingletonMonoBehaviour<ConnectionManager>.Instance;

		if(!GameManager.IsDedicatedServer){
			target.AttachParticle(
				DataLoader.LoadAsset<GameObject>(particle), "LOD0", Vector3.zero,
					Vector3.up, shapeToMesh, false);
		}

		if(connManager.IsServer){
			connManager.SendPackage(
				NetPackageManager.GetPackage<NetPackageAddRemoveBuffParticles>().
				SetupAdd(inst.entityId,
					parentTransform, localOffset, localRotation, shapeToMesh, particle, 
						false),
				_attachedToEntityId: inst.entityId);
			return;
		}

		connManager.SendToServer(
			NetPackageManager.GetPackage<NetPackageAddRemoveBuffParticles>().
			SetupAdd(inst.entityId,
				parentTransform, localOffset, localRotation, shapeToMesh, particle, false));
	}

	public override bool CanExecute(
			MinEventTypes _eventType,
			MinEventParams _params) =>
		base.CanExecute(_eventType, _params)
			&& particle != null;

	public override bool ParseXmlAttribute(XAttribute _attribute)
	{
		if(_attribute.Name.LocalName == "particle"){
			particle = _attribute.Value;
			return true;
		}

		if(_attribute.Name.LocalName == "shape_to_mesh"){
			shapeToMesh = bool.Parse(_attribute.Value);
			return true;
		}

		return base.ParseXmlAttribute(_attribute);
	}
}
