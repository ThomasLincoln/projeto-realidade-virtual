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

    [Tooltip("Giro da bola (Curva).")]
    [SerializeField] private float spinMultiplier = 20f;

    [Header("Sensação de Aceleração")]
    [Tooltip("1 = Linear. 2 = Exponencial. Quanto maior, mais 'explosivo' é o arremesso forte.")]
    [SerializeField] private float curvaDePotencia = 2.0f;

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
        rb.maxAngularVelocity = 50f;

        // IMPORTANTE: Mantém a bola kinematic até ser arremessada
        rb.isKinematic = true;

        initialPosition = transform.position;
        initialRotation = transform.rotation;

        // FORÇA O RESET NO INÍCIO
        hasBeenThrown = false;
        peakAcceleration = 0f;
        isSwinging = false;

        // Garante que o input funciona
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("🎳 BOLA RESETADA! hasBeenThrown = false");

        StartCoroutine(IniciarGiroscopio());
    }

    void OnEnable()
    {
        // Garante reset quando o objeto é reativado
        hasBeenThrown = false;
        isSwinging = false;
        Debug.Log("🔄 OnEnable - Bola resetada");
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
        // DEBUG: Mostra o estado da bola
        Debug.Log($"hasBeenThrown: {hasBeenThrown}");

        // TESTE DE INPUT PRIMEIRO (antes de qualquer bloqueio)
        if (Input.GetKey(KeyCode.A))
        {
            Debug.Log("🔴 TECLA A DETECTADA!");
        }
        if (Input.GetKey(KeyCode.D))
        {
            Debug.Log("🔴 TECLA D DETECTADA!");
        }

        if (hasBeenThrown)
        {
            Debug.Log("⚠️ Movimento bloqueado - bola já foi arremessada");
            return;
        }

        // TESTE COM TECLAS DIRETAS (sem Input Manager)
        float horizontalInput = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            horizontalInput = -1f;
            Debug.Log("✅ MOVENDO ESQUERDA");
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            horizontalInput = 1f;
            Debug.Log("✅ MOVENDO DIREITA");
        }

        // 1. Movimento Lateral
        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            Vector3 newPos = transform.position;
            newPos.z += horizontalInput * moveSpeed * Time.deltaTime;

            // Aplica os limites
            newPos.z = Mathf.Clamp(newPos.z, minZ, maxZ);

            Debug.Log($"🎯 Nova posição Z: {newPos.z:F2}");
            transform.position = newPos;
        }

        // 2. Leitura do Arremesso
        if (Input.GetMouseButtonDown(0))
        {
            isSwinging = true;
            peakAcceleration = 0f;
            Debug.Log("🎳 Começou a balançar!");
        }

        if (isSwinging && gyro != null)
        {
            float currentAccel = gyro.userAcceleration.magnitude;
            if (currentAccel > peakAcceleration) peakAcceleration = currentAccel;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isSwinging) ThrowBall();
            isSwinging = false;
        }

        // Atalho de Teste (Espaço)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            peakAcceleration = 3f;
            ThrowBall();
        }
    }

    void FixedUpdate()
    {
        // Não precisa mais do FixedUpdate para movimento
        // O movimento agora é direto no Update
    }

    void ThrowBall()
    {
        if (hasBeenThrown) return;

        float forcaExponencial = Mathf.Pow(peakAcceleration, curvaDePotencia);
        float finalForce = forcaExponencial * throwForceMultiplier;

        if (finalForce < 2f) return;

        hasBeenThrown = true;
        rb.isKinematic = false;

        // Direção do arremesso (para frente, eixo X)
        Vector3 direction = Vector3.right; // Eixo X positivo

        rb.AddForce(direction * finalForce, ForceMode.VelocityChange);
        rb.AddTorque(Vector3.forward * finalForce * spinMultiplier, ForceMode.VelocityChange);

        Debug.Log($"ARREMESSO! Força: {finalForce:F2} | Posição Z: {transform.position.z:F2}");

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
    }

    // Visualiza os limites no Editor (muito útil!)
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        float y = 0.5f;
        float xStart = -15f;
        float xEnd = 10f;

        // Linha do limite esquerdo (minZ)
        Gizmos.DrawLine(new Vector3(xStart, y, minZ), new Vector3(xEnd, y, minZ));

        // Linha do limite direito (maxZ)
        Gizmos.DrawLine(new Vector3(xStart, y, maxZ), new Vector3(xEnd, y, maxZ));
    }
}