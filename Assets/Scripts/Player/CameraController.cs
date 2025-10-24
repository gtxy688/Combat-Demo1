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
        //�Զ������� Camera Y  Camera X ���� Mouse Y  Mouse X(��ΪMouse Y��Mouse X�ֱ��޷�ʹ��)
        rotationX -= Input.GetAxisRaw("Camera Y") * rotationSpeed * (invertY ? -1 : 1);
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);
        rotationY += Input.GetAxisRaw("Camera X") * rotationSpeed * (invertX ? -1 : 1);

        targetRotation=Quaternion.Euler(rotationX, rotationY, 0);

        focusPosition = followTarget.position + new Vector3(framingOffset.x, framingOffset.y);
        //�����������λ��
        transform.position = focusPosition - targetRotation * Vector3.forward * distance;
        //����������ĽǶ�
        //���ﲻֱ�ӿ������ ����Ϊ��������Ľ�ɫ�����֮��Ĺ�ϵ �ǽ�ɫ��������� �����ǽ�ɫ�����������
        transform.rotation = targetRotation;
    }
    //�������ƽ�淽��
    public Quaternion PlanarRotation=>Quaternion.Euler(0,rotationY,0);
}
