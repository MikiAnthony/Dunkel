using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using System;

public class Whiteboard : MonoBehaviour
{
    public Renderer[] boards;
    private int textureSize = 2048;
    private int penSize = 10;
    private int eraserSizeWidth = 30;
    private int eraserSizeHeight = 250;
    private Texture2D texture;
    private List<Texture2D> textures = new List<Texture2D>();
    public Color[] color;

    private bool touchingLast;
    private bool touching;
    private float posX, posY;
    private float lastX, lastY;
    private bool isEraser;

    void Start()
    {
        for (int i = 0; i < boards.Length; i++)
        {
            Texture2D boardTexture = new Texture2D(textureSize, textureSize);
            Color fillColor = Color.white;
            var fillColorArray = boardTexture.GetPixels();

            for (var h = 0; h < fillColorArray.Length; ++h)
            {
                fillColorArray[h] = fillColor;
            }

            boardTexture.SetPixels(fillColorArray);

            boardTexture.Apply();

            boards[i].material.mainTexture = boardTexture;

            textures.Add(boardTexture);
        }
    }

    void Update()
    {
        if (texture == null)
            return;

        if (!isEraser)
        {
            int x = (int)(posX * textureSize - (penSize / 2));
            int y = (int)(posY * textureSize - (penSize / 2));

            if (touchingLast)
            {
                if ((x > 0 && x < (textureSize - (penSize * 2))) && (y > 0 && y < (textureSize - penSize * 2)))
                {
                    if (color.Length != (penSize * penSize))
                        SetColor(Color.black, 0);

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
        else if (isEraser)
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
                else if (x < 0)
                {
                    deltaX = Mathf.Abs(x) * 2;
                }

                if (color.Length != eraserSizeWidth * eraserSizeHeight)
                    SetColor(Color.white, 0);

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

    public void ToggleTouch(bool touching, int boardID)
    {
        this.touching = touching;
        SetBoardID(boardID);
    }

    public void SetTouchingPosition(float x, float y, int boardID)
    {
        this.posX = x;
        this.posY = y;
    }

    public void SetIsEraser(bool boolean, int boardID)
    {
        isEraser = boolean;
    }

    public void SetColor(Color color, int boardID)
    {
        if (isEraser)
        {
            this.color = Enumerable.Repeat<Color>(color, eraserSizeWidth * eraserSizeHeight).ToArray<Color>();
            return;
        }

        this.color = Enumerable.Repeat<Color>(color, penSize * penSize).ToArray<Color>();
    }

    public void SetBoardID(int boardID)
    {
        texture = textures[boardID];
    }

    public int GetBoardID(string name)
    {
        for(int i = 0; i <= boards.Length; i++)
        {
            if (boards[i].name == name)
            {
                return i;
            }
        }
        return 0;
    }
}
