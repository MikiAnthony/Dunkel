using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Button_Local : Interactive
{
    [SerializeField] private Transform _button = null;
    [SerializeField] private Transform _buttonDepressedPosition = null;
    [SerializeField] private float _animationTime = 0.5f;
    [SerializeField] private AnimationCurve _buttonCurve;
    
    private Vector3 _buttonUpPosition = Vector3.zero;
    private bool _goingDown = false;
    private float _currentAnimationValue = 0f;
    private bool _sleeping = true;

    protected override void Awake()
    {
        base.Awake();
        _buttonUpPosition = _button.localPosition;
    }

    protected override void Update()
    {
        base.Update();
        if (_sleeping) return; //To not update position every update if not necessary
        _currentAnimationValue = Mathf.Clamp01(_currentAnimationValue + Time.deltaTime * _animationTime * (_goingDown ? 1f : -1f));
        _button.localPosition = Vector3.Lerp(_buttonUpPosition, _buttonDepressedPosition.localPosition, _buttonCurve.Evaluate(_currentAnimationValue));
        if (_currentAnimationValue is <= 0f or >= 1f) 
            _sleeping = true;
    }

    public override async UniTask<bool> Use(Vector3 interactionHitWorldPos, PlayerInteraction playerInteraction)
    {
        if (!await base.Use(interactionHitWorldPos, playerInteraction)) return false;
        _goingDown = true;
        _sleeping = false;
        return true;
    }

    public override void Release(PlayerInteraction playerInteraction)
    {
        base.Release(playerInteraction);
        _goingDown = false;
        _sleeping = false;
    }
}
