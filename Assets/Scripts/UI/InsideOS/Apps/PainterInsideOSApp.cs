using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PainterInsideOSApp : InsideOSApp
{
    
    public enum PainterColor : byte
    {
        White = 0,
        Black = 1,
        Red = 2,
        Yellow = 3,
        Green = 4,
        Cyan = 5,
        Blue = 6,
        Purple = 7
    }

    private enum BrushShape
    {
        Circle = 0,
        Square = 1,
        Triangle = 2
    }

    private enum BrushTool
    {
        Pencil = 0,
        Eraser = 1
    }
    
    [SerializeField] private int _textureWidth = 128;
    [SerializeField] private int _textureHeight = 128;
    [SerializeField] private Vector2 _painterOffset;
    [SerializeField] private RectTransform _drawRegion;
    [SerializeField] private RawImage _drawTexture;
    [SerializeField] private Image _colorPickerCurrentColor;
    [SerializeField] private int _maxUndos = 12;
    
    [Separator("Brush")]
    [SerializeField] private TextMeshProUGUI _brushSizeText = null;
    [SerializeField] private RectTransform _brushSizeDotTransform;
    [SerializeField] private int _maxBrushSize = 32;
    [SerializeField] private Image[] _shapeSelectors = null;
    [SerializeField] private Image[] _shapeIcons = null;
    [SerializeField] private Image[] _toolIcons = null;
    
    private Texture2D _texture;
    private byte[,] _pixels = null;
    private Vector2Int _lastPixel, _newPixel = default;
    private bool _ready = false;
    private bool _cursorOutside = false;
    private Color _currentColor = Color.black;
    private byte _currentColorValue = 0;
    private int _currentBrushSize = 3;
    private BrushShape _currentBrushShape = BrushShape.Circle;
    private BrushTool _currentBrushTool = BrushTool.Pencil;
    private readonly List<byte[,]> _states = new List<byte[,]>();
    private bool _drewSomething = false;
    private int _currentStateIndex = 0;

    public override async void InitializeApp()
    {
        // Create and initialize the texture with RGBA32 format for full color
        _texture = new Texture2D(_textureWidth, _textureHeight, TextureFormat.RGBA32, false);
        _texture.filterMode = FilterMode.Point;
        // Initialize the pixel array
        _pixels = new byte[_textureWidth, _textureHeight];
        
        // Assign the texture to the RawImage component for display
        if (_drawTexture != null)
        {
            _drawTexture.texture = _texture;
        }
        
        ClearTexture(PainterColor.White);
        SetColor(PainterColor.Black);
        SetBrushShape((int)BrushShape.Circle);
        SetBrushSize(3);
        SetBrushTool(BrushTool.Pencil);
        
        //_states.Add(_pixels.Copy());

        await UniTask.WaitForEndOfFrame(this);
        _ready = true;
    }

    public override void UpdateApp(Vector2 cursorPosition)
    {
        if (!_ready) return;

        cursorPosition += _painterOffset;

        Color currentColor = _currentBrushTool == BrushTool.Pencil ? _currentColor : Color.white;
        byte currentColorValue = _currentBrushTool == BrushTool.Pencil ? _currentColorValue : (byte)0;
        
        if (Input.GetMouseButton(0)) // Check if the left mouse button is pressed
        {
            if (!IsCursorInsideDrawArea(cursorPosition))
            {
                _cursorOutside = true;
                return;
            }
            
            // Convert the local mouse position to texture coordinates
            float pivotOffsetX = _drawRegion.rect.width * 0.5f;
            float pivotOffsetY = _drawRegion.rect.height * 0.5f;

            float normalizedX = Mathf.Clamp01((cursorPosition.x + pivotOffsetX) / _drawRegion.rect.width);
            float normalizedY = Mathf.Clamp01((cursorPosition.y + pivotOffsetY) / _drawRegion.rect.height);

            int x = Mathf.CeilToInt(normalizedX * _textureWidth) ;
            int y = Mathf.CeilToInt(normalizedY * _textureHeight) ;

            // Ensure coordinates are within texture bounds
            x = Mathf.Clamp(x, 0, _textureWidth - 1);
            y = Mathf.Clamp(y, 0, _textureHeight - 1);

            if (Input.GetMouseButtonDown(0) || _cursorOutside)
            {
                _cursorOutside = false;
                _lastPixel.x = x;
                _lastPixel.y = y;
                //Don't draw on first pixel
                return;
            }

            _newPixel.x = x;
            _newPixel.y = y;

            var pixels = GetLineCoordinates(_lastPixel, _newPixel, _currentBrushSize);
            _lastPixel = _newPixel;
            
            //Remove pixels that are already the right color
            for (var i = 0; i < pixels.Count; i++)
            {
                var pixel = pixels[i];
                //Is pixel outside of bounds or already the correct color
                if (pixel.x < 0 || pixel.x > _pixels.GetUpperBound(0)
                    || pixel.y < 0 || pixel.y > _pixels.GetUpperBound(1)
                    || _pixels[pixel.x, pixel.y] == currentColorValue)
                {
                    pixels.RemoveAt(i);
                    i--;
                }
            }
            
            Color[] colors = _texture.GetPixels();
            for (int i = 0; i < pixels.Count; i++)
            {
                var pixel = pixels[i];
                int pixelIndex = pixel.y * _textureWidth + pixel.x;
                colors[pixelIndex] = currentColor;
                _pixels[pixel.x, pixel.y] = currentColorValue;
                _drewSomething = true;
            }

            _texture.SetPixels(colors);
            _texture.Apply();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (!_drewSomething) return;
            _drewSomething = false;
            

            if (_currentStateIndex != _states.Count - 1)
            {
                _states.Clear();
            }
            
            //_states.Add(_pixels.Copy());
            if (_states.Count >= _maxUndos)
                _states.RemoveAt(0);
            _currentStateIndex = _states.Count - 1;
        }
    }

    private void SetPixels(byte[,] pixels)
    {
        var currentTotal = 0;
        for (int x = 0; x < _pixels.GetLength(0); x++)
        {
            for (int y = 0; y < _pixels.GetLength(1); y++)
            {
                currentTotal += _pixels[x, y];
            }
        }

        var newTotal = 0;
        Color[] colors = _texture.GetPixels();
        for (int x = 0; x < pixels.GetLength(0); x++)
        {
            for (int y = 0; y < pixels.GetLength(1); y++)
            {
                var pixel = pixels[x, y];
                newTotal += pixel;
                int pixelIndex = y * _textureWidth + x;
                colors[pixelIndex] = GetColor((PainterColor)pixel);
                _pixels[x, y] = pixel;
            }
        }
        
        _texture.SetPixels(colors);
        _texture.Apply();
    }
    
    public override void Close()
    {
        base.Close();
        if (_texture != null)
        {
            Destroy(_texture);
            _texture = null;
        }
    }

    public void PrintDrawing()
    {
        //_insideOS.PrintDrawingOnPaper(_pixels);
    }

    public void ClearTexture()
    {
        ClearTexture(PainterColor.White);
        _states.Clear();
        //_states.Add(_pixels.Copy());
        _currentStateIndex = 0;
    }

    public void SetColor(int color)
    {
        SetColor((PainterColor)color);
    }
    
    public void SetColor(PainterColor color)
    {
        _currentColor = GetColor(color);
        _colorPickerCurrentColor.color = _currentColor;
        _currentColorValue = (byte)color;
    }

    public void BrushSizeUp()
    {
        if (_currentBrushSize >= _maxBrushSize) return;
        SetBrushSize(_currentBrushSize + 1);
    }

    public void BrushSizeDown()
    {
        if (_currentBrushSize <= 1) return;
        SetBrushSize(_currentBrushSize - 1);
    }

    private void SetBrushSize(int size)
    {
        _currentBrushSize = size;
        _brushSizeDotTransform.localScale = Vector3.one * ((float)_currentBrushSize / _maxBrushSize); 
        _brushSizeText.text = "Brush: " + _currentBrushSize;
    }

    public void SetBrushShape(int brushShapeIndex)
    {
        var brushShape = (BrushShape)brushShapeIndex;
        _currentBrushShape = brushShape;

        for (int i = 0; i < _shapeSelectors.Length; i++)
        {
            _shapeSelectors[i].color = Color.white;
            _shapeIcons[i].enabled = false;
        }

        _shapeIcons[brushShapeIndex].enabled = true;
        _shapeSelectors[brushShapeIndex].color = Color.green;
    }

    public void SetBrushTool(int brushToolIndex)
    {
        SetBrushTool((BrushTool)brushToolIndex);
    }

    public void Undo()
    {
        if (_states.Count <= 1 || _currentStateIndex < 1) return; //Not enough states to undo anything
        _currentStateIndex--;
        SetPixels(_states[_currentStateIndex]);
    }

    public void Redo()
    {
        if (_currentStateIndex == _states.Count - 1) return; //Can't redo anything
        _currentStateIndex++;
        SetPixels(_states[_currentStateIndex]); 
    }

    private void SetBrushTool(BrushTool brushTool)
    {
        _currentBrushTool = brushTool;
        
        for (var i = 0; i < _toolIcons.Length; i++)
        {
            _toolIcons[i].color = Color.white;
        } 
       
        _toolIcons[(int)brushTool].color = Color.green;
    }

    private void ClearTexture(PainterColor painterColor, bool setPixels = true)
    {
        var clearColor = GetColor(painterColor);
        Color[] colors = new Color[_textureWidth * _textureHeight];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = clearColor;
        }
        _texture.SetPixels(colors);
        _texture.Apply();

        if (!setPixels) return;
        for (var x = 0; x < _textureWidth; x++)
        {
            for (var y = 0; y < _textureHeight; y++)
            {
                _pixels[x, y] = (byte)painterColor; 
            }
        }
    }
    
    //Totally AI generated - don't look 🙈
    private List<Vector2Int> GetLineCoordinates(Vector2Int point1, Vector2Int point2, int brushSize)
    {
        List<Vector2Int> lineCoordinates = new List<Vector2Int>();

        int x0 = point1.x;
        int y0 = point1.y;
        int x1 = point2.x;
        int y1 = point2.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            AddBrushPixels(lineCoordinates, x0, y0, brushSize, _currentBrushShape);

            if (x0 == x1 && y0 == y1) break;

            int e2 = err * 2;

            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }

        return lineCoordinates;
    }

    private void AddBrushPixels(List<Vector2Int> coordinates, int x, int y, int brushSize, BrushShape shape)
    {
        switch (shape)
        {
            case BrushShape.Circle:
                AddCircleBrushPixels(coordinates, x, y, brushSize);
                break;
            case BrushShape.Square:
                AddSquareBrushPixels(coordinates, x, y, brushSize);
                break;
            case BrushShape.Triangle:
                AddTriangleBrushPixels(coordinates, x, y, brushSize);
                break;
        }
    }

    private void AddCircleBrushPixels(List<Vector2Int> coordinates, int centerX, int centerY, int brushSize)
    {
        float radius = brushSize / 2f;
        float radiusSquared = radius * radius;

        for (int y = -brushSize; y <= brushSize; y++)
        {
            for (int x = -brushSize; x <= brushSize; x++)
            {
                if (x * x + y * y <= radiusSquared)
                {
                    coordinates.Add(new Vector2Int(centerX + x, centerY + y));
                }
            }
        }
    }

    private void AddSquareBrushPixels(List<Vector2Int> coordinates, int centerX, int centerY, int brushSize)
    {
        int halfSize = brushSize / 2;
        for (int y = -halfSize; y <= halfSize; y++)
        {
            for (int x = -halfSize; x <= halfSize; x++)
            {
                coordinates.Add(new Vector2Int(centerX + x, centerY + y));
            }
        }
    }
    
    private void AddTriangleBrushPixels(List<Vector2Int> coordinates, int centerX, int centerY, int brushSize)
    {
        float halfHeight = (float)(Math.Sqrt(3) / 2 * brushSize);
        int halfSize = brushSize / 2;

        for (int y = 0; y <= halfHeight; y++)
        {
            int rowWidth = (int)(y / halfHeight * halfSize);
            for (int x = -rowWidth; x <= rowWidth; x++)
            {
                coordinates.Add(new Vector2Int(centerX + x, centerY - (int)(halfHeight / 2) + (int)halfHeight - y));
            }
        }
    }

    
    private bool IsCursorInsideDrawArea(Vector2 cursorPosition)
    {
        float minX = _drawRegion.anchoredPosition.x - _drawRegion.rect.width * 0.5f;
        float maxX = _drawRegion.anchoredPosition.x + _drawRegion.rect.width * 0.5f;
        float minY = _drawRegion.anchoredPosition.y - _drawRegion.rect.height * 0.5f;
        float maxY = _drawRegion.anchoredPosition.y + _drawRegion.rect.height * 0.5f; 

        return cursorPosition.x >= minX && cursorPosition.x <= maxX && cursorPosition.y >= minY && cursorPosition.y <= maxY;
    }
    
    
    public static Color GetColor(PainterColor painterColor)
    {
        switch (painterColor)
        {
            case PainterColor.White:
                return Color.white;
            case PainterColor.Black:
                return Color.black;
            case PainterColor.Red:
                return Color.red;
            case PainterColor.Yellow:
                return Color.yellow;
            case PainterColor.Green:
                return Color.green;
            case PainterColor.Cyan:
                return Color.cyan;
            case PainterColor.Blue:
                return Color.blue;
            case PainterColor.Purple:
                return Color.magenta;
        }
        return Color.white;
    }

}
