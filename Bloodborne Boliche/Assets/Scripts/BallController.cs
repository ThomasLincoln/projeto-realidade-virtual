using UnityEngine;
using System.Collections;

public class BallController : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Limites de Movimento Lateral")]
    [SerializeField] private float minZ = -3f;
    [SerializeField] private float maxZ = 10f;

    [Header("Configurações de Arremesso")]
    [Tooltip("Força base multiplicadora.")]
    [SerializeField] private float throwForceMultiplier = 40f;

    [Tooltip("Sensibilidade da curva. Diminua se estiver muito forte.")]
    [SerializeField] private float sideSensitivity = 0.5f; 

    [Tooltip("Zona morta para ignorar tremidas leves da mão.")]
    [SerializeField] private float tiltDeadzone = 0.15f;

    [Tooltip("Giro da bola (Efeito visual).")]
    [SerializeField] private float spinMultiplier = 20f;

    [Header("Sensação de Aceleração")]
    [SerializeField] private float curvaDePotencia = 2.0f;

    private Gyroscope gyro;
    private bool isSwinging = false;
    private float peakAcceleration = 0f;
    private bool hasBeenThrown = false;
    
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

        // --- 1. Movimento Lateral (Teclado) INVERTIDO ---
        float horizontalInput = 0f;
        
        // MUDANÇA AQUI: Inverti os valores (1f e -1f)
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) 
        {
            horizontalInput = 1f; // Antes era -1
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) 
        {
            horizontalInput = -1f; // Antes era 1
        }

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

            // MUDANÇA AQUI: Adicionei o sinal de MENOS (-) para inverter a direção
            float rawTilt = -Input.acceleration.x; 

            // Aplica a zona morta
            if (Mathf.Abs(rawTilt) < tiltDeadzone)
            {
                lateralTilt = 0f;
            }
            else
            {
                lateralTilt = rawTilt;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isSwinging) ThrowBall();
            isSwinging = false;
        }
    }

    void FixedUpdate()
    {
        // --- BLOQUEIO ANTI-RETORNO ---
        if (hasBeenThrown)
        {
            if (rb.linearVelocity.x < 0)
            {
                Vector3 travada = rb.linearVelocity;
                travada.x = 0f; 
                rb.linearVelocity = travada;
            }
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

        Vector3 forwardDir = Vector3.right; 
        
        // Usa a inclinação invertida calculada no Update
        Vector3 sideDir = Vector3.forward * (lateralTilt * sideSensitivity);
        
        Vector3 throwDirection = (forwardDir + sideDir).normalized;

        rb.AddForce(throwDirection * finalForce, ForceMode.VelocityChange);
        
        // Torque para frente
        rb.AddTorque(Vector3.back * finalForce * spinMultiplier, ForceMode.VelocityChange);

        if (GameManager.instance != null) 
        {
            GameManager.instance.BolaArremessada(); 
        }
    }

    public void ResetBallPublico()
    {
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        hasBeenThrown = false;
        peakAcceleration = 0f;
        isSwinging = false;
        lateralTilt = 0f; 
    }
}