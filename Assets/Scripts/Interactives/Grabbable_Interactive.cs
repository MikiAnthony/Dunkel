using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Grabbable_Interactive : Interactive
{
	public Collider Collider;
	protected Rigidbody _rigidbody = null;
	public bool _lookAtObject;
    public bool _isInsideGuidingObject;
    public Vector3 _grabbedObjectOffset = new Vector3(0, 0, 1f);
	public GameObject[] _guidingObjects;
	public GameObject _guidingObjectCollidedWith;

    protected override void Awake()
	{
		base.Awake();
		_rigidbody = GetComponent<Rigidbody>();
		_defaultInteractionType = InteractionType.Pickup;
        _isInsideGuidingObject = false;
    }

    private void OnTriggerEnter(Collider other)
    {
		foreach (GameObject _guidingObject in _guidingObjects)
		{
			if (other.gameObject == _guidingObject)
			{
                _isInsideGuidingObject = true;
				_guidingObjectCollidedWith = other.gameObject;
            }
		}
    }

    private void OnTriggerExit(Collider other)
    {
        foreach (GameObject _guidingObject in _guidingObjects)
        {
            if (other.gameObject == _guidingObject)
            {
                _isInsideGuidingObject = false;
                _guidingObjectCollidedWith = null;
            }
        }
    }

	public override async UniTask<bool> Use(Vector3 interactionHitWorldPos, PlayerInteraction playerInteraction)
	{
		if(_guidingObjectCollidedWith != null && !_guidingObjectCollidedWith.transform.GetChild(0).gameObject.activeSelf)
		{
            _guidingObjectCollidedWith.transform.GetChild(0).gameObject.SetActive(true);
            _guidingObjectCollidedWith = null;
        }

		if(_isInsideGuidingObject && _guidingObjectCollidedWith == null)
		{
			_isInsideGuidingObject = false;
		}

		if (_rigidbody is not null)
			_rigidbody.isKinematic = true;

		//Collider.enabled = false;
		OnUsed?.Invoke(this, playerInteraction);
		return true;
	}

	public override void Release(PlayerInteraction playerInteraction)
	{
		base.Release(playerInteraction);
		if (_isInsideGuidingObject)
		{
			_guidingObjectCollidedWith.transform.GetChild(0).gameObject.SetActive(false);
            _isInsideGuidingObject = false;
        }
		else
		{
			if (_rigidbody is not null)
	            _rigidbody.isKinematic = false;
			if (Collider is not null)
	            Collider.enabled = true;
        }
	}
}
