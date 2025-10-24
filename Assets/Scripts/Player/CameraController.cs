using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform followTarget;

    [SerializeField] float distance = 5f;

    [SerializeField] float rotationSpeed = 2f;

    [SerializeField] float minVerticalAngle = -10f;
    [SerializeField] float maxVerticalAngle = 45f;

    [SerializeField] Vector2 framingOffset;

    [SerializeField] bool invertX=false;
    [SerializeField] bool invertY=false;

    float rotationX;
    float rotationY;

    Quaternion targetRotation;
    Vector3 focusPosition;

    private void Start()
    {
        //Cursor.visible=false;
        //Cursor.lockState=CursorLockMode.Locked;
        focusPosition = followTarget.position + new Vector3(framingOffset.x, framingOffset.y);
        
    }

    private void Update()
    { 
        //Edit->Project Settings->Input Manager
        //自定义输入 Camera Y  Camera X 代替 Mouse Y  Mouse X(因为Mouse Y和Mouse X手柄无法使用)
        rotationX -= Input.GetAxisRaw("Camera Y") * rotationSpeed * (invertY ? -1 : 1);
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);
        rotationY += Input.GetAxisRaw("Camera X") * rotationSpeed * (invertX ? -1 : 1);

        targetRotation=Quaternion.Euler(rotationX, rotationY, 0);

        focusPosition = followTarget.position + new Vector3(framingOffset.x, framingOffset.y);
        //设置摄像机的位置
        transform.position = focusPosition - targetRotation * Vector3.forward * distance;
        //设置摄像机的角度
        //这里不直接看向对象 是因为这个方法的角色与相机之间的关系 是角色按相机方向动 而不是角色动带领相机动
        transform.rotation = targetRotation;
    }
    //摄像机的平面方向
    public Quaternion PlanarRotation=>Quaternion.Euler(0,rotationY,0);
}
