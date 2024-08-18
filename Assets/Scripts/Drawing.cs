using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public class Drawing : MonoBehaviour
{
    private int textureSize = 2048;
    private int penSize = 10;
    private int eraserSizeWidth = 30;
    private int eraserSizeHeight = 250;
    private Texture2D texture;
    public Color[] color;

    private bool touchingLast;
    private bool touching;
    private float posX, posY;
    private float lastX, lastY;
    private bool isEraser;
    // Start is called before the first frame update
    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        this.texture = new Texture2D(textureSize, textureSize);
        Color fillColor = Color.white;
        var fillColorArray = texture.GetPixels();

        for (var i = 0; i < fillColorArray.Length; ++i)
        {
            fillColorArray[i] = fillColor;
        }

        texture.SetPixels(fillColorArray);

        texture.Apply();

        renderer.material.mainTexture = this.texture;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isEraser)
        {
            int x = (int)(posX * textureSize - (penSize / 2));
            int y = (int)(posY * textureSize - (penSize / 2));

            if (touchingLast)
            {
                if ((x > 0 && x < (textureSize - (penSize * 2))) && (y > 0 && y < (textureSize - penSize*2)))
                {
                    if (color.Length != (penSize * penSize))
                        SetColor(Color.black);

                    texture.SetPixels(x, y, penSize, penSize, color);

                    for (float t = 0.01f; t < 1.00f; t += 0.01f)
                    {
                        int lerpX = (int)Mathf.Lerp(lastX, (float)x, t);
                        int lerpY = (int)Mathf.Lerp(lastY, (float)y, t);

                        texture.SetPixels(lerpX, lerpY, penSize, penSize, color);
                    }
                    texture.Apply();
                }
            }

            if ((x > 0 && x < (textureSize - (penSize * 2))) && (y > 0 && y < (textureSize - penSize * 2)))
            {
                this.lastX = (float)x;
                this.lastY = (float)y;
            }
        }
        else if(isEraser)
        {
            int x = (int)(posX * textureSize - (eraserSizeWidth / 2));
            int y = (int)(posY * textureSize - (eraserSizeHeight / 2));
            int deltaX = 0;
            int deltaY = 0;

            if (touchingLast)
            {
                if ((y + eraserSizeHeight) >= textureSize)
                {
                    deltaY = ((y + eraserSizeHeight) - textureSize) * 2;
                }
                else if (y < 0)
                {
                    deltaY = y * 2;
                }

                if ((x + eraserSizeWidth) >= textureSize)
                {
                    deltaX = ((x + eraserSizeWidth) - textureSize) * 2;
                }
                else if(x < 0)
                {
                    deltaX = Mathf.Abs(x) * 2;
                }

                if (color.Length != eraserSizeWidth * eraserSizeHeight)
                    SetColor(Color.white);

                texture.SetPixels(x - deltaX, y - deltaY, eraserSizeWidth - deltaX, eraserSizeHeight - Mathf.Abs(deltaY), color);

                for (float t = 0.01f; t < 1.00f; t += 0.01f)
                {
                    int lerpX = (int)Mathf.Lerp(lastX, (float)x - deltaX, t);
                    int lerpY = (int)Mathf.Lerp(lastY, (float)y - deltaY, t);
                    texture.SetPixels(lerpX, lerpY, eraserSizeWidth - deltaX, eraserSizeHeight - Mathf.Abs(deltaY), color);
                }

                texture.Apply();
            }

            if ((x > 0 && x < (textureSize - (eraserSizeWidth * 2))) && (y > 0 && y < (textureSize - eraserSizeHeight * 2)))
            {
                this.lastX = (float)x - deltaX;
                this.lastY = (float)y - deltaY;
            }
        }

        /*
        Debug.Log("TOUCHING: " + this.touching);
        Debug.Log("LAST TOUCH: " + this.touchingLast);
        if(touching)
        {
            Debug.Log("Postion X: " + posX + " Y: " + posY);
            Debug.Log("Last Postion X: " + lastX + " Y: " + lastX);
        }
        */
        this.touchingLast = this.touching;
    }

    public void ToggleTouch(bool touching)
    {
        this.touching = touching;
    }

    public void SetTouchingPosition(float x, float y)
    {
        this.posX = x;
        this.posY = y;
    }

    public void SetIsEraser(bool boolean)
    {
        isEraser = boolean;
    }

    public void SetColor(Color color)
    {
        if (isEraser)
        {
            this.color = Enumerable.Repeat<Color>(color, eraserSizeWidth * eraserSizeHeight).ToArray<Color>();
            return;
        }

        this.color = Enumerable.Repeat<Color>(color, penSize * penSize).ToArray<Color>();
    }
}
