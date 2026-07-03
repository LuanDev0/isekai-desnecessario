# Grupos — Isekai Desnecessário

> **Status (v0.21):** MVP implementado — banco (migration `AdicionarGrupos`), backend (`GruposController` + `GrupoConteudoController` + `GrupoService`) e frontend (`/grupos` e `/grupos/:id`). **Billing pendente** — criação/upgrade liberados como stub até o RevenueCat entrar. Endpoints em [API.md](API.md#grupos--apigrupos-v021), telas em [FRONTEND.md](FRONTEND.md#grupos-grupos-e-gruposid).

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

- Convite por **e-mail da conta** (o e-mail é o identificador único; quando a feature de Amigos existir, o convite por amigo se integra aqui). O destinatário recebe uma notificação no sininho e aceita escolhendo o perfil (o app usa o perfil ativo).
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

## Roadmap de implementação

1. **Banco de dados** ✅ *(migration `AdicionarGrupos`)*
   - `Grupos` (nome, organizadorUsuarioId, plano, maxMembros, ativo, criadoEm)
   - `GrupoMembros` (grupoId, perfilId únicos; **xpGrupo/moedasGrupo do membro vivem aqui**)
   - `GrupoHabitos`, `GrupoMissoes`, `GrupoRecompensas` (conteúdo exclusivo)
   - `GrupoHabitoExecucoes`, `GrupoMissaoConclusoes`, `GrupoRecompensaResgates` (estado por membro)
   - `GrupoConvites` (por conta/e-mail; Pendente/Aceito/Recusado)
   - `GrupoFeedEventos` (timeline denormalizada)

2. **Backend** ✅
   - CRUD de grupos (criar, renomear, upgrade só para plano maior, cancelar/reativar, excluir)
   - Convites por e-mail (convidar, aceitar, recusar) com notificação no sininho
   - CRUD de conteúdo do grupo (só Organizador — roles globais não têm privilégio)
   - Completar hábito (cooldown por frequência) / missão (1× por membro, prazo) / resgatar recompensa
   - Feed do grupo + ranking interno (por XP do grupo)
   - Notificações de cancelamento/reativação/exclusão/remoção
   - 🚧 Webhook do RevenueCat para ativar/desativar acesso

3. **Frontend** ✅
   - `/grupos` — listagem, convites pendentes e criação
   - `/grupos/:id` — abas Feed, Ranking, Conteúdo e Gerenciar (organizador)
   - Item **Grupos** na navbar

4. **Billing** 🚧
   - Configurar produtos no RevenueCat (4 planos)
   - Integrar Google Play Billing + Stripe Web via RevenueCat
   - Pontos de cobrança já mapeados: `POST /grupos` (criação) e `POST /grupos/{id}/upgrade`

---

## O que ainda está em aberto

- Preços de cada plano.
- Billing real (RevenueCat) — hoje a criação/upgrade é liberada (stub de beta).
- Feature de **Amigos** (mencionada como futura — convite por amigo se encaixa aqui quando estiver pronta).
- Período de graça após cancelamento (hoje o conteúdo fica preservado indefinidamente enquanto o grupo existir; excluir o grupo apaga tudo).
