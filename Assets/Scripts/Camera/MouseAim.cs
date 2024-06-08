using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class MouseAim : MonoBehaviour
{

    [SerializeField]
    public Transform focus;
    [SerializeField]
    private Transform mouseAim;
    [SerializeField]
    private Transform rig;
    [SerializeField]
    private Camera cam;


    public Vector3 cameraPosition;


    public float mouseSensitivity = 2f;

    public float cameraSmoothSpeed = 2f;

    public bool enableSmoothFollow = false;

    public float followSmoothSpeed = 10f;

    public float phaseRatio = 0f;


    private float aimDistance = 500f;

    public Vector3 MouseAimPosition
    {
        get
        {
            return mouseAim.position + (mouseAim.forward * aimDistance);
        }
    }
    public Vector3 ReticlePosition
    {
        get
        {
            return focus.position + (focus.forward * aimDistance);
        }
    }

    private void Start()
    {
        cam.transform.localPosition = cameraPosition;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {

        FollowFocusPosition();
        FreeRigRotation();
        MaintainLOS();
    }

    void MaintainLOS()
    {
        int layerMask = 1 << 6;

        RaycastHit _hit;
        if (Physics.SphereCast(rig.position, cam.nearClipPlane, rig.TransformDirection(cameraPosition), out _hit, cameraPosition.magnitude, layerMask))
        {
            cam.transform.localPosition = _hit.distance * cameraPosition.normalized;
        }
        else
        {
            cam.transform.localPosition = cameraPosition;
        }
    }

    void FreeRigRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = -Input.GetAxis("Mouse Y") * mouseSensitivity;

        mouseAim.Rotate(cam.transform.up, mouseX, Space.World);
        mouseAim.Rotate(cam.transform.right, mouseY, Space.World);

        float a = Mathf.Clamp(phaseRatio, 0, .99f);
        float b = (1 - a);
        Vector3 up = Vector3.Slerp(Vector3.up, rig.up, Mathf.Max(Mathf.Abs(mouseAim.forward.y) / b - a / b, 0f));

        rig.rotation = Damp(rig.rotation,
                            Quaternion.LookRotation(mouseAim.forward, up),
                            cameraSmoothSpeed,
                            Time.deltaTime);
    }

    void LockedRigRotation()
    {
        if (focus == null) return;

        rig.rotation = focus.rotation;
    }

    void FollowFocusPosition()
    {
        if (focus != null)
        {
            if (enableSmoothFollow)
            {
                transform.position = Damp(transform.position,
                                        focus.position,
                                        followSmoothSpeed,
                                        Time.deltaTime);
            }
            else { transform.position = focus.position; }
        }
    }

    public void TeleportCamera(Vector3 position)
    {
        
        transform.position = position;
        if (enableSmoothFollow)
        {
            enableSmoothFollow = false;
            StartCoroutine(ReenableSmoothFollow());
        }

    }

    private Quaternion Damp(Quaternion a, Quaternion b, float lambda, float dt)
    {
        return Quaternion.Slerp(a, b, 1f - Mathf.Exp(-lambda * dt));
    }

    private Vector3 Damp(Vector3 a, Vector3 b, float lambda, float dt)
    {
        return Vector3.Slerp(a, b, 1f - Mathf.Exp(-lambda * dt));
    }

    IEnumerator ReenableSmoothFollow()
    {
        yield return new WaitForEndOfFrame();
        enableSmoothFollow = true;
    }
}

