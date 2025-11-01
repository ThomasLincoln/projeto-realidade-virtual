using UnityEngine;

public class BallController : MonoBehaviour
{
    // [SerializeField] permite que você ajuste esse valor no Inspector do Unity
    [SerializeField]
    private float moveForce = 10f;

    // Referência para o componente de física da bola
    private Rigidbody rb;

    // Guarda a direção do input
    private Vector3 moveDirection;

    void Start()
    {
        // Pega o componente Rigidbody automaticamente no início do jogo
        rb = GetComponent<Rigidbody>();

        // (Opcional) Adiciona uma verificação de erro
        if (rb == null)
        {
            Debug.LogError("O objeto " + name + " não tem um Rigidbody! Adicione um para o script BallController funcionar.");
        }
    }

    void Update()
    {
        // 1. Lemos o input no Update() pois ele roda a cada frame
        // Input.GetAxis("Horizontal") pega as teclas A e D (ou setas <- ->)
        float horizontalInput = Input.GetAxis("Horizontal");

        // Input.GetAxis("Vertical") pega as teclas W e S (ou setas ^ v)
        float verticalInput = Input.GetAxis("Vertical");

        // 2. Criamos um vetor de direção (X, Y, Z)
        // Note que não usamos Y, pois não queremos que a bola voe
        moveDirection = new Vector3(horizontalInput, 0, verticalInput);

        // (Opcional) Normalizar o vetor previne que o movimento diagonal seja mais rápido
        moveDirection.Normalize();
    }

    void FixedUpdate()
    {
        // 3. Aplicamos a física no FixedUpdate()
        // FixedUpdate roda em sincronia com o motor de física do Unity

        // Se o Rigidbody existir, aplicamos uma força na direção do input
        if (rb != null)
        {
            rb.AddForce(moveDirection * moveForce);
        }
    }
}