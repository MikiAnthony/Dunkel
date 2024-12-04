using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DectectiveBoard_Interactive : Interactive
{
    [SerializeField] private Transform _cameraTarget;
    [SerializeField] private Collider _interactionCollider;

    private DectectiveBoard _dectectiveBoard;

    protected override void Awake()
    {
        base.Awake();
        _dectectiveBoard = GetComponent<DectectiveBoard>();
    }

    public override async UniTask<bool> Use(Vector3 interactionHitWorldPos, PlayerInteraction playerInteraction)
    {
        playerInteraction.StartFullScreenInteraction(_cameraTarget.position, _cameraTarget.eulerAngles, null, _dectectiveBoard.MouseActionUpdate, PlayerAbortInteraction);
        playerInteraction.SetInputCursorState(false);
        return true;
    }

    public void PlayerAbortInteraction(PlayerInteraction playerInteraction)
    {
        playerInteraction.EndFullScreenInteraction();
        playerInteraction.SetInputCursorState(true);
    }
}
