using UnityEngine;

public class PinoBoliche : MonoBehaviour
{
    private bool foiContabilizado = false;
    private Vector3 posicaoInicial;
    private Vector3 eixoParaCima; // Qual eixo do pino aponta para o céu? (Y, Z ou X?)
    private float distanciaLimiteCalculada;

    [Header("Configurações de Sensibilidade")]
    [Tooltip("Quantos % da altura do pino ele precisa se mexer para contar ponto?")]
    [Range(1f, 100f)]
    public float porcentagemMovimento = 20f;

    [Tooltip("Ângulo em graus para considerar que o pino caiu")]
    public float anguloParaCair = 45f;

    public int pontosPorPino = 10;

    void Start()
    {
        posicaoInicial = transform.position;

        // --- 1. DETECÇÃO INTELIGENTE DO EIXO ---
        // Verifica qual eixo local está mais alinhado com o Cima do Mundo (Vector3.up)
        // Isso corrige o problema do pino achar que está com 90 graus de inclinação
        Vector3[] eixosPossiveis = { Vector3.up, Vector3.forward, Vector3.right };

        float melhorAlinhamento = -1f;
        eixoParaCima = Vector3.up; // Padrão

        foreach (Vector3 eixo in eixosPossiveis)
        {
            // Transforma o eixo local para global e vê o quão "em pé" ele está
            Vector3 direcaoGlobal = transform.TransformDirection(eixo);
            float alinhamento = Mathf.Abs(Vector3.Dot(direcaoGlobal, Vector3.up));

            if (alinhamento > melhorAlinhamento)
            {
                melhorAlinhamento = alinhamento;
                eixoParaCima = eixo;
            }
        }

        Debug.Log($"[PinoBoliche] Eixo detectado como 'Cima': {eixoParaCima} (Correção automática)");

        // --- 2. CÁLCULO DO LIMITE DE MOVIMENTO ---
        Collider colisor = GetComponent<Collider>();
        if (colisor != null)
        {
            float alturaDoPino = colisor.bounds.size.y;
            distanciaLimiteCalculada = alturaDoPino * (porcentagemMovimento / 100f);
        }
        else
        {
            distanciaLimiteCalculada = 0.5f;
        }
    }

    void Update()
    {
        if (foiContabilizado) return;

        // VERIFICAÇÃO 1: Rotação (Usando o eixo inteligente que detectamos)
        // Pegamos a direção atual daquele eixo que decidimos ser o "topo" do pino
        Vector3 direcaoDoTopoAtual = transform.TransformDirection(eixoParaCima);
        float anguloAtual = Vector3.Angle(direcaoDoTopoAtual, Vector3.up);

        // VERIFICAÇÃO 2: Posição
        float distancia = Vector3.Distance(transform.position, posicaoInicial);

        // Se o ângulo for maior que o limite OU a distância for grande
        if (anguloAtual > anguloParaCair || distancia > distanciaLimiteCalculada)
        {
            ContarPonto();
            // Mostra no log exatamente o que causou a queda para ajudar a ajustar
            Debug.Log($"PINO CAIU! Motivo: {(anguloAtual > anguloParaCair ? "Inclinação" : "Movimento")} | Ângulo: {anguloAtual:F1}° | Distância: {distancia:F2}");
        }
    }

    void ContarPonto()
    {
        foiContabilizado = true;

        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.AdicionarPontos(pontosPorPino);
        }
    }
}