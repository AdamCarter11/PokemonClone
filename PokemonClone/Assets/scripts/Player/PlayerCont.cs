using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCont : MonoBehaviour
{
    public float speed;
    public bool isMoving;
    private Vector2 input;
    private Animator animator;
    public LayerMask grassLayer;
    public LayerMask solidObjectTileMap;
    public event Action OnEncountered;
    void Awake() {
        animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public void HandleUpdate()
    {
        if (!isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
            if(input.x != 0) { input.y = 0; }
            if(input != Vector2.zero)
            {
                animator.SetFloat("moveX",input.x);
                animator.SetFloat("moveY", input.y);
                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;
                if (IsWalkable(targetPos))
                {
                    StartCoroutine(Move(targetPos));
                }
            }
        }
        animator.SetBool("isMoving", isMoving);
        
    }
    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
        isMoving = false;
        CheckForEncounters();
    }
    private bool IsWalkable(Vector3 targetPos)
    {
        if(Physics2D.OverlapCircle(targetPos, 0.05f, solidObjectTileMap) != null)
        {
            return false;
        }
        return true;
    }
    private void CheckForEncounters()
    {
        if (Physics2D.OverlapCircle(transform.position, 0.1f, grassLayer) != null)
        {
            if(UnityEngine.Random.Range(1,101) <= 10)   //need to figure out a way to limit it so after 1 battle, you can't get it again for a bit, and also increase the chances if you have been walking in the grass for a while. we do UnityEngine because we did using System at the top and both unity and system have a random class so we have to specify
            {
                animator.SetBool("isMoving", false);

                OnEncountered();
            }
        }
    }
}
