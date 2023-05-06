using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Variables movimiento
    float horizontalMove;
    float verticalMove;

    private Vector3 playerInput;

    public CharacterController player;
    public float playerSpeed;
    private float sprintSpeed;
    public float gravity;
    public float fallVelocity;
    public float jumpForce;

    //Varaibles movimiento relativo a camara
    public Camera mainCamera;
    private Vector3 camForward;
    private Vector3 camRight;
    private Vector3 movePlayer;

    //Variables deslizamiento en pendientes
    public bool isOnSlope = false;
    private Vector3 hitNormal;
    public float slideVelocity;
    public float slopeForceDown;

    public float sensibility;
    public GameObject spine;

    //variables animacion
    private Animator playerAnimator;

    public int playerHealth = 100;
    public int currentHealth;

    // Cargamos el componente CharacterController en la variable player al iniciar el script
    void Start()
    {
        player = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        playerAnimator = GetComponent<Animator>();
        sprintSpeed = playerSpeed * 2;
        currentHealth = playerHealth;
    }
    private void updateMouseLook()
    {
        //Recogemos los valores de movimiento del raton
        float hor = Input.GetAxis("Mouse X");
        float vert = Input.GetAxis("Mouse Y");

        if (hor != 0)
        {
            //Rotamos al jugador
            player.transform.Rotate(Vector3.up * hor * sensibility);
        }

        if (vert != 0)
        {
            Vector3 rotation = mainCamera.transform.localEulerAngles;

            rotation.x = (rotation.x - vert * sensibility + 360) % 360;
            if (rotation.x > 20 && rotation.x < 180) { rotation.x = 20; }
            else
            if (rotation.x < 340 && rotation.x > 180) { rotation.x = 340; }

            //rotation.x = (rotation.x + vert * sensibility + 360) % 360;
            //Debug.Log(rotation.x);
            //if (rotation.x > 40 && rotation.x < 180) { rotation.x = 40; }else
            //if (rotation.x < 0 || rotation.x > 180) { rotation.x = 0; }

            mainCamera.transform.localEulerAngles = rotation;
        }
    }

    // Bucle de juego que se ejecuta en cada frame
    void Update()
    {
        //Guardamos el valor de entrada horizontal y vertical para el movimiento
        horizontalMove = Input.GetAxis("Horizontal");
        verticalMove = Input.GetAxis("Vertical");

        playerInput = new Vector3(horizontalMove, 0, verticalMove); //los almacenamos en un Vector3
        playerInput = Vector3.ClampMagnitude(playerInput, 1); //Y limitamos su magnitud a 1 para evitar aceleraciones en movimientos diagonales

        playerAnimator.SetFloat("playerWalkVelocity", playerInput.magnitude * playerSpeed);

        CamDirection(); //Llamamos a la funcion CamDirection()

        movePlayer = playerInput.x * camRight + playerInput.z * camForward;

        movePlayer = movePlayer * playerSpeed;  //Y lo multiplicamos por la velocidad del jugador "playerSpeed"

        SetGravity(); //Llamamos a la funcion SetGravity() para aplicar la gravedad

        PlayerSkills(); //Llamamos a la funcion PlayerSkills() para invocar las habilidades de nuestro personaje

        updateMouseLook();

        player.Move(movePlayer * Time.deltaTime); //Y por ultimo trasladamos los datos de movimiento a nuestro jugador * Time.deltaTime 
                                                  //De este modo mantenemos unos FPS estables independientemente de la potencia del equipo.
                                                  //Debug.Log("Tocando el suelo: " + player.isGrounded); //Descomenta esta linea si quieres monitorizar si estas tocando el suelo en la consola de depuracion

    }

    //Funcion para determinar la direccion a la que mira la camara. 
    public void CamDirection()
    {
        //Guardamos los vectores correspondientes a la posicion/rotacion de la carama tanto hacia delante como hacia la derecha.
        camForward = mainCamera.transform.forward;
        camRight = mainCamera.transform.right;
        //Asignamos los valores de "y" a 0 para no crear conflictos con otras operaciones de movimiento.
        camForward.y = 0;
        camRight.y = 0;
        //Y normalizamos sus valores.
        camForward = camForward.normalized;
        camRight = camRight.normalized;
    }

    //Funcion para las habilidades de nuestro jugador.

    public void PlayerSkills()
    {
        //Si estamos tocanto el suelo y pulsamos el boton "Jump"
        if (player.isGrounded && Input.GetButtonDown("Jump"))
        {
            fallVelocity = jumpForce; //La velocidad de caida pasa a ser igual a la velocidad de salto
            movePlayer.y = fallVelocity; //Y pasamos el valor a movePlayer.y
            playerAnimator.SetTrigger("playerJump");
        }
        else if (Input.GetKey(KeyCode.LeftShift) /*&& player.isGrounded -> para que no haga sprint en el aire*/)
        {
            playerSpeed = sprintSpeed;
        }
        else
        {
            playerSpeed = sprintSpeed / 2;
        }

        if (Input.GetButtonDown("Fire2"))
        {
            Debug.Log("Apuntando");
        }

        if (Input.GetButtonDown("Fire1"))
        {
            Debug.Log("Disparando");
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Recargando");
        }

    }

    //Funcion para la gravedad.
    public void SetGravity()
    {
        //Si estamos tocando el suelo
        if (player.isGrounded)
        {
            //La velocidad de caida es igual a la gravedad en valor negativo * Time.deltaTime.
            fallVelocity = -gravity * Time.deltaTime;
            movePlayer.y = fallVelocity;
        }
        else //Si no...
        {
            //aceleramos la caida cada frame restando el valor de la gravedad * Time.deltaTime.
            fallVelocity -= gravity * Time.deltaTime;
            movePlayer.y = fallVelocity;
            playerAnimator.SetFloat("playerVerticalVelocity", player.velocity.y);
        }
        playerAnimator.SetBool("isGrounded", player.isGrounded);

        SlideDown(); //Llamamos a la funcion SlideDown() para comprobar si estamos en una pendiente
    }

    //Esta funcion detecta si estamos en una pendiente muy pronunciada y nos desliza hacia abajo.
    public void SlideDown()
    {
        //si el angulo de la pendiente en la que nos encontramos es mayor o igual al asignado en player.slopeLimit, isOnSlope es VERDADERO
        isOnSlope = Vector3.Angle(Vector3.up, hitNormal) >= player.slopeLimit;

        if (isOnSlope) //Si isOnSlope es VERDADERO
        {
            //movemos a nuestro jugador en los ejes "x" y "z" mas o menos deprisa en proporcion al angulo de la pendiente.
            movePlayer.x += ((1f - hitNormal.y) * hitNormal.x) * slideVelocity;
            movePlayer.z += ((1f - hitNormal.y) * hitNormal.z) * slideVelocity;
            //y aplicamos una fuerza extra hacia abajo para evitar saltos al caer por la pendiente.
            movePlayer.y += slopeForceDown;
        }
    }

    //Esta funcion detecta cuando colisinamos con otro objeto mientras nos movemos
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //Almacenamos la normal del plano contra el que hemos chocado en hitNormal.
        hitNormal = hit.normal;
    }

    public void DoDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0) playerDie();
    }

    private void playerDie()
    {
        Cursor.lockState = CursorLockMode.None;
        Destroy(gameObject, 1.0f);
        Debug.Log("YOU DIED");
    }

    private void OnAnimatorMove()
    {

    }
}