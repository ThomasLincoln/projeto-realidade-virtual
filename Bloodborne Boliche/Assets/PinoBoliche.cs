using UnityEngine;

public class PinoBoliche : MonoBehaviour
{
    private bool foiContabilizado = false; // Para não contar o mesmo pino duas vezes
    private Vector3 posicaoInicial;

    [Header("Configurações")]
    public float anguloParaCair = 45f; // Se inclinar mais que 45 graus, conta como queda
    public float distanciaParaMover = 0.5f; // Se mover mais que 0.5 unidades, conta
    public int pontosPorPino = 10;

    void Start()
    {
        // Salva a posição onde o pino começou
        posicaoInicial = transform.position;
    }

    void Update()
    {
        // Se já contamos este pino, não faz nada
        if (foiContabilizado) return;

        // VERIFICAÇÃO 1: Rotação (O pino tombou?)
        // Calcula o ângulo entre o vetor "Cima" do pino e o "Cima" do mundo
        float anguloAtual = Vector3.Angle(transform.up, Vector3.up);

        // VERIFICAÇÃO 2: Posição (O pino saiu do lugar?)
        float distancia = Vector3.Distance(transform.position, posicaoInicial);

        // Se o ângulo for maior que o limite OU a distância for grande
        if (anguloAtual > anguloParaCair || distancia > distanciaParaMover)
        {
            ContarPonto();
        }
    }

    void ContarPonto()
    {
        foiContabilizado = true;

        // Chama o ScoreManager para somar os pontos
        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.AdicionarPontos(pontosPorPino);
        }

        // Opcional: Mudar a cor ou tocar um som para feedback visual
        Debug.Log(gameObject.name + " caiu ou saiu do lugar!");
    }
}