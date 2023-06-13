using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    CharacterController controller; 
    float speed = 30;
// Start is called before the first frame update 
    void Awake() 
    {      
        controller = GetComponent<CharacterController>(); 
    }
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("UpDown"), Input.GetAxisRaw("Vertical")).normalized;
        controller.Move(movement * speed * Time.deltaTime);
    }
}
