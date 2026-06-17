
INSERT INTO BonsHabitos (PerfilId, Habito, Xp, Frequencia, Streak, UltimaExecucao, AtributoId) VALUES
-- 🧠 Inteligência — Estudar, fazer cursos, resolver exercícios
(4, 'Estudar por 30 minutos', 30, 'Livre', 0, NULL, 1),
(4, 'Assistir aula ou curso', 30, 'Diário', 0, NULL, 1),
(4, 'Resolver exercícios ou problemas', 25, 'Diário', 0, NULL, 1),
(4, 'Revisar anotações do dia', 20, 'Diário', 0, NULL, 1),

-- 📚 Sabedoria — Ler livros, podcasts, reflexão
(4, 'Ler 20 páginas de um livro', 25, 'Livre', 0, NULL, 2),
(4, 'Ouvir um podcast educativo', 20, 'Diário', 0, NULL, 2),
(4, 'Escrever no diário / reflexão do dia', 20, 'Diário', 0, NULL, 2),
(4, 'Aprender algo novo', 25, 'Livre', 0, NULL, 2),

-- 💪 Físico — Treinar, academia, exercícios físicos
(4, 'Ir à academia', 40, 'Diário', 0, NULL, 3),
(4, 'Fazer exercício em casa', 30, 'Diário', 0, NULL, 3),
(4, 'Caminhar ou correr por 30 minutos', 25, 'Diário', 0, NULL, 3),
(4, 'Fazer alongamento', 15, 'Diário', 0, NULL, 3),

-- ⚙️ Disciplina — Tarefas domésticas, rotina, pontualidade
(4, 'Acordar no horário planejado', 20, 'Diário', 0, NULL, 4),
(4, 'Lavar a louça / limpar a cozinha', 15, 'Diário', 0, NULL, 4),
(4, 'Organizar o quarto', 15, 'Diário', 0, NULL, 4),
(4, 'Cumprir a rotina do dia', 25, 'Diário', 0, NULL, 4),
(4, 'Organizar o quarto / espaço de trabalho', 15, 'Diário', 0, NULL, 4),
(4, 'Planejar o dia seguinte', 15, 'Diário', 0, NULL, 4),

-- 🎯 Foco — Projeto, sem celular, concentração
(4, 'Trabalhar no projeto por 1 hora', 35, 'Diário', 0, NULL, 5),
(4, 'Não usar celular pela manhã', 25, 'Diário', 0, NULL, 5),
(4, 'Fazer uma sessão Pomodoro', 30, 'Diário', 0, NULL, 5),
(4, 'Finalizar uma tarefa pendente', 25, 'Diário', 0, NULL, 5),

-- ❤️ Vitalidade — Sono, hidratação, pausas
(4, 'Dormir antes da meia-noite', 20, 'Diário', 0, NULL, 6),
(4, 'Beber 2L de água', 20, 'Diário', 0, NULL, 6),
(4, 'Fazer uma pausa ativa durante o dia', 15, 'Diário', 0, NULL, 6),
(4, 'Meditar por 10 minutos', 25, 'Diário', 0, NULL, 6);

-----------------------------------------------------------------------------------------------------
INSERT INTO MausHabitos (PerfilId, Habito, Xp, Frequencia, Streak, UltimaExecucao, AtributoId) VALUES
-- 🧠 Inteligência
(4, 'Estudar sem foco / só enrolar', 20, 'Diário', 0, NULL, 1),
(4, 'Passar o dia sem aprender nada novo', 15, 'Diário', 0, NULL, 1),
(4, 'Assistir aula sem prestar atenção', 20, 'Diário', 0, NULL, 1),
(4, 'Deixar exercícios acumulando sem fazer', 25, 'Diário', 0, NULL, 1),

-- 📚 Sabedoria
(4, 'Trocar leitura por scroll infinito', 20, 'Diário', 0, NULL, 2),
(4, 'Não refletir sobre o dia', 15, 'Diário', 0, NULL, 2),
(4, 'Ouvir podcast distraído, sem absorver nada', 15, 'Diário', 0, NULL, 2),
(4, 'Tomar decisão impulsiva sem pensar', 25, 'Diário', 0, NULL, 2),

-- 💪 Físico
(4, 'Ficar sedentário o dia todo', 30, 'Diário', 0, NULL, 3),
(4, 'Pular o treino sem motivo', 25, 'Diário', 0, NULL, 3),
(4, 'Ficar mais de 2h sentado sem levantar', 20, 'Diário', 0, NULL, 3),
(4, 'Não fazer nenhum alongamento no dia', 15, 'Diário', 0, NULL, 3),

-- ⚙️ Disciplina
(4, 'Procrastinar tarefas importantes', 25, 'Diário', 0, NULL, 4),
(4, 'Deixar louça/quarto acumulando', 15, 'Diário', 0, NULL, 4),
(4, 'Atrasar compromisso ou deadline', 20, 'Diário', 0, NULL, 4),
(4, 'Dormir muito além do horário planejado', 20, 'Diário', 0, NULL, 4),

-- 🎯 Foco
(4, 'Ficar no celular no horário de trabalho', 25, 'Diário', 0, NULL, 5),
(4, 'Abrir redes sociais ao acordar', 20, 'Diário', 0, NULL, 5),
(4, 'Trocar tarefa importante por algo fácil', 20, 'Diário', 0, NULL, 5),
(4, 'Passar mais de 2h em entretenimento passivo', 25, 'Diário', 0, NULL, 5),

-- ❤️ Vitalidade
(4, 'Dormir depois da 1h da manhã', 25, 'Diário', 0, NULL, 6),
(4, 'Beber menos de 1L de água', 20, 'Diário', 0, NULL, 6),
(4, 'Pular refeição ou comer mal', 20, 'Diário', 0, NULL, 6),
(4, 'Não fazer nenhuma pausa durante o dia', 15, 'Diário', 0, NULL, 6);

------------------------------------------------------------------------------------

-- ===========================
-- MISSÕES PRINCIPAIS
-- ===========================
-- São os grandes arcos. Moedas altas, prazo longo.

INSERT INTO Missoes (PerfilId, Titulo, TipoId, RecompensaXp, RecompensaMoedas, Streak, Concluida, ConcluidaEm, DataLimite, MissaoPrincipalId, AtributoId) VALUES

-- [MP-1] Arco do Estudioso
(4, 'Dominar um novo conteúdo do zero ao fim', 1, 500, 200, 0, 0, NULL, NULL, NULL, 1),

-- [MP-2] Arco do Corpo Forjado
(4, 'Manter rotina de exercícios por 30 dias', 1, 500, 200, 0, 0, NULL, NULL, NULL, 3),

-- [MP-3] Arco da Mente Afiada
(4, 'Concluir um projeto do início ao fim', 1, 600, 250, 0, 0, NULL, NULL, NULL, 5),

-- [MP-4] Arco do Herói Disciplinado
(4, 'Manter rotina completa por 21 dias', 1, 550, 220, 0, 0, NULL, NULL, NULL, 4);

-- ===========================
-- MISSÕES SECUNDÁRIAS
-- (interligadas às principais via MissaoPrincipalId)
-- ===========================
-- ⚠️ Ajuste os IDs de MissaoPrincipalId conforme os IDs gerados acima no seu banco.
-- Aqui assumindo MP-1=1, MP-2=2, MP-3=3, MP-4=4

INSERT INTO Missoes (PerfilId, Titulo, TipoId, RecompensaXp, RecompensaMoedas, Streak, Concluida, ConcluidaEm, DataLimite, MissaoPrincipalId, AtributoId) VALUES

-- Secundárias do Arco do Estudioso (MP-1)
(4, 'Estudar por 7 dias seguidos', 2, 100, 40, 0, 0, NULL, NULL, 1, 1),
(4, 'Completar um módulo ou capítulo inteiro', 2, 80, 30, 0, 0, NULL, NULL, 1, 1),
(4, 'Fazer anotações organizadas de um conteúdo', 2, 60, 25, 0, 0, NULL, NULL, 1, 2),
(4, 'Resolver 10 exercícios do conteúdo estudado', 2, 80, 35, 0, 0, NULL, NULL, 1, 1),

-- Secundárias do Arco do Corpo Forjado (MP-2)
(4, 'Treinar 5 vezes em uma semana', 2, 100, 40, 0, 0, NULL, NULL, 2, 3),
(4, 'Fazer alongamento por 7 dias seguidos', 2, 60, 25, 0, 0, NULL, NULL, 2, 3),
(4, 'Caminhar ou correr por 7 dias seguidos', 2, 80, 30, 0, 0, NULL, NULL, 2, 3),
(4, 'Beber 2L de água por 7 dias seguidos', 2, 60, 25, 0, 0, NULL, NULL, 2, 6),

-- Secundárias do Arco da Mente Afiada (MP-3)
(4, 'Trabalhar no projeto por 5 dias seguidos', 2, 100, 40, 0, 0, NULL, NULL, 3, 5),
(4, 'Concluir uma etapa importante do projeto', 2, 120, 50, 0, 0, NULL, NULL, 3, 5),
(4, 'Passar uma semana sem abrir redes sociais de manhã', 2, 80, 35, 0, 0, NULL, NULL, 3, 5),
(4, 'Fazer 10 sessões Pomodoro no projeto', 2, 100, 40, 0, 0, NULL, NULL, 3, 5),

-- Secundárias do Arco do Herói Disciplinado (MP-4)
(4, 'Acordar no horário por 7 dias seguidos', 2, 80, 35, 0, 0, NULL, NULL, 4, 4),
(4, 'Manter o quarto organizado por 7 dias', 2, 60, 25, 0, 0, NULL, NULL, 4, 4),
(4, 'Planejar o dia seguinte por 7 dias seguidos', 2, 70, 30, 0, 0, NULL, NULL, 4, 4),
(4, 'Dormir antes da meia-noite por 7 dias seguidos', 2, 80, 35, 0, 0, NULL, NULL, 4, 6);

-- ===========================
-- DESAFIOS
-- (sem vínculo — missões especiais de alto risco/recompensa)
-- ===========================
INSERT INTO Missoes (PerfilId, Titulo, TipoId, RecompensaXp, RecompensaMoedas, Streak, Concluida, ConcluidaEm, DataLimite, MissaoPrincipalId, AtributoId) VALUES
(4, 'Semana Sem Redes Sociais', 3, 200, 100, 0, 0, NULL, NULL, NULL, 5),
(4, '30 Dias Bebendo 2L de Água', 3, 250, 120, 0, 0, NULL, NULL, NULL, 6),
(4, 'Ler um livro inteiro', 3, 200, 90, 0, 0, NULL, NULL, NULL, 2),
(4, 'Acordar cedo por 14 dias seguidos', 3, 220, 100, 0, 0, NULL, NULL, NULL, 4),
(4, '30 dias de treino consecutivos', 3, 300, 150, 0, 0, NULL, NULL, NULL, 3);

------------------------------------------------------------------------------------------------
INSERT INTO Recompensas (PerfilId, Nome, Descricao, Emoji, Preco, Ativa, AtributoId, PontosNecessarios) VALUES

-- 🍕 Comida / Prazer
(4, N'Pedir comida',        N'Pedir aquela comida favorita sem culpa',          N'🍕', 80,  1, NULL, 0),
(4, N'Sobremesa',           N'Comer uma sobremesa especial',                    N'🍰', 30,  1, NULL, 0),
(4, N'Café especial',       N'Tomar um café gourmet ou na cafeteria favorita',  N'☕', 40,  1, NULL, 0),
(4, N'Jantar fora',         N'Sair para jantar em um restaurante bacana',       N'🍽️', 150, 1, NULL, 0),

-- 🎮 Entretenimento
(4, N'Sessão de jogo',      N'2 horas de jogo sem culpa',                      N'🎮', 50,  1, NULL, 0),
(4, N'Maratona de série',   N'Uma tarde inteira de série ou anime',             N'📺', 60,  1, NULL, 0),
(4, N'Cinema',              N'Ir ao cinema assistir o filme que quiser',        N'🎬', 120, 1, NULL, 0),
(4, N'Comprar um jogo',     N'Comprar aquele jogo que está na wishlist',        N'🕹️', 300, 1, NULL, 0),

-- 🛍️ Compras / Presentes para si
(4, N'Comprar um livro',    N'Comprar aquele livro que está na lista',          N'📚', 100, 1, NULL, 0),
(4, N'Comprar uma roupa',   N'Aquela peça que você está de olho',               N'👕', 200, 1, NULL, 0),
(4, N'Acessório desejado',  N'Comprar um acessório ou item pessoal',            N'🛒', 250, 1, NULL, 0),

-- 😴 Descanso / Autocuidado
(4, N'Dia de descanso',     N'Um dia inteiro sem obrigações, só relaxando',    N'😴', 90,  1, NULL, 0),
(4, N'Soneca liberada',     N'Tirar uma soneca sem culpa no meio do dia',       N'🛌', 35,  1, NULL, 0),
(4, N'Banho relaxante',     N'Banho longo com música e sem pressa',             N'🛁', 25,  1, NULL, 0),
(4, N'Cuidado com a pele',  N'Skincare completo ou máscara facial',             N'✨', 45,  1, NULL, 0),

-- 🎯 Experiências
(4, N'Passeio ao ar livre', N'Ir a um parque, praça ou lugar especial',        N'🌳', 70,  1, NULL, 0),
(4, N'Evento / Show',       N'Ir a um show, evento ou exposição',              N'🎵', 200, 1, NULL, 0);