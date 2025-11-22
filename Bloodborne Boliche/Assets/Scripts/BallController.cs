using UnityEngine;

public class BallController : MonoBehaviour
{
    // --- Controle de Movimento (Teclado) ---
    [Header("Controle de Movimento")]
    [SerializeField]
    private float moveForce = 10f;
    // NOVO: Velocidade extra para forçar a rotação visual
    [SerializeField]
    private float rotationSpeed = 10f; 
    private Vector3 moveDirection;

    // --- Controle de Arremesso (Giroscópio do CELULAR) ---
    [Header("Controle de Arremesso (Giroscópio)")]
    [SerializeField]
    private float throwForceMultiplier = 20f;
    [SerializeField]
    private float spinMultiplier = 50f; // NOVO: Força do giro no arremesso

    [SerializeField]
    private float minThrowThreshold = 0.1f;

    private Gyroscope gyro;
    private bool isSwinging = false;
    private float peakAcceleration = 0f;
    private bool hasBeenThrown = false;

    // --- Componentes ---
    private Rigidbody rb;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) Debug.LogError("Rigidbody faltando!");

        // Garante que o Rigidbody não tenha limite máximo de rotação muito baixo
        rb.maxAngularVelocity = 100f; 

        initialPosition = transform.position;
        initialRotation = transform.rotation;

        if (SystemInfo.supportsGyroscope)
        {
            gyro = Input.gyro;
            gyro.enabled = true;
        }
    }

    void Update()
    {
        if (hasBeenThrown)
        {
            if (Input.GetMouseButtonDown(0)) ResetBall();
            return;
        }

        // --- Lógica de Movimento (Teclado) ---
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        moveDirection = new Vector3(horizontalInput, 0, verticalInput);

        // --- Lógica de Arremesso ---
        if (gyro == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            isSwinging = true;
            peakAcceleration = 0f;
        }

        if (isSwinging)
        {
            float currentAccelerationZ = Mathf.Abs(gyro.userAcceleration.z);
            if (currentAccelerationZ > peakAcceleration)
            {
                peakAcceleration = currentAccelerationZ;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isSwinging) ThrowBall();
            isSwinging = false;
        }
    }

    void ThrowBall()
    {
        float throwForce = peakAcceleration * throwForceMultiplier;

        if (peakAcceleration < minThrowThreshold) return;

        hasBeenThrown = true;

        // 1. Aplica a força para frente (Empurrão)
        rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);

        // 2. NOVO: Aplica TORQUE (Rotação) para a bola sair girando
        // Usamos 'transform.right' como eixo para a bola girar para frente
        rb.AddTorque(transform.right * throwForce * spinMultiplier, ForceMode.Impulse);
    }

    private void ResetBall()
    {
        // Nota: Unity 6 usa 'linearVelocity', versões antigas usam 'velocity'
        rb.linearVelocity = Vector3.zero; 
        rb.angularVelocity = Vector3.zero; // Zera o giro
        
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        hasBeenThrown = false;
        moveDirection = Vector3.zero;
    }

    void FixedUpdate()
    {
        if (rb != null && !hasBeenThrown)
        {
            // Move a bola (Empurrão)
            rb.AddForce(moveDirection * moveForce);

            // NOVO: Adiciona rotação manual enquanto move com WASD
            // O cálculo Vector3.Cross descobre o eixo de rotação correto baseado na direção
            if (moveDirection != Vector3.zero)
            {
                Vector3 rotationAxis = Vector3.Cross(Vector3.up, moveDirection);
                rb.AddTorque(rotationAxis * rotationSpeed * moveForce);
            }
        }
    }
}