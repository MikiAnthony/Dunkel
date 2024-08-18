using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Computer_InteractiveSynced : Interactive
{
    [SerializeField] private Transform _cameraTarget;
    [SerializeField] private Collider _interactionCollider;
    
    private InsideOS _insideOS;
    
    protected override void Awake()
    {
        base.Awake();
        _insideOS = GetComponent<InsideOS>();
    }

    public override async UniTask<bool> Use(Vector3 interactionHitWorldPos, PlayerInteraction playerInteraction)
    {
        playerInteraction.StartFullScreenInteraction(_cameraTarget.position, _cameraTarget.eulerAngles, _insideOS.MouseCursorUpdate, _insideOS.MouseActionUpdate, PlayerAbortInteraction);
        return true;
    }

    public void PlayerAbortInteraction(PlayerInteraction playerInteraction)
    {
        playerInteraction.EndFullScreenInteraction();
    }
}
