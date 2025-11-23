using UnityEngine;
using UnityEngine.InputSystem;

public class MovendoBolas : MonoBehaviour
{
    [Header("Controle de Movimento")]
    [SerializeField] private float moveForce = 20f;

    [Header("Controle de Arremesso (Giroscópio)")]
    [SerializeField] private float throwForceMultiplier = 50f;

    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool hasBeenThrown = false;
    private bool isHoldingThrow = false;
    private float peakSwingSpeed = 0f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    public void OnMover(InputAction.CallbackContext context)
    {
        if (hasBeenThrown) { moveDirection = Vector3.zero; return; }

        Vector2 moveInput = context.ReadValue<Vector2>();
        moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
    }

    public void OnArremessar(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (hasBeenThrown)
            {
                // Se apertar botão com bola jogada, reseta manualmente (opcional, o GameManager já faz isso)
                // ResetBallPublico(); 
            }
            else
            {
                isHoldingThrow = true;
                peakSwingSpeed = 0f;
            }
        }
        else if (context.canceled)
        {
            if (isHoldingThrow) ThrowBall();
            isHoldingThrow = false;
        }
    }

    public void OnGiroscopio(InputAction.CallbackContext context)
    {
        if (!isHoldingThrow) return;
        float forwardSwingSpeed = context.ReadValue<Vector3>().x;
        if (forwardSwingSpeed > peakSwingSpeed) peakSwingSpeed = forwardSwingSpeed;
    }

    void ThrowBall()
    {
        hasBeenThrown = true;
        isHoldingThrow = false;
        float throwForce = peakSwingSpeed * throwForceMultiplier;

        rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);

        // --- AVISA O JOGO QUE A BOLA FOI JOGADA ---
        if (GameManager.instance != null)
        {
            GameManager.instance.BolaArremessada();
        }
    }

    // Agora é PÚBLICO para o GameManager poder chamar
    public void ResetBallPublico()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        hasBeenThrown = false;
        isHoldingThrow = false;
        moveDirection = Vector3.zero;

        Debug.Log("Bola resetada pelo GameManager.");
    }

    void FixedUpdate()
    {
        if (rb != null && !hasBeenThrown)
        {
            rb.AddForce(moveDirection * moveForce);
        }
    }
}