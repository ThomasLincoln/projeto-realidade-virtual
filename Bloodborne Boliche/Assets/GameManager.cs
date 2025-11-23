using UnityEngine;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Referências")]
    public BallController scriptBola;
    public Rigidbody bolaRb;

    [Header("UI")]
    public TextMeshProUGUI textoPontuacao;
    public TextMeshProUGUI textoAvisos;

    private PinoBoliche[] todosOsPinos;
    private int pontuacaoTotal = 0;
    private int pinosDerrubadosNestaRodada = 0;
    private int tentativaAtual = 1;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        todosOsPinos = FindObjectsByType<PinoBoliche>(FindObjectsSortMode.None);

        // Auto-encontrar a bola para evitar erros de referência nula
        if (scriptBola == null) scriptBola = FindObjectOfType<BallController>();
        if (scriptBola != null && bolaRb == null) bolaRb = scriptBola.GetComponent<Rigidbody>();

        AtualizarUI();
    }

    public void RegistrarPinoCaido(int pontos)
    {
        pontuacaoTotal += pontos;
        pinosDerrubadosNestaRodada++;
        AtualizarUI();
    }

    public void BolaArremessada()
    {
        StartCoroutine(ProcessarJogada());
    }

    IEnumerator ProcessarJogada()
    {
        // 1. Espera a bola sair da mão
        yield return new WaitForSeconds(1.0f);

        float tempoLimite = 12f;
        float tempoAtual = 0f;

        // 2. Loop: Espera ENQUANTO a bola se move E não caiu na valeta
        while (bolaRb.linearVelocity.magnitude > 0.1f && bolaRb.transform.position.y > -1f && tempoAtual < tempoLimite)
        {
            tempoAtual += 0.2f;
            yield return new WaitForSeconds(0.2f);
        }

        Debug.Log("Bola parou ou caiu! Calculando...");

        // Pequena espera para os pinos terminarem de cair
        yield return new WaitForSeconds(1.5f);

        CalcularRegras();
    }

    void CalcularRegras()
    {
        bool fezStrike = (pinosDerrubadosNestaRodada >= todosOsPinos.Length);

        // Se caiu na valeta rápido, reseta rápido (1s). Se parou na pista, espera mais (3s).
        float tempoEspera = (bolaRb.transform.position.y <= -1f) ? 1.0f : 3.0f;

        if (tentativaAtual == 1)
        {
            if (fezStrike)
            {
                MostrarAviso("STRIKE!");
                StartCoroutine(ResetarJogo(tempoEspera, true));
            }
            else
            {
                if (bolaRb.transform.position.y <= -1f && pinosDerrubadosNestaRodada == 0)
                    MostrarAviso("Canaleta!");
                else
                    MostrarAviso("2ª Tentativa");

                StartCoroutine(ResetarJogo(tempoEspera, false));
            }
        }
        else // 2ª tentativa
        {
            if (pinosDerrubadosNestaRodada >= todosOsPinos.Length) MostrarAviso("SPARE!");
            else MostrarAviso("Fim do Round");

            StartCoroutine(ResetarJogo(tempoEspera, true));
        }
    }

    IEnumerator ResetarJogo(float tempo, bool novoRoundCompleto)
    {
        yield return new WaitForSeconds(tempo);

        if (novoRoundCompleto)
        {
            tentativaAtual = 1;
            pinosDerrubadosNestaRodada = 0;
            foreach (var pino in todosOsPinos) pino.ResetarCompleto();
            MostrarAviso("Jogue!");
        }
        else
        {
            tentativaAtual = 2;
            foreach (var pino in todosOsPinos) pino.OcultarSeCaiu();
        }

        if (scriptBola != null) scriptBola.ResetBallPublico();
    }

    void AtualizarUI()
    {
        if (textoPontuacao != null) textoPontuacao.text = "Pontos: " + pontuacaoTotal;
    }

    void MostrarAviso(string mensagem)
    {
        if (textoAvisos != null)
        {
            textoAvisos.text = mensagem;
            textoAvisos.alpha = 1f;
            StartCoroutine(SumirTexto());
        }
    }

    IEnumerator SumirTexto()
    {
        yield return new WaitForSeconds(2.0f);
        float alpha = 1f;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime;
            if (textoAvisos != null) textoAvisos.alpha = alpha;
            yield return null;
        }
    }
}