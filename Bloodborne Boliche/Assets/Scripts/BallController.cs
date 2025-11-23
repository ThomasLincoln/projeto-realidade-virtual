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
    [SerializeField] private float sideSensitivity = 2.0f; // Sensibilidade da curva

    [Tooltip("Giro da bola (Efeito visual).")]
    [SerializeField] private float spinMultiplier = 20f;

    [Header("Sensação de Aceleração")]
    [SerializeField] private float curvaDePotencia = 2.0f;

    private Gyroscope gyro;
    private bool isSwinging = false;
    private float peakAcceleration = 0f;
    private bool hasBeenThrown = false;
    
    // Variável para guardar a inclinação lateral média durante o swing
    private float lateralTilt = 0f; 

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

            // Ler a inclinação lateral ENQUANTO balança
            lateralTilt = Input.acceleration.x; 
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isSwinging) ThrowBall();
            isSwinging = false;
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

        // CÁLCULO DA DIREÇÃO COM CURVA
        Vector3 forwardDir = Vector3.right; 
        Vector3 sideDir = Vector3.forward * (lateralTilt * sideSensitivity);
        Vector3 throwDirection = (forwardDir + sideDir).normalized;

        Debug.Log($"ARREMESSO! Direção: {throwDirection} | Tilt Detectado: {lateralTilt}");

        rb.AddForce(throwDirection * finalForce, ForceMode.VelocityChange);
        rb.AddTorque(Vector3.forward * finalForce * spinMultiplier, ForceMode.VelocityChange);

        if (GameManager.instance != null) 
        {
            GameManager.instance.BolaArremessada(); 
        }
    }

    // --- A FUNÇÃO QUE FALTAVA ---
    public void ResetBallPublico()
    {
        rb.isKinematic = true; // Trava a física de novo
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        hasBeenThrown = false;
        peakAcceleration = 0f;
        isSwinging = false;
        lateralTilt = 0f; // Reseta a inclinação também

        Debug.Log("Bola resetada via script externo.");
    }
}