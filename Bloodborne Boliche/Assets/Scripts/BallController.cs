using UnityEngine;

// Renomeie a classe para MovendoBolas se seu arquivo tiver esse nome
public class BallController : MonoBehaviour
{
    // --- Controle de Movimento (Teclado) ---
    [Header("Controle de Movimento")]
    [SerializeField]
    private float moveForce = 10f;
    private Vector3 moveDirection;

    // --- Controle de Arremesso (Giroscópio do CELULAR) ---
    [Header("Controle de Arremesso (Giroscópio)")]
    [SerializeField]
    private float throwForceMultiplier = 20f; // Multiplicador da força

    // <-- CORREÇÃO 1: Adiciona um limiar (threshold) mínimo
    [SerializeField]
    private float minThrowThreshold = 0.1f; // Aceleração mínima (absoluta) para contar como arremesso

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
        if (rb == null)
        {
            Debug.LogError("O objeto " + name + " não tem um Rigidbody!");
        }

        initialPosition = transform.position;
        initialRotation = transform.rotation;

        // Verifica se o giroscópio existe e o ativa
        if (SystemInfo.supportsGyroscope)
        {
            gyro = Input.gyro;
            gyro.enabled = true;
        }
        else
        {
            Debug.LogError("Giroscópio não suportado neste dispositivo.");
        }
    }

    void Update()
    {
        // Se a bola já foi arremessada, não faz nada
        if (hasBeenThrown)
        {
            // Detecta o toque para resetar a bola
            if (Input.GetMouseButtonDown(0)) // "GetMouseButtonDown(0)" é o TOQUE na tela
            {
                ResetBall();
            }
            return;
        }

        // --- 1. Lógica de Movimento (Teclado) ---
        float horizontalInput = Input.GetAxis("Horizontal"); // Teclas A/D
        float verticalInput = Input.GetAxis("Vertical");     // Teclas W/S
        moveDirection = new Vector3(horizontalInput, 0, verticalInput);

        // --- 2. Lógica de Arremesso (Giroscópio do Celular) ---
        if (gyro == null) return;

        // Detecta o TOQUE na tela para começar o "swing"
        if (Input.GetMouseButtonDown(0))
        {
            isSwinging = true;
            peakAcceleration = 0f; // Reseta o pico a cada novo balanço
            Debug.Log("[Arremessar] TOQUE INICIADO");
        }

        // Durante o "swing" (enquanto segura o dedo)
        if (isSwinging)
        {
            // <-- CORREÇÃO 2: Usar o valor ABSOLUTO (Mathf.Abs)
            // Isso captura o "vigor" do balanço, não importa se é para frente ou para trás.
            float currentAccelerationZ = Mathf.Abs(gyro.userAcceleration.z);

            // Log melhorado para vermos o valor real e o absoluto
            // Descomente a linha abaixo se quiser depurar os valores em tempo real
            // Debug.Log($"[Giroscópio] Aceleração Z (Real): {gyro.userAcceleration.z} | (Absoluta): {currentAccelerationZ}");

            if (currentAccelerationZ > peakAcceleration)
            {
                peakAcceleration = currentAccelerationZ; // Salva o maior pico de vigor
            }
        }

        // Detecta o SOLTAR o dedo da tela para arremessar
        if (Input.GetMouseButtonUp(0))
        {
            if (isSwinging)
            {
                ThrowBall();
            }
            isSwinging = false;
            Debug.Log("[Arremessar] TOQUE SOLTO");
        }
    }

    void ThrowBall()
    {
        float throwForce = peakAcceleration * throwForceMultiplier;

        // <-- CORREÇÃO 3: Verificar se o pico de aceleração atingiu o mínimo
        if (peakAcceleration < minThrowThreshold)
        {
            Debug.LogWarning($"ARREMESSO CANCELADO. Pico de Aceleração ({peakAcceleration}) muito baixo.");
            // Não fazemos nada. A bola não é arremessada.
            // O jogador pode tentar de novo.
            return; // Sai da função sem arremessar
        }

        // Se o pico foi alto o suficiente, arremessa:

        // <-- CORREÇÃO 4: Mover 'hasBeenThrown' para DEPOIS da verificação
        // Só marca como arremessada se o arremesso for válido.
        hasBeenThrown = true;

        Debug.LogWarning($"ARREMESSANDO! Pico de Aceleração: {peakAcceleration}, Força Aplicada: {throwForce}");

        rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
    }

    private void ResetBall()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        hasBeenThrown = false;
        moveDirection = Vector3.zero;
        Debug.LogWarning("--- BOLA RESETADA ---");
    }


    void FixedUpdate()
    {
        if (rb != null && !hasBeenThrown)
        {
            rb.AddForce(moveDirection * moveForce);
        }
    }
}