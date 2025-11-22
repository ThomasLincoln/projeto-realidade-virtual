using UnityEngine;

public class BallController : MonoBehaviour
{
    // --- Controle de Movimento ---
    [Header("Configurações de Movimento")]
    [SerializeField] private float moveForce = 8f; // Reduzi um pouco
    [SerializeField] private float rotationSpeed = 5f; // Reduzi para suavizar
    
    // --- Configurações de Física ---
    [Header("Limites de Física")]
    [SerializeField] private float maxRotationSpeed = 25f; // Limite visual para não "bugar" o olho

    // --- Controle de Arremesso (Giroscópio) ---
    [Header("Arremesso (Giroscópio)")]
    [SerializeField] private float throwForceMultiplier = 20f;
    [SerializeField] private float spinMultiplier = 50f;
    [SerializeField] private float minThrowThreshold = 0.1f;

    private Gyroscope gyro;
    private bool isSwinging = false;
    private float peakAcceleration = 0f;
    private bool hasBeenThrown = false;
    private Vector3 moveDirection;

    private Rigidbody rb;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) Debug.LogError("Rigidbody faltando!");

        // AQUI ESTÁ O SEGREDO 2:
        // Limitamos a velocidade angular para o olho humano conseguir acompanhar
        rb.maxAngularVelocity = maxRotationSpeed; 

        // Dica: Garanta que no Inspector o Rigidbody esteja com 'Interpolate' ligado!
        
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

        // --- Inputs ---
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Direção corrigida (W/S no Eixo X)
        moveDirection = new Vector3(verticalInput, 0, -horizontalInput);

        // --- Lógica de Arremesso ---
        if (gyro == null) return;
        
        // (Lógica do mouse mantida igual...)
        if (Input.GetMouseButtonDown(0)) { isSwinging = true; peakAcceleration = 0f; }
        
        if (isSwinging)
        {
            float currentAccel = Mathf.Abs(gyro.userAcceleration.z);
            if (currentAccel > peakAcceleration) peakAcceleration = currentAccel;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isSwinging) ThrowBall();
            isSwinging = false;
        }
    }

    void FixedUpdate()
    {
        if (rb != null && !hasBeenThrown)
        {
            // Move a bola
            rb.AddForce(moveDirection * moveForce);

            // APLICAÇÃO DE ROTAÇÃO MAIS SUAVE:
            if (moveDirection != Vector3.zero)
            {
                Vector3 rotationAxis = Vector3.Cross(Vector3.up, moveDirection);
                
                // Mudança: Usamos ForceMode.Acceleration ignora a massa para ser mais fluido
                // E não multiplicamos mais pelo moveForce, para ter controle separado
                rb.AddTorque(rotationAxis * rotationSpeed, ForceMode.Acceleration);
            }
        }
    }

    void ThrowBall()
    {
        float throwForce = peakAcceleration * throwForceMultiplier;
        if (peakAcceleration < minThrowThreshold) return;

        hasBeenThrown = true;
        rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        rb.AddTorque(transform.right * throwForce * spinMultiplier, ForceMode.Impulse);
    }

    private void ResetBall()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        hasBeenThrown = false;
        moveDirection = Vector3.zero;
    }
}