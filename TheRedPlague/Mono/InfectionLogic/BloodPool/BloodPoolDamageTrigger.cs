using System.Collections.Generic;
using UnityEngine;

namespace TheRedPlague.Mono.InfectionLogic.BloodPool;

public class BloodPoolDamageTrigger : MonoBehaviour, IManagedUpdateBehaviour
{
	private readonly List<LiveMixin> _targets = new();

	private int _currentIndex;

	public BoxCollider box;

	public int managedUpdateIndex { get; set; }

	public string GetProfileTag()
	{
		return "BloodPoolDamageTrigger";
	}

	private void OnDisable()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	private void OnDestroy()
	{
		BehaviourUpdateUtils.Deregister(this);
	}

	private bool IsValidTarget(LiveMixin liveMixin)
	{
		if (liveMixin == null)
		{
			return false;
		}
		if (!liveMixin.IsAlive())
		{
			return false;
		}
		var player = liveMixin.GetComponent<Player>();
		if (player == null)
		{
			return false;
		}

		return player.currentSub != null && !player.inSeamoth && !player.inExosuit;
	}

	private bool Contains(Vector3 point)
	{
		var vector = box.transform.InverseTransformPoint(point);
		var v = box.center - vector;
		var vector2 = box.size * 0.5f + Vector3.one;
		return v.InBox(-vector2, vector2);
	}

	private void RemoveTarget(LiveMixin target)
	{
		if (_targets.Contains(target))
		{
			var component = target.GetComponent<BloodPoolDamageTarget>();
			if (component != null)
			{
				component.Decrement();
			}
			_targets.Remove(target);
			RequestUpdateIfNecessary();
		}
	}

	private void AddTarget(LiveMixin target)
	{
		if (!_targets.Contains(target))
		{
			var acidicBrineDamage = target.GetComponent<BloodPoolDamageTarget>();
			if (acidicBrineDamage == null)
			{
				acidicBrineDamage = target.gameObject.AddComponent<BloodPoolDamageTarget>();
			}
			acidicBrineDamage.Increment();
			_targets.Add(target);
			RequestUpdateIfNecessary();
		}
	}

	private LiveMixin GetLiveMixin(GameObject go)
	{
		var gameObject = UWE.Utils.GetEntityRoot(go);
		if (gameObject == null)
		{
			gameObject = go;
		}
		return gameObject.GetComponentInChildren<Player>() == null ? null : Player.main.liveMixin;
	}

	private void OnTriggerEnter(Collider other)
	{
		var liveMixin = GetLiveMixin(other.gameObject);
		if (IsValidTarget(liveMixin))
		{
			AddTarget(liveMixin);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		var liveMixin = GetLiveMixin(other.gameObject);
		if (liveMixin)
		{
			RemoveTarget(liveMixin);
		}
	}

	public void ManagedUpdate()
	{
		if (_targets.Count <= 0)
		{
			return;
		}
		if (_currentIndex >= _targets.Count)
		{
			_currentIndex = 0;
		}
		if (_targets[_currentIndex] == null)
		{
			_targets.RemoveAt(_currentIndex);
			return;
		}
		var position = _targets[_currentIndex].transform.position;
		if (!Contains(position) || !IsValidTarget(_targets[_currentIndex]))
		{
			RemoveTarget(_targets[_currentIndex]);
		}
		else
		{
			_currentIndex++;
		}
	}

	private void RequestUpdateIfNecessary()
	{
		if (_targets.Count != 0)
		{
			BehaviourUpdateUtils.Register(this);
		}
		else
		{
			BehaviourUpdateUtils.Deregister(this);
		}
	}
}