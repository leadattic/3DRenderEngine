using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System;
using System.Threading;

public class Raycaster : MonoBehaviour
{
    public static int x_lim = 260;
    public static int y_lim = 144;
    public static float FOV = 100; // x FOV
    public static bool done = false;
    public static float x_gap;
    public static float y_gap;
    public static float startPosX;
    public static float startPosY;
    public float shadowMultiplier;
    private Stopwatch stopwatch;
    public GameObject targetObject;
    public Material reflectiveMaterial;
    List<Color> pixList;
    private int iterationCount;
    private Texture2D texture;
    public GameObject parent;
    public Material invicibleMat;
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
       
            
                for (int i = 0; i < y_lim; i++)
                {
                    for (int j = 0; j < x_lim; j++)
                    {
                        float angleY = startPosY + i * y_gap;
                        float angleX = startPosX + j * x_gap;
                        Vector3 direction = Quaternion.Euler(angleY, angleX + parent.transform.eulerAngles.y, 0) * Vector3.forward;
                        GetColorFromRaycast(direction, 1000000);
                    }
                }


                RenderImg(pixList, x_lim, y_lim, $"C:\\Users\\Axelc\\Downloads\\renderImg{(int) iterationCount}.png");
                iterationCount++;
                pixList.Clear();
                UnityEngine.Debug.Log((stopwatch.ElapsedMilliseconds/iterationCount));
    }
            
   

    private void RenderImg(List<Color> pixels, int width, int height, string filePath)
    {
        if (pixels == null || pixels.Count != width * height)
        {
            UnityEngine.Debug.LogError("Invalid pixels data or dimensions");
            return;
        }

        texture = new Texture2D(width, height);

        // Set the pixels from the list
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int pixelIndex = i * width + j;
                texture.SetPixel(j, height - 1 - i, pixels[pixelIndex]); // Flip y-coordinate
            }
        }

        texture.Apply(); 
    }


    private void GetColorFromRaycast(Vector3 direction, float distance)
    {
        
        // UnityEngine.Debug.DrawRay(transform.position, direction * 100, new Color(0, 1f, 0, 0.5f), 100);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, distance))
        {
            if(hit.transform.gameObject.tag == "Texture") 
            {
                Color hitColor;
                Renderer renderer = hit.collider.GetComponent<Renderer>();
            
                if (renderer != null)
                {
                    // Get the main texture of the material
                    Texture2D texture = renderer.material.mainTexture as Texture2D;
                    int x = Mathf.FloorToInt(hit.textureCoord.x * texture.width);
                    int y = Mathf.FloorToInt(hit.textureCoord.y * texture.height);
                    hitColor = texture.GetPixel(x, y);
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
                }
                else
                {
                    hitColor = new Color(0f, 1f, 0f);
                    pixList.Add(hitColor);
                }
            }
            else
            {
                
            Renderer renderer = hit.collider.gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                Color hitColor;
                if (renderer.material != invicibleMat)
                {
                    hitColor = renderer.material.color;
                }
                else
                {
                    RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, distance);
                    GameObject[] gameObjects = new GameObject[hits.Length];
                    for(int i = 0; i < hits.Length; i++)
                    {
                        gameObjects[i] = hits[i].transform.gameObject;
                    }
                    GameObject hitObject = GetNextClosestObject(gameObjects);
                    renderer = hitObject.GetComponent<Collider>().gameObject.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        hitColor = renderer.material.color;
                    }
                    else
                    {
                        hitColor = Color.white;
                    }
                }
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
        }
        else
        {
            Color hitColor =new Color(0.40234375f, 0.04296875f, 0.04296875f);
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

    void OnGUI()
    {

        Graphics.DrawTexture(new Rect(0, 0, x_lim, y_lim), texture);
    }

    public GameObject GetNextClosestObject(GameObject[] objects)
    {
        float closestDist = float.MaxValue;
        float nextClosestDist = float.MaxValue;
        GameObject closestObject = null;
        GameObject nextClosestObject = null;

        for (int i = 0; i < objects.Length; i++)  //list of gameObjects to search through
        {
            float dist = Vector3.Distance(objects[i].transform.position, transform.position);
            if (dist < closestDist)
            {
                nextClosestDist = closestDist;
                nextClosestObject = closestObject;
                closestDist = dist;
                closestObject = objects[i];
            }
            else if (dist < nextClosestDist)
            {
                nextClosestDist = dist;
                nextClosestObject = objects[i];
            }
        }
        return nextClosestObject;
    }

}