using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    private Rigidbody rb;
    private Vec3 pos;
    [SerializeField]
    private float speed = 1.0f;
    [SerializeField]
    private float jumpSpeed = 10f;
    [SerializeField]
    private Vec3 moveDirection = Vec3.zero;
    [SerializeField]
    private bool isBlocked;
    private Vec3 rotator;
    private Animator animator;
    private float distToGround;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        isBlocked = false;
        pos = transform.position;
        animator = GetComponent<Animator>();
        rotator = new Vec3();
        distToGround = GetComponent<Collider>().bounds.extents.y;

    }

    private void Update()
    {

        float axisX = Input.GetAxis("Horizontal");
        float axisZ = Input.GetAxis("Vertical");
        moveDirection = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * new Vec3(axisX, 0, axisZ);
        if (moveDirection.magnitude > 0.1f)
        {
            rotator = new Vec3(axisX, 0, axisZ);
        }
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * Quaternion.LookRotation(rotator), 0.15f);
        moveDirection *= speed;
        Vec3 leftMoveDir = Vector3.Cross(moveDirection.normalized, Vector3.up);


        if (Input.GetButton("Jump") && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpSpeed, ForceMode.Impulse);

        }
        if (moveDirection.magnitude > 0.1f)
        {
            animator.SetBool(Animator.StringToHash("isMoving"), true);
            RaycastHit hit;
            Vec3 rayPos = new Vec3(pos.x,pos.y + 0.1f, pos.z);
            if (Physics.Raycast(new Ray(rayPos, moveDirection), out hit, gameObject.GetComponent<Collider>().bounds.extents.x))
            {
                Debug.DrawLine(rayPos, rayPos + (moveDirection.normalized * gameObject.GetComponent<Collider>().bounds.extents.x), Color.red, 0f);
                if (hit.collider.gameObject.tag == "Wall")
                {
                    moveDirection = Vec3.zero;
                }
               
            }
            else
            {
                Debug.DrawLine(rayPos, rayPos + (moveDirection.normalized * gameObject.GetComponent<Collider>().bounds.extents.x), Color.green, 0f);
            }
        }
        else
        {
            animator.SetBool(Animator.StringToHash("isMoving"), false);

        }

        pos += moveDirection * Time.deltaTime;
        Debug.Log("velocity" + moveDirection.x + "," + moveDirection.y + "," + moveDirection.z);
        pos.y = rb.transform.position.y;
        transform.position = pos;
        rb.MovePosition(transform.position);
    }


    void OnCollisionEnter(Collision col)
    {

    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f);
    }
}

