using UnityEngine;
using UnityEngine.InputSystem;

public class MovendoBolas : MonoBehaviour 
{
    [Header("Controle de Movimento")]
    [SerializeField]
    private float moveForce = 10f;

    [Header("Controle de Arremesso (Giroscópio)")]
    [SerializeField]
    private float throwForceMultiplier = 50f;

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
        if (rb == null)
        {
            Debug.LogError("O objeto " + name + " não tem um Rigidbody!");
        }

        // --- NOVO: Salva a posi��o e rota��o iniciais ---
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    // --- Fun��es chamadas pelos Eventos do PlayerInput ---

    public void OnMover(InputAction.CallbackContext context)
    {
        if (hasBeenThrown)
        {
            moveDirection = Vector3.zero;
            return;
        }
        Vector2 moveInput = context.ReadValue<Vector2>();
        Debug.Log($"[Mover] Input: {moveInput}");
        moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
    }

    // --- L�GICA ATUALIZADA AQUI ---
    public void OnArremessar(InputAction.CallbackContext context)
    {
        // 1. Detecta o apertar do bot�o
        if (context.started)
        {
            if (hasBeenThrown)
            {
                // SE A BOLA J� FOI JOGADA: Reseta a bola
                ResetBall();
                Debug.Log("[Arremessar] Bot�o pressionado -> RESETANDO BOLA");
            }
            else
            {
                // SE A BOLA N�O FOI JOGADA: Inicia o "swing"
                Debug.Log("[Arremessar] Bot�o PRESSIONADO (started)");
                isHoldingThrow = true;
                peakSwingSpeed = 0f;
            }
        }
        // 2. Detecta o soltar do bot�o (s� arremessa se estivermos no meio de um "swing")
        else if (context.canceled)
        {
            if (isHoldingThrow) // Garante que s� arremessa se estava segurando
            {
                Debug.Log("[Arremessar] Botão SOLTO (canceled)");
                ThrowBall();
            }
            isHoldingThrow = false; // Para o "swing" de qualquer forma
        }
    }

    public void OnGiroscopio(InputAction.CallbackContext context)
    {
        if (!isHoldingThrow) return;

        Vector3 angularVelocity = context.ReadValue<Vector3>();
        Debug.Log($"[Giroscopio] Raw Data: X={angularVelocity.x}, Y={angularVelocity.y}, Z={angularVelocity.z}");

        float forwardSwingSpeed = angularVelocity.x;

        if (forwardSwingSpeed > peakSwingSpeed)
        {
            peakSwingSpeed = forwardSwingSpeed;
        }
    }

    // --- L�gica Principal ---

    void ThrowBall()
    {
        hasBeenThrown = true; // Marca a bola como arremessada
        isHoldingThrow = false;
        float throwForce = peakSwingSpeed * throwForceMultiplier;

        Debug.LogWarning($"ARREMESSANDO! Pico de Velocidade: {peakSwingSpeed}, Força Aplicada: {throwForce}");

        rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
    }

    // --- NOVA FUN��O DE RESET ---
    private void ResetBall()
    {
        // 1. Para toda a f�sica da bola
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 2. Reposiciona a bola no in�cio
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        // 3. Libera os controles para jogar de novo
        hasBeenThrown = false;
        isHoldingThrow = false;
        moveDirection = Vector3.zero; // Garante que o input de movimento pare

        Debug.LogWarning("--- BOLA RESETADA PARA A POSIÇÃO INICIAL ---");
    }


    void FixedUpdate()
    {
        if (rb != null && !hasBeenThrown)
        {
            rb.AddForce(moveDirection * moveForce);
        }
    }
}