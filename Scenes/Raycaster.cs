using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System;

public class Raycaster : MonoBehaviour
{
    public static int x_lim = 1280;
    public static int y_lim = 720;
    public static float FOV = 100; // x FOV
    public static bool done = false;
    public static float x_gap;
    public static float y_gap;
    public static float startPosX;
    public static float startPosY;
    public float shadowMultiplier;
    private Stopwatch stopwatch;
    public GameObject targetObject;
    List<Color> pixList;
    private void Start()
    {
        pixList = new List<Color>();
        stopwatch = new Stopwatch();
        stopwatch.Start();
        x_gap = FOV / x_lim;
        y_gap = FOV /  x_lim;
        startPosX = (float)-((x_lim / 2d) * x_gap);
        startPosY = (float)-((y_lim / 2d) * y_gap);
        UnityEngine.Debug.Log("X_gap, Y_gap, FOV, Y_FOV:" + x_gap + "," + y_gap + "," + FOV + "," + FOV * y_lim / x_lim + ",");
        UnityEngine.Debug.Log("startPosX,Y:" + startPosX + "," + startPosY);
        UnityEngine.Debug.Log(transform.position);
    }

    private void Update()
    {
        if (!done)
        {
            for (int i = 0; i < y_lim; i++)
            {
                for (int j = 0; j < x_lim; j++)
                {
                    float angleY = startPosY + i * y_gap;
                    float angleX = startPosX + j * x_gap;
                    Vector3 direction = Quaternion.Euler(angleY, angleX, 0) * Vector3.forward;

                    GetColorFromRaycast(direction, 1000000);
                }
            }
            done = true;
            UnityEngine.Debug.Log(stopwatch.ElapsedMilliseconds + " ms");
            UnityEngine.Debug.Log(pixList.Count);
            RenderImg(pixList, x_lim, y_lim, "C:\\Users\\Axelc\\Downloads\\renderImg.png");
        }
        

    }


    private void RenderImg(List<Color> pixels, int width, int height, string filePath)
    {
        if (pixels == null || pixels.Count != width * height)
        {
            UnityEngine.Debug.LogError("Invalid pixels data or dimensions");
            return;
        }

        Texture2D texture = new Texture2D(width, height);

        // Set the pixels from the list
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int pixelIndex = i * width + j;
                texture.SetPixel(j, height - 1 - i, pixels[pixelIndex]); // Flip y-coordinate
            }
        }

        texture.Apply(); // Apply changes to the texture

        // Encode texture to PNG
        byte[] bytes = texture.EncodeToPNG();
        Destroy(texture); // Destroy the texture to free up memory

        // Save PNG file
        File.WriteAllBytes(filePath, bytes);

        UnityEngine.Debug.Log($"Image saved to {filePath}");
    }


    private void GetColorFromRaycast(Vector3 direction, float distance)
    {
        
        // UnityEngine.Debug.DrawRay(transform.position, direction * 100, new Color(0, 1f, 0, 0.5f), 100);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, distance))
        {
            Renderer renderer = hit.collider.gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color hitColor = renderer.material.color;
                Vector3 hitPosition = hit.point;
                Vector3 shadowDirection = (targetObject.transform.position - hitPosition).normalized;
                Ray shadowRay = new Ray(hitPosition - shadowDirection * 0.01f, shadowDirection);
                RaycastHit shadowHit;
                // UnityEngine.Debug.DrawRay(hitPosition, shadowDirection * 100, new Color(1f, 0, 0, 0.5f), 100);
                if (Physics.Raycast(shadowRay, out shadowHit))
                {
                    if (shadowHit.collider.gameObject == targetObject)
                    {

                        pixList.Add(hitColor);
                    }
                    else
                    {
                        pixList.Add(CreateShadowEffect(hitColor, shadowMultiplier));
                    }
                }
                else
                {
                    UnityEngine.Debug.Log("Did not hit the sun nor any other GameObject. TF?");
                    pixList.Add(hitColor);
                }
            }
        }
        else
        {
            Color hitColor = Color.cyan;
            pixList.Add(hitColor);
        }
    }


    public static Color CreateShadowEffect(Color color, float shadowFactor)
    {
        // Clamp shadowFactor between 0 and 1 to avoid increasing brightness
        shadowFactor = Mathf.Clamp01(shadowFactor);

        // Decrease the brightness by multiplying each color component
        return new Color(color.r * shadowFactor, color.g * shadowFactor, color.b * shadowFactor, color.a);
    }

}

