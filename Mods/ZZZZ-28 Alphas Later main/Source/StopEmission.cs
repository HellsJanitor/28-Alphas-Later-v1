using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

namespace _28AL.ParticleSystems;

[Preserve]
public class StopEmission : MinEventActionTargetedBase
{
	string? peName;

	public override void Execute(MinEventParams _params)
	{
		if(_params.Self.GetComponentsInChildren<ParticleSystem>() is {} pSystems){
			foreach(var ps in pSystems.Where(a => a.gameObject.name == "Ptl_" + peName)){
				ps.Stop();
			}
		}
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
}
