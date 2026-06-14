# Isekai Desnecessário — Contexto do Projeto

## O que é
RPG habit tracker com XP, ranks e gacha.
Documentação completa no vault: `Obsidian/1. Geral/2. Projetos/3. Solo Leveling App/`

## Ranks
F → E → D → C → B → A → S → SS → SSS

## Fórmula de XP
`XP_próximo_nível = round(XP_anterior * 1.35)`

## Banco de Dados (tabelas principais)
- **Perfil**: Id, Nome, Xp, Moedas, Rank, Título, Nível, Próximo Nível XP
- **BonsHábitos**: Id, Habito, Xp, Frequência, Streak
- **MausHábitos**: Id, Habito, Xp, Frequência, Streak
- **Missões**: Id, Título, TipoId (FK), RecompensaXP, Streak
- **Recompensas**: Id, Recompensa, Preço

## Mecânicas
- **Gacha**: libera a cada 3 dias com 300 XP acumulados
- **Moedas**: ganhas por completar missões, gastas em recompensas

## Stack (a definir)
- Backend: .NET / C#
- Frontend: Angular
- ORM: Entity Framework
