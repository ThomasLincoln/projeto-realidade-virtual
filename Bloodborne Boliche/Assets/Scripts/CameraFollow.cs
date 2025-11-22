using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Alvos")]
    public Transform target;           
    
    [Header("Configurações")]
    public float smoothSpeed = 0.125f; 
    
    // AGORA É X: O limite onde a câmera deve parar de avançar na pista
    public float xStopPosition = 0.0f; 

    [Header("Debug")]
    public Vector3 offset;
    private float initialCameraZ;      // Vamos travar o Z (Laterais)
    private float initialCameraHeight; // Vamos travar o Y (Altura)

    void Start()
    {
        if (target != null)
        {
            offset = transform.position - target.position;
        }

        // Salva a posição Z (lateral) e Y (altura) iniciais.
        // Assim a câmera nunca sai do centro da pista e nunca muda de altura.
        initialCameraZ = transform.position.z;
        initialCameraHeight = transform.position.y;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // 1. Calcula onde a câmera quer ir no eixo X (Avançar na pista)
            float desiredX = target.position.x + offset.x;

            // 2. O LIMITADOR: Impede que o X passe do ponto que você definiu.
            // Como você está indo do negativo (-13) para o positivo, usamos o "Min" para limitar o máximo.
            // Se a câmera parar cedo demais ou não parar, me avise, depende do lado da pista.
            float clampedX = Mathf.Clamp(desiredX, -1000f, xStopPosition);

            // 3. Monta a posição final
            // X = Calculado com limite (Movimento)
            // Y = Fixo (Altura)
            // Z = Fixo (Centro da pista)
            Vector3 desiredPosition = new Vector3(clampedX, initialCameraHeight, initialCameraZ);

            // 4. Move a câmera
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }
}