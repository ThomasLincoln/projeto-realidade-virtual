using UnityEngine;
using System.Collections;

public class BallController : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Limites de Movimento Lateral")]
    [Tooltip("Limite mínimo no eixo Z (esquerda)")]
    [SerializeField] private float minZ = -3f;
    [Tooltip("Limite máximo no eixo Z (direita)")]
    [SerializeField] private float maxZ = 10f;

    [Header("Configurações de Arremesso")]
    [Tooltip("Força base multiplicadora.")]
    [SerializeField] private float throwForceMultiplier = 40f;

    [Tooltip("O quanto a inclinação do celular afeta a direção.")]
    [SerializeField] private float sideSensitivity = 2.0f; // <--- NOVO: Sensibilidade da curva

    [Tooltip("Giro da bola (Efeito visual).")]
    [SerializeField] private float spinMultiplier = 20f;

    [Header("Sensação de Aceleração")]
    [SerializeField] private float curvaDePotencia = 2.0f;

    private Gyroscope gyro;
    private bool isSwinging = false;
    private float peakAcceleration = 0f;
    private bool hasBeenThrown = false;
    
    // Variável para guardar a inclinação lateral média durante o swing
    private float lateralTilt = 0f; // <--- NOVO

    private Rigidbody rb;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 50f;
        rb.isKinematic = true;

        initialPosition = transform.position;
        initialRotation = transform.rotation;

        hasBeenThrown = false;
        peakAcceleration = 0f;
        isSwinging = false;

        StartCoroutine(IniciarGiroscopio());
    }

    void OnEnable()
    {
        hasBeenThrown = false;
        isSwinging = false;
    }

    IEnumerator IniciarGiroscopio()
    {
        if (SystemInfo.supportsGyroscope)
        {
            gyro = Input.gyro;
            gyro.enabled = true;
            gyro.updateInterval = 0.0167f;
        }
        yield return null;
    }

    void Update()
    {
        if (hasBeenThrown) return;

        // --- 1. Movimento Lateral (Teclado) ---
        float horizontalInput = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) horizontalInput = -1f;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontalInput = 1f;

        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            Vector3 newPos = transform.position;
            newPos.z += horizontalInput * moveSpeed * Time.deltaTime;
            newPos.z = Mathf.Clamp(newPos.z, minZ, maxZ);
            transform.position = newPos;
        }

        // --- 2. Leitura do Arremesso ---
        if (Input.GetMouseButtonDown(0))
        {
            isSwinging = true;
            peakAcceleration = 0f;
        }

        if (isSwinging && gyro != null)
        {
            float currentAccel = gyro.userAcceleration.magnitude;
            if (currentAccel > peakAcceleration) peakAcceleration = currentAccel;

            // <--- NOVO: Ler a inclinação lateral ENQUANTO balança
            // Input.acceleration pega a gravidade. 
            // Se estiver em Paisagem (Landscape), geralmente o Y ou X indica a inclinação lateral.
            // Teste: Se inclinar o celular para a esquerda, a bola deve ir para a esquerda.
            // Se estiver invertido, coloque um sinal de menos (-) na frente de Input.acceleration.x
            lateralTilt = Input.acceleration.x; 
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isSwinging) ThrowBall();
            isSwinging = false;
        }

        // Atalho de Teste (Espaço) - Simulando uma curva para a direita
        if (Input.GetKeyDown(KeyCode.Space))
        {
            peakAcceleration = 3f;
            lateralTilt = 0.5f; // Simula inclinação
            ThrowBall();
        }
    }

    void ThrowBall()
    {
        if (hasBeenThrown) return;

        float forcaExponencial = Mathf.Pow(peakAcceleration, curvaDePotencia);
        float finalForce = forcaExponencial * throwForceMultiplier;

        if (finalForce < 2f) return;

        hasBeenThrown = true;
        rb.isKinematic = false;

        // <--- NOVO: CÁLCULO DA DIREÇÃO
        // 1. Pegamos a direção "Frente" (Eixo X positivo)
        Vector3 forwardDir = Vector3.right; 
        
        // 2. Pegamos a direção "Lado" (Eixo Z), multiplicada pela inclinação do celular
        // O valor negativo (-) ou positivo depende de como o celular está virado (Paisagem Esquerda/Direita)
        // Se a bola for para o lado errado, troque o sinal de (+) para (-) aqui embaixo:
        Vector3 sideDir = Vector3.forward * (lateralTilt * sideSensitivity);

        // 3. Somamos as duas e normalizamos (para não ficar mais rápido só porque foi na diagonal)
        Vector3 throwDirection = (forwardDir + sideDir).normalized;

        Debug.Log($"ARREMESSO! Direção: {throwDirection} | Tilt Detectado: {lateralTilt}");

        // Aplica a força na direção calculada
        rb.AddForce(throwDirection * finalForce, ForceMode.VelocityChange);
        
        // Aplica torque para girar a bola visualmente
        rb.AddTorque(Vector3.forward * finalForce * spinMultiplier, ForceMode.VelocityChange);

        if (GameManager.instance != null) // Removi o erro caso GameManager não exista no seu script ainda
        {
           // GameManager.instance.BolaArremessada(); 
        }
    }
}