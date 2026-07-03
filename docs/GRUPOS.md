# Grupos — Isekai Desnecessário

> Feature planejada. Nenhum código implementado ainda. Este documento registra a visão e as decisões de design tomadas.

---

## Conceito

Grupos são uma assinatura **separada do VIP**, paga pelo **Organizador**. Permitem que um conjunto de pessoas compartilhe missões, hábitos e recompensas exclusivos, com economia (XP e moeda) própria e ranking interno.

---

## Planos

| Plano | Membros máximos | Upgrade disponível para |
|-------|----------------|------------------------|
| Starter | 5 | Standard, Pro, Max |
| Standard | 10 | Pro, Max |
| Pro | 30 | Max |
| Max | 50 | — |

- O Organizador escolhe o plano na hora da compra.
- Upgrade disponível a qualquer momento (pagando a diferença ou o novo valor).
- Downgrade **não** disponível.

---

## Papéis dentro do grupo

| Papel | Cria conteúdo | Remove membros | Paga |
|-------|--------------|---------------|------|
| Organizador | ✅ | ✅ | ✅ |
| Membro | ❌ | ❌ | ❌ |

> Roles globais da plataforma (VIP, Admin, Moderador) **não concedem privilégios extras** dentro do grupo. O único critério é ser Organizador.

---

## Economia interna

Cada grupo tem sua própria economia, completamente separada da economia principal do jogador.

- **XP de grupo** — ganho completando hábitos e missões do grupo. Não vai para o XP principal do perfil.
- **Moeda de grupo** — ganha junto com o XP de grupo. Só pode ser gasta nas recompensas criadas pelo Organizador do grupo.
- **Ranking interno** — ordenado pelo XP de grupo acumulado, sem reset por temporada.

---

## Conteúdo criado pelo Organizador

O Organizador pode criar, editar e excluir:

- **Hábitos do grupo** — aparecem para todos os membros.
- **Missões do grupo** — aparecem para todos os membros; o Organizador pode definir prazo.
- **Recompensas do grupo** — compradas com a moeda de grupo.

Nenhum membro pode criar conteúdo. O conteúdo existe apenas dentro do grupo.

---

## Feed do grupo

Timeline de atividade compartilhada entre todos os membros — funciona como um "diário de conquistas do grupo".

Exemplos de eventos exibidos:
- *"Luan completou · Ler 30 minutos · há 2h"*
- *"Ana subiu para 1º lugar no ranking · hoje"*
- *"Carlos resgatou · Sorvete · há 5h"*

---

## Convites & Acesso

- Convite por **nome de usuário** (se a feature de Amigos for implementada, integrará naturalmente aqui).
- Grupo com capacidade cheia rejeita novos convites automaticamente.
- Membro pode sair a qualquer momento.
- Organizador pode remover membros manualmente.

### Cancelamento do plano

Quando o Organizador cancela ou o pagamento falha:
- Todos os membros **perdem o acesso imediatamente**.
- Todos os membros recebem uma **notificação** informando o motivo.
- O conteúdo do grupo (missões, hábitos, recompensas) é preservado no banco por um período de graça (a definir) caso o Organizador reative.

---

## Billing

- Processado via **RevenueCat** (mesma infraestrutura planejada para o VIP).
- Preços por plano a definir.
- Webhooks do RevenueCat notificam o backend nos eventos: `INITIAL_PURCHASE`, `RENEWAL`, `CANCELLATION`, `EXPIRATION`.

---

## Roadmap de implementação (quando chegar a hora)

1. **Banco de dados**
   - Tabela `Grupos` (id, nome, organizadorId, plano, maxMembros, criadoEm)
   - Tabela `GrupoMembros` (grupoId, perfilId, entradaEm)
   - Tabela `HabitosGrupo`, `MissoesGrupo`, `RecompensasGrupo`
   - Tabela `XpGrupo` e `MoedaGrupo` por membro por grupo
   - Tabela `FeedGrupo` (eventos)

2. **Backend**
   - CRUD de grupos (criar, upgrade de plano, encerrar)
   - Endpoints de convite (convidar por username, aceitar, recusar)
   - CRUD de conteúdo do grupo (só Organizador)
   - Endpoints de completar hábito/missão do grupo (qualquer membro)
   - Feed do grupo
   - Ranking interno
   - Webhook do RevenueCat para ativar/desativar acesso

3. **Frontend**
   - Tela de listagem de grupos do usuário
   - Tela interna do grupo (feed, ranking, conteúdo, membros)
   - Fluxo de compra / upgrade de plano
   - Notificações de cancelamento

4. **Billing**
   - Configurar produtos no RevenueCat (4 planos)
   - Integrar Google Play Billing + Stripe Web via RevenueCat

---

## O que ainda está em aberto

- Preços de cada plano.
- Feature de **Amigos** (mencionada como futura — convite por amigo se encaixa aqui quando estiver pronta).
- Período de graça após cancelamento (quanto tempo o conteúdo fica preservado).
