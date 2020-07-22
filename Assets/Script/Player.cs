using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    private Rigidbody rb;
    private Vec3 pos;
    public float speed = 1.0f;
    public float jumpSpeed = 5f;
    private Vec3 moveDirection = Vec3.zero;
    public bool isGrounded;
    public bool isBlocked;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        isBlocked = false;
        pos = transform.position;
        animator = GetComponent<Animator>();
    }

    void OnCollisionStay()
    {
        isGrounded = true;
    }

    private void Update()
    {

        float axisX = Input.GetAxis("Horizontal");
        float axisZ = Input.GetAxis("Vertical");
        moveDirection = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * new Vec3(axisX, 0, axisZ);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * Quaternion.LookRotation(new Vec3(axisX, 0, axisZ)), 0.15f);
        moveDirection *= speed;
        Vec3 leftMoveDir = Vector3.Cross(moveDirection.normalized, Vector3.up);


        if (Input.GetButton("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpSpeed, ForceMode.Impulse);
            isGrounded = false;

        }
        if (moveDirection.magnitude > 0.1f)
        {
            animator.SetBool(Animator.StringToHash("isMoving"), true);
            RaycastHit hit;
            Vec3 rayPos = new Vec3(pos.x,pos.y + 0.2f, pos.z);
            if (Physics.Raycast(new Ray(rayPos, moveDirection), out hit, gameObject.GetComponent<Collider>().bounds.extents.x))
            {
                Debug.DrawLine(rayPos, rayPos + (moveDirection.normalized * gameObject.GetComponent<Collider>().bounds.extents.x), Color.red, 0f);
                if (hit.collider.gameObject.tag == "Wall")
                {
                    moveDirection = Vec3.zero;
                }
                Debug.Log(Vector3.Dot(transform.position, hit.point));
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
        pos.y = rb.transform.position.y;
        transform.position = pos;
        rb.MovePosition(transform.position);
    }


    void OnCollisionEnter(Collision col)
    {

    }
}

