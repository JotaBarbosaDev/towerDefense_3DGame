using UnityEngine;

public class TowerRotation : MonoBehaviour
{
    public Transform[] npcs;     // Array de NPCs
    public float alcance = 20f;  // Distância do alcance da torre
    public float anguloMaximo = 90f; // Ângulo máximo de visão (frente da torre)

    void Update()
    {
        Transform npcAlvo = null;
        float distanciaMinima = Mathf.Infinity;

        foreach (Transform npc in npcs)
        {
            // Calcular a distância entre a torre e o NPC
            float distancia = Vector3.Distance(transform.position, npc.position);

            // Verificar se o NPC está dentro do alcance
            if (distancia <= alcance)
            {
                // Calcular o ângulo entre a direção da torre e a posição do NPC
                Vector3 direcao = npc.position - transform.position;
                float angulo = Vector3.Angle(transform.forward, direcao);

                // Verificar se o NPC está à frente da torre (dentro do alcance e ângulo)
                if (angulo <= anguloMaximo)
                {
                    // Se for o NPC mais próximo à frente
                    if (distancia < distanciaMinima)
                    {
                        distanciaMinima = distancia;
                        npcAlvo = npc;
                    }
                }
            }
        }

        // Se a torre tem um alvo
        if (npcAlvo != null)
        {
            // Calcula a direção do NPC em relação à torre
            Vector3 direcao = npcAlvo.position - transform.position;

            // Gira a torre para olhar para o NPC
            Quaternion rotacao = Quaternion.LookRotation(direcao);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacao, Time.deltaTime * 5f); // Ajusta a velocidade de rotação
        }
    }
}
