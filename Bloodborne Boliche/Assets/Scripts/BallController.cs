using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Controle de Movimento")]
    [SerializeField]
    private float moveForce = 10f;
    private Vector3 moveDirection;

    [Header("Controle de Arremesso (Giroscópio)")]
    [SerializeField]
    private float throwForceMultiplier = 20f;

    private Gyroscope gyro;
    private bool isSwinging = false;
    private float peakAcceleration = 0f;
    private bool hasBeenThrown = false;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("O objeto " + name + " não tem um Rigidbody!");
        }

        if (SystemInfo.supportsGyroscope)
        {
            gyro = Input.gyro;
            gyro.enabled = true;
        } else
        {
            Debug.LogWarning("Giroscópio não suportado. O arremesso não funcionará.");
        }

    }

    void Update()
    {
        if (hasBeenThrown) return;

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        moveDirection = new Vector3(horizontalInput, 0, verticalInput);
        moveDirection.Normalize();

        if (gyro == null) return;

        if (Input.GetMouseButtonDown(0)) {
            isSwinging = true;
            peakAcceleration = 0f;
        }

        if (isSwinging)
        {
            float currentAccelereationZ = gyro.userAcceleration.z;

            if(currentAccelereationZ > peakAcceleration)
            {
                peakAcceleration = currentAccelereationZ;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isSwinging)
            {
                ThrowBall();
            }
        }
    }

    /// <summary>
    /// Nova Função: Aplica o impulso do arremesso
    /// </summary>
    void ThrowBall()
    {
        isSwinging = false;
        hasBeenThrown = true;

        // Calcula a força final
        float throwForce = peakAcceleration * throwForceMultiplier;

        // Pega a direção "para frente" da bola (que você ajustou com as teclas)
        Vector3 throwDirection = transform.forward;

        Debug.Log("Arremessando! Pico de Aceleração: " + peakAcceleration + ", Força Aplicada: " + throwForce);

        // Aplica a força de UMA VEZ SÓ (Impulse)
        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
    }



    void FixedUpdate()
    {
        if (rb != null)
        {
            if (!hasBeenThrown)
            {
                rb.AddForce(moveDirection * moveForce);
            }
        }
    }
}