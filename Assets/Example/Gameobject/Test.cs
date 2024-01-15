using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Taco.Gameplay;

public class Test : MonoBehaviour
{
    public GameplayTagContainer Tag;

    #region Test
    public float rotatingSpeed;
    Vector3 Axis;

    public float moveSpeed;
    float timer;
    float originalTimer;
    Vector3 Direction;

    private void Start()
    {
        timer = originalTimer = Random.Range(1, 3f);
        Axis = Direction = new Vector3(Random.Range(0, 2f), Random.Range(0, 2f), Random.Range(0, 2f));
    }
    public void Update()
    {
        if (Tag.ContainsTag("运动状态.旋转"))
        {
            transform.RotateAround(transform.position, Axis, rotatingSpeed * Time.deltaTime);
        }
        if (Tag.ContainsTag("运动状态.位移"))
        {
            if(timer > 0)
            {
                timer -= Time.deltaTime;
                transform.position += Direction * moveSpeed * Time.deltaTime;
            }
            else
            {
                timer = originalTimer;
                Direction *= -1;
            }
        }
    }
    public void Select(bool value)
    {
    }
    #endregion

    [ContextMenu("AddRotate")]
    public void AddRotate()
    {
        Tag.AddTagRuntime("运动状态.旋转");
    }
    [ContextMenu("RemoveRotate")]
    public void RemoveRotate()
    {
        Tag.RemoveTagRuntime("运动状态.旋转");
    }
}
