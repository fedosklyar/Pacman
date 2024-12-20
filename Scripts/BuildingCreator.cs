using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class BuildingCreator : Singleton<BuildingCreator>
{
    [SerializeField] Tilemap previewMap, defaultMap;

    TileBase tileBase;
    PlayerInput playerInput;

    BuildingObjectBase selectedObj;

    Camera _camera;
    Vector2 mousePos;

    Vector3Int currentGridPosition;
    Vector3Int lastGridPosition;

    protected override void Awake()
    {
        base.Awake();
        playerInput = new PlayerInput();
        _camera = Camera.main;
    }

    

    private void OnEnable()
    {
        playerInput.Enable();

        playerInput.Gameplay.MousePosition.performed += OnMouseMove;
        playerInput.Gameplay.MouseLeftClick.performed += OnLeftClick;
        playerInput.Gameplay.MouseRightClick.performed += OnRightClick;
    }

    private void OnDisable()
    {
        playerInput.Disable();

        playerInput.Gameplay.MousePosition.performed -= OnMouseMove;
        playerInput.Gameplay.MouseLeftClick.performed -= OnLeftClick;
        playerInput.Gameplay.MouseRightClick.performed -= OnRightClick;
    }

    private BuildingObjectBase SelectedObj
    {
        set
        {
            selectedObj = value;

            tileBase = selectedObj != null ? selectedObj.TileBase : null;

            UpdatePreview();
        }
    }

    private void Update()
    {
        //if something is selecred  - show preview
        if(selectedObj != null)
        {    
            Vector3 pos = _camera.ScreenToWorldPoint (mousePos);
            Vector3Int gridPos = previewMap.WorldToCell (pos);

            if (gridPos != currentGridPosition)
            {
                lastGridPosition = currentGridPosition;
                currentGridPosition = gridPos;

                UpdatePreview();
                // update preview
            }
        }
    }


    private void OnMouseMove(InputAction.CallbackContext ctx)
    {
        //Debug.Log("Mouse Moved");
        mousePos = ctx.ReadValue<Vector2>();
    }

    //Also some exceptions...
    private void OnLeftClick(InputAction.CallbackContext ctx)
    {
        //added the condition with performed
        if (ctx.performed && selectedObj != null && !EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Left clicked");
            Debug.Log(currentGridPosition);
            HandleDrawing();
        }
    }

    //Do not work properly, for some reason
    private void OnRightClick(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Debug.Log("Right clicked");
            SelectedObj = null;
        }
    }

    public void ObjectSelected(BuildingObjectBase obj)
    {
        SelectedObj = obj;

        // Set preview where the mouse is
        // on click draw
        //on right click cancel drawing
    }
    private void UpdatePreview()
    {
        // Remove old tile if existing

        previewMap.SetTile(lastGridPosition, null);

        // Set current tile to current mouse position tile

        previewMap.SetTile(currentGridPosition, tileBase);
    }

    private void HandleDrawing ()
    {
        DrawItem();
    }

    private void DrawItem()
    {
        defaultMap.SetTile(currentGridPosition, tileBase);
    }
}


