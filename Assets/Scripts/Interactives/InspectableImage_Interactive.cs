using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class InspectableImage_Interactive : Interactive
{
    public Sprite image;
    protected override void Awake()
    {
        base.Awake();
        _defaultInteractionType = InteractionType.Pickup;
    }

    public override async UniTask<bool> Use(Vector3 interactionHitWorldPos, PlayerInteraction playerInteraction)
    {
        OnUsed?.Invoke(this, playerInteraction);
        playerInteraction.InspectImageInteraction(image, PlayerAbortInteraction);
        return true;
    }

    public override void Release(PlayerInteraction playerInteraction)
    {
        base.Release(playerInteraction);
    }

    public void PlayerAbortInteraction(PlayerInteraction playerInteraction)
    {
        playerInteraction.EndInspectImageInteraction();
    }
}
