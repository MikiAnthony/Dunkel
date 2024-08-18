using cakeslice;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class Interactive : MonoBehaviour
{
	public enum InteractionType
	{
		Press,
		Grab,
		Grabbing,
		Look,
		Pickup,
		TurnClockwise,
		TurnCounterClockwise,
		Pull,
		Push
	}
	
	public UnityAction<Interactive, PlayerInteraction> OnUsed;
	public UnityAction<Interactive, PlayerInteraction> OnReleased;

	[HideInInspector] public bool Blocked = false;

    [SerializeField] protected Renderer _renderer = default;
	
	protected InteractionType _defaultInteractionType = InteractionType.Press;
	protected Outline _outline = null;

	protected virtual void Awake()
	{
		if (_renderer is not null)
		{
			_outline = _renderer.gameObject.AddComponent<Outline>();
			_outline.enabled = false;
		}
    }

	protected virtual void Start() {}

	protected virtual void Update() {}

	public virtual void Highlight()
	{
		if (_outline is not null)
	        _outline.enabled = true;
	}
	
	public virtual void Deselect()
	{
		if (_outline is not null)
			_outline.enabled = false;
	}
    public virtual UniTask<bool> Use(Vector3 interactionHitWorldPos, PlayerInteraction playerInteraction)
    {
        OnUsed?.Invoke(this, playerInteraction);
        return UniTask.FromResult(true);
	}

    public virtual void Release(PlayerInteraction playerInteraction)
    {
	    OnReleased?.Invoke(this, playerInteraction);
    }

    public virtual InteractionType LookAt(Vector3 interactionHitWorldPos)
    {
	    return _defaultInteractionType;
    }
}
