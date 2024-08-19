using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    public bool LookBlocked = false;
    public delegate void MouseUpdate(Vector2 delta);
    public event MouseUpdate OnMouseUpdate;
    
    [SerializeField] private float _interactionRange = 5f;
    [SerializeField] private InteractionIcons[] _interactionIcons;
    [SerializeField] private Texture2D _defaultCursorDot;
    [SerializeField] private Camera _playerCamera = default;
    [SerializeField] private TextMeshProUGUI _cursorText;

    [SerializeField] private float _fullScreenInteractionCameraTransitionTime = 1f;
    
    private PlayerInput _playerInput = default;
    private Interactive _interactionTarget = null;
    private Interactive.InteractionType _currentInteraction = Interactive.InteractionType.Press;
    private Int32 _interactionId = 0;
    private InputAction _interactAction = null;
    private InputAction _useAction = null;
    private InputAction _lookAction = null;
    private InputAction _mouseAction = null;
    private Int32 _guid = 0;
    private bool _isHoldingInteractive = false;
    private bool _grabbingThings = false;
    private Grabbable_Interactive _grabbedInteractive = null;
    private Quaternion _grabbedRotation;
    private Vector3 _interactionHitPosition;
    private Vector3 _grabbedObjectOffset = default;
    private readonly Dictionary<Interactive.InteractionType, Texture2D> _interactionCursorIcons = new();
    public GameObject _whiteboard = null;
    private FirstPersonController _firstPersonController = null;
    
   //Interaction stuff
    public enum MouseAction
    {
        Pressed,
        Released,
        Drag
    };

    private bool _playerLocked = false;
    private bool PlayerLocked
    {
        get => _playerLocked;
        set
        {
            _playerLocked = value;
            _firstPersonController.enabled = !value;
        }
    }

    private Action<Vector2> _mouseUpdateSubscription = null;
    private Action<MouseAction> _mouseActionSubscription = null;
    private Action<PlayerInteraction> _abortAction = null;
    private bool _transitioningToFullScreenMode = false;
    private bool _transitioningOutOfFullScreenMode = false;
    private Vector3 _playerCameraOrgPosition = default;
    private Vector3 _playerCameraOrgRotation = default;
    
    [Serializable]
    public struct InteractionIcons
    {
        public Interactive.InteractionType InteractionType;
        public Texture2D CursorIcon;
    }

    void Awake()
    {
        if (!enabled) return; //I am not auth of this player
        _firstPersonController = GetComponent<FirstPersonController>();
        _playerCameraOrgRotation = _playerCamera.transform.localEulerAngles;
        _playerCameraOrgPosition = _playerCamera.transform.localPosition;
        _playerInput = GetComponent<PlayerInput>();
        _interactAction = _playerInput.currentActionMap.FindAction("Interact", true);
        _useAction = _playerInput.currentActionMap.FindAction("Use", true);
        Cursor.SetCursor(_defaultCursorDot, Vector2.zero, CursorMode.ForceSoftware);
        _cursorText.text = "";
        for (var i = 0; i < _interactionIcons.Length; i++)
        {
            _interactionCursorIcons.Add(_interactionIcons[i].InteractionType, _interactionIcons[i].CursorIcon);
        }
    }
    
    void Update()
    {
        if (PlayerLocked)
        {
            if (_interactionTarget != null)
            {
                _interactionTarget.Deselect();
                Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
                _cursorText.text = "";
                _interactionTarget = null;
            }
            return;
        }
        
        if (!_grabbingThings && _isHoldingInteractive)
        {
            if (_interactAction.WasReleasedThisFrame())
            {
                _interactionTarget?.Release(this);
                _interactionTarget?.Deselect();
                _interactionTarget = null;
                _isHoldingInteractive = false;
                Cursor.SetCursor(_defaultCursorDot, Vector2.zero, CursorMode.ForceSoftware);
                _cursorText.text = "";
            }

            return;
        }
        if (_grabbingThings)
        {
            if (_interactAction.WasPressedThisFrame())
            {
                _interactionTarget?.Release(this);
                _interactionTarget?.Deselect();
                _interactionTarget = null;
                _isHoldingInteractive = false;
                _grabbingThings = false;
                _grabbedInteractive = null;
            }

            return;
        }

        bool rayTarget = false;
        if (Physics.Raycast(_playerCamera.transform.position, _playerCamera.transform.forward, out var rayHit,_interactionRange, LayerMask.GetMask("Interaction")))
        {
            var newTarget = rayHit.transform.GetComponent<Interactive>();
            if (newTarget != null && !newTarget.Blocked && rayHit.transform.GetInstanceID() != _interactionId)
            {
                rayTarget = true;
                if (_interactionTarget != null)
                {
                    _interactionTarget.Deselect();
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
                    _cursorText.text = "";
                }

                _interactionTarget = newTarget;
                _interactionTarget.Highlight();
                _interactionHitPosition = rayHit.point;
                _currentInteraction = _interactionTarget.LookAt(rayHit.point);
                Texture2D cursorIcon = _interactionCursorIcons[_currentInteraction];
                Cursor.SetCursor(cursorIcon, new Vector2(cursorIcon.width/2f, cursorIcon.height/2f), CursorMode.ForceSoftware);
                _cursorText.text = _currentInteraction.ToString() + " : [E]";
            }
        }
        if (!rayTarget)
        {
            if (_interactionTarget != null)
            {
                _interactionTarget.Deselect();
                Cursor.SetCursor(_defaultCursorDot, Vector2.zero, CursorMode.ForceSoftware);
                _cursorText.text = "";
            }
            _interactionTarget = null;
        }

        if (_interactionTarget != null && _interactAction.WasPressedThisFrame())
        {
            _interactionTarget.Use(_interactionHitPosition, this);
            _isHoldingInteractive = true;

            if (_interactionTarget is Grabbable_Interactive grabbableInteractive)
            {
                _grabbingThings = true;
                _grabbedInteractive = grabbableInteractive;
                _grabbedRotation = Quaternion.Inverse(transform.rotation) * _grabbedInteractive.transform.rotation;
                _grabbedObjectOffset = grabbableInteractive._grabbedObjectOffset;
            }
        }
    }
    
    public void OnLook(InputValue value)
    {
        OnMouseUpdate?.Invoke(value.Get<Vector2>());
        _mouseUpdateSubscription?.Invoke(value.Get<Vector2>());
    }

    public void OnAbort(InputValue value)
    {
        if (value.isPressed)
            _abortAction?.Invoke(this);
    }

    private void LateUpdate()
    {
        if (PlayerLocked)
        {
            if (_mouseActionSubscription != null)
            {
                if (_useAction.WasPressedThisFrame())
                    _mouseActionSubscription.Invoke(MouseAction.Pressed);
                else if (_useAction.IsInProgress())
                    _mouseActionSubscription.Invoke(MouseAction.Drag);
                else if (_useAction.WasReleasedThisFrame())
                    _mouseActionSubscription.Invoke(MouseAction.Released);
            }
            return;
        }
        if (_grabbingThings)
        {
            if (!_grabbedInteractive._isInsideGuidingObject)
            {
                Quaternion grabbedObjectRotation = _grabbedInteractive._lookAtObject ? transform.rotation * Quaternion.Euler(0, 0, 0) : transform.rotation * _grabbedRotation;
                _grabbedInteractive.transform.SetPositionAndRotation(_playerCamera.transform.position + _playerCamera.transform.rotation * _grabbedObjectOffset, grabbedObjectRotation);
            }
            else if(_grabbedInteractive._isInsideGuidingObject && _grabbedInteractive._guidingObjectCollidedWith != null)
            {
                _grabbedInteractive.transform.SetPositionAndRotation(_grabbedInteractive._guidingObjectCollidedWith.transform.position, _grabbedInteractive._guidingObjectCollidedWith.transform.rotation);
            }
            if (_currentInteraction == Interactive.InteractionType.Grab)
            {
                Texture2D cursorIcon = _interactionCursorIcons[Interactive.InteractionType.Grabbing];
                Cursor.SetCursor(cursorIcon, new Vector2(cursorIcon.width / 2f, cursorIcon.height / 2f), CursorMode.ForceSoftware);
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
            }
            _cursorText.text = "";

            if (_grabbedInteractive is Pen_Interaction penInteraction)
            {
                if (Physics.Raycast(penInteraction.transform.position, penInteraction.transform.forward, out var RayHit, 0.4f))
                {
                    int boardID = 0;

                    if (RayHit.collider.tag == "Whiteboard")
                    {
                        if (_useAction.IsPressed())
                        {
                            _whiteboard = RayHit.collider.gameObject;

                            boardID = _whiteboard.transform.parent.GetComponent<Whiteboard>().GetBoardID(_whiteboard.name);

                            _whiteboard.transform.parent.GetComponent<Whiteboard>().ToggleTouch(true, boardID);
                            _whiteboard.transform.parent.GetComponent<Whiteboard>().SetTouchingPosition(RayHit.textureCoord.x, RayHit.textureCoord.y, boardID);
                            _whiteboard.transform.parent.GetComponent<Whiteboard>().SetIsEraser(false, boardID);
                            _whiteboard.transform.parent.GetComponent<Whiteboard>().SetColor(Color.black, boardID);

                        }
                        else
                        {
                            if (_whiteboard != null)
                            {
                                boardID = _whiteboard.transform.parent.GetComponent<Whiteboard>().GetBoardID(_whiteboard.name);

                                _whiteboard.transform.parent.GetComponent<Whiteboard>().ToggleTouch(false, boardID);
                            }
                        }

                    }
                }
                else
                {
                    /*
                    if(_whiteboard != null)
                    {
                        if (sync == null)
                            sync = _whiteboard.transform.parent.GetComponent<CoherenceSync>();

                        sync.SendCommandToChildren<Whiteboard>("ToggleTouch", MessageTarget.All,
                        (typeof(bool), (bool)false));
                        //_whiteboard.ToggleTouch(false);
                        //_whiteboard = null;
                    }
                    */
                }
            }

            if (_grabbedInteractive is Eraser_Interaction eraserInteraction)
            {
                if (Physics.Raycast(eraserInteraction.transform.position, eraserInteraction.transform.forward, out var RayHit, 0.4f))
                {
                    int boardID = 0;

                    if (RayHit.collider.tag == "Whiteboard")
                    {
                        if (_useAction.IsPressed())
                        {
                            _whiteboard = RayHit.collider.gameObject;

                            boardID = _whiteboard.transform.parent.GetComponent<Whiteboard>().GetBoardID(_whiteboard.name);

                            _whiteboard.transform.parent.GetComponent<Whiteboard>().ToggleTouch(true, boardID);
                            _whiteboard.transform.parent.GetComponent<Whiteboard>().SetTouchingPosition(RayHit.textureCoord.x, RayHit.textureCoord.y, boardID);
                            _whiteboard.transform.parent.GetComponent<Whiteboard>().SetIsEraser(true, boardID);
                            _whiteboard.transform.parent.GetComponent<Whiteboard>().SetColor(Color.white, boardID);
                        }
                        else
                        {
                            if (_whiteboard != null)
                            {
                                boardID = _whiteboard.transform.parent.GetComponent<Whiteboard>().GetBoardID(_whiteboard.name);

                                _whiteboard.transform.parent.GetComponent<Whiteboard>().ToggleTouch(false, boardID);
                            }
                        }
                    }
                }
                else
                {
                    /*
                    if (_whiteboard != null)
                    {
                        if (sync == null)
                            sync = _whiteboard.transform.parent.GetComponent<CoherenceSync>();

                        sync.SendCommandToChildren<Whiteboard>("ToggleTouch", MessageTarget.All,
                        (typeof(bool), (bool)false));
                        //_whiteboard.ToggleTouch(false);
                        //_whiteboard = null;
                    }
                    */
                }
            }
        }
    }

    public void StartFullScreenInteraction(Vector3 cameraPosition, Vector3 cameraRotation, Action<Vector2> mouseUpdate, Action<MouseAction> mouseClick, Action<PlayerInteraction> abortAction)
    {
        if (_transitioningToFullScreenMode || _transitioningOutOfFullScreenMode) return;
        PlayerLocked = true;
        _mouseUpdateSubscription = mouseUpdate;
        _mouseActionSubscription = mouseClick;
        _abortAction = abortAction;
        _transitioningToFullScreenMode = true;

        LeanTween.move(_playerCamera.gameObject, cameraPosition, _fullScreenInteractionCameraTransitionTime).setOnComplete(() => { _transitioningToFullScreenMode = false;});
        LeanTween.rotate(_playerCamera.gameObject, cameraRotation, _fullScreenInteractionCameraTransitionTime);
    }

    public bool EndFullScreenInteraction()
    {
        if (_transitioningToFullScreenMode || _transitioningOutOfFullScreenMode) return false;
        _mouseUpdateSubscription = null;
        _mouseActionSubscription = null;
        _abortAction = null;
        _transitioningOutOfFullScreenMode = true;
        LeanTween.moveLocal(_playerCamera.gameObject, _playerCameraOrgPosition, _fullScreenInteractionCameraTransitionTime).setOnComplete(() =>
        {
            PlayerLocked = false;
            _transitioningOutOfFullScreenMode = false;
            _isHoldingInteractive = false;
        });
        LeanTween.rotateLocal(_playerCamera.gameObject, _playerCameraOrgRotation, _fullScreenInteractionCameraTransitionTime);
        return true;
    }

}
