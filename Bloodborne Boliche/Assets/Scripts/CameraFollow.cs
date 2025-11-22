using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Configurações")]
    public Transform target;       
    public float smoothSpeed = 0.125f;
    public float stopAfterSeconds = 3.0f; // Tempo até a câmera parar

    [Header("Debug (Monitoramento)")]
    public Vector3 offset;
    public bool isFollowing = true;   
    private float timer = 0f;         
    private bool timerStarted = false; 
    
    // VARIÁVEIS INVERTIDAS
    private float initialCameraZ;     // Agora travamos o Z (laterais)
    private float initialBallX;       // Usamos o X para detectar movimento da bola

    void Start()
    {
        if (target != null)
        {
            // Calcula a distância inicial
            offset = transform.position - target.position;
            
            // Salva a posição X inicial da bola (para saber quando ela andou na pista)
            initialBallX = target.position.x;
        }

        // Salva a posição Z inicial da Câmera. 
        // A câmera vai ficar travada nessa linha lateral (não vai para a sarjeta).
        initialCameraZ = transform.position.z;
    }

    void LateUpdate()
    {
        if (target == null || !isFollowing) return;

        // --- 1. Lógica do Temporizador (Agora verificando o eixo X) ---
        
        // Verifica se a bola se moveu no eixo X (usamos Mathf.Abs para funcionar independente do lado)
        if (!timerStarted && Mathf.Abs(target.position.x - initialBallX) > 0.1f)
        {
            timerStarted = true;
        }

        if (timerStarted)
        {
            timer += Time.deltaTime;
            if (timer >= stopAfterSeconds)
            {
                isFollowing = false;
                return; 
            }
        }

        // --- 2. Lógica de Movimento INVERTIDA (Segue X, Trava Z) ---

        Vector3 desiredPosition = transform.position;

        // O X agora SEGUE a bola (Pista)
        desiredPosition.x = target.position.x + offset.x;

        // O Z agora fica TRAVADO (Laterais)
        desiredPosition.z = initialCameraZ;

        // Mantém a altura original
        desiredPosition.y = transform.position.y;

        // Move suavemente
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}