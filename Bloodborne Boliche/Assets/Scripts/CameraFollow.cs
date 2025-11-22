using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Configurações")]
    public Transform target;       // Arraste sua bola (gam005_ball00) para aqui no Inspector
    public float smoothSpeed = 0.125f; // Suavidade (0 = travado, 1 = muito lento)

    [Header("Debug (Não precisa mexer)")]
    public Vector3 offset;         // Vai ser calculado automaticamente

    void Start()
    {
        // AQUI ESTÁ A MUDANÇA:
        // O script calcula a diferença exata entre a câmera e a bola no momento que o jogo começa.
        if (target != null)
        {
            offset = transform.position - target.position;
        }
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // Calcula a posição onde a câmera deve ir
            Vector3 desiredPosition = target.position + offset;

            // Opcional: Se você quiser que a câmera siga a bola mas NÃO mude de altura (fique fixa no chão),
            // remova as duas barras (//) da linha abaixo:
            // desiredPosition.y = transform.position.y;

            // Move a câmera suavemente até a posição desejada
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }
}