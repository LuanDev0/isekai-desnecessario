export interface Classe {
  id: number;
  nome: string;
  nomeFeminino?: string | null;
  emoji: string;
  atributoId: number;
  atributo?: Atributo;
}

// Papel de acesso da conta (não confundir com Classe RPG do perfil).
export type Role = 'Usuario' | 'VIP' | 'Moderador' | 'Admin';

export interface Usuario {
  id: number;
  googleId: string;
  email: string;
  nome: string;
  fotoUrl?: string | null;
  role: Role;
}

export interface Perfil {
  id: number;
  usuarioId?: number | null;
  nome: string;
  genero?: string | null;
  classeId?: number | null;
  xp: number;
  moedas: number;
  rank: string;
  titulo: string;
  nivel: number;
  proximoNivelXp: number;
  fotoUrl?: string | null;
  xpHoje?: number;
  dataXpHoje?: string | null;
  ultimaLootbox?: string | null;
  desafioRecusadoEm?: string | null;
  desafioConcluidoEm?: string | null;
  principal?: boolean;
  principalDesde?: string | null;
}

export interface Recompensa {
  id: number;
  perfilId: number;
  nome: string;
  descricao: string;
  emoji: string;
  preco: number;
  ativa: boolean;
  atributoId?: number | null;
  pontosNecessarios: number;
  escopo?: EscopoConteudo;
}

export interface TipoMissao {
  id: number;
  nome: string;
}

export interface Missao {
  id: number;
  perfilId: number;
  titulo: string;
  tipoId: number;
  tipo: TipoMissao;
  recompensaXp: number;
  recompensaMoedas: number;
  streak: number;
  concluida: boolean;
  concluidaEm?: string | null;
  dataLimite?: string | null;
  missaoPrincipalId?: number | null;
  atributoId?: number | null;
  escopo?: EscopoConteudo;
}

export interface JornadaSemana {
  semana: string;
  total: number;
  xp: number;
  principais: number;
}

export interface ItemInventario {
  id: number;
  perfilId: number;
  recompensaId: number;
  nome: string;
  emoji: string;
  descricao: string;
  preco: number;
  dataCompra: string;
  dataUso?: string | null;
  usado: boolean;
}

export interface Atributo {
  id: number;
  nome: string;
  emoji: string;
  icone: string;
  descricao: string;
  cor: string;
}

export interface BomHabito {
  id: number;
  perfilId: number;
  habito: string;
  xp: number;
  frequencia: string;
  streak: number;
  ultimaExecucao?: string | null;
  atributoId?: number | null;
  escopo?: EscopoConteudo;
}

export interface DiarioAcao {
  id: number;
  perfilId: number;
  mensagem: string;
  emoji: string;
  tipo: string;
  data: string;
}

export interface Experimento {
  id: number;
  perfilId: number;
  titulo: string;
  descricao: string;
  duracaoDias: number;
  dataInicio: string;
  ativo: boolean;
  convertido: boolean;
  dias: ExperimentoDia[];
}

export interface ExperimentoDia {
  id: number;
  experimentoId: number;
  data: string;
}

export interface MauHabito {
  id: number;
  perfilId: number;
  habito: string;
  xp: number;
  frequencia: string;
  streak: number;
  ultimaExecucao?: string | null;
  atributoId?: number | null;
  escopo?: EscopoConteudo;
}

// ── Catálogo (definição + estado no perfil) ──────────
export type EscopoConteudo = 'Global' | 'Proprio';
export type StatusConteudo = 'Aprovado' | 'Pendente' | 'Rejeitado';

export interface HabitoCatalogo {
  id: number;
  habito: string;
  xp: number;
  frequencia: string;
  atributoId?: number | null;
  escopo: EscopoConteudo;
  status: StatusConteudo;
  ativo: boolean;
  classeIds: number[];
  bloqueado: boolean;
}

export interface MissaoCatalogo {
  id: number;
  titulo: string;
  tipoId: number;
  tipo?: TipoMissao;
  recompensaXp: number;
  recompensaMoedas: number;
  dataLimite?: string | null;
  missaoPrincipalId?: number | null;
  atributoId?: number | null;
  escopo: EscopoConteudo;
  status: StatusConteudo;
  ativo: boolean;
  classeIds: number[];
  bloqueado: boolean;
}

export interface RecompensaCatalogo {
  id: number;
  nome: string;
  descricao: string;
  emoji: string;
  preco: number;
  ativa: boolean;
  atributoId?: number | null;
  pontosNecessarios: number;
  escopo: EscopoConteudo;
  status: StatusConteudo;
  ativo: boolean;
  classeIds: number[];
  bloqueado: boolean;
}

// Item pendente de aprovação (unificado entre tipos), para a tela do admin.
export interface Pendente {
  tipo: 'bomhabito' | 'mauhabito' | 'missao' | 'recompensa';
  id: number;
  titulo: string;
  criadoPorUsuarioId?: number | null;
  autorNome?: string | null;
}

export interface PerfilRanking {
  id:       number;
  nome:     string;
  nivel:    number;
  xp:       number;
  rank:     string;
  titulo:   string;
  fotoUrl:  string | null;
  classeId: number | null;
}

export interface PerfilRankingAtributo {
  id:          number;
  nome:        string;
  nivel:       number;
  rank:        string;
  titulo:      string;
  fotoUrl:     string | null;
  classeId:    number | null;
  totalPontos: number;
}

// Notificação in-app (sininho). Tipo: 'pendente' | 'rejeitado' | 'modificado' | 'convite' | 'grupo'.
export interface Notificacao {
  id: number;
  usuarioId: number;
  tipo: string;
  mensagem: string;
  lida: boolean;
  criadaEm: string;
}

// ── Grupos (assinatura separada do VIP; economia própria) ──
export type PlanoGrupo = 'Starter' | 'Standard' | 'Pro' | 'Max';

export interface GrupoResumo {
  id: number;
  nome: string;
  plano: PlanoGrupo;
  maxMembros: number;
  ativo: boolean;
  organizador: boolean;
  totalMembros: number;
  membro: boolean;
  xpGrupo?: number | null;
  moedasGrupo?: number | null;
}

export interface GrupoMeuSaldo {
  xpGrupo: number;
  moedasGrupo: number;
}

export interface GrupoMembroInfo {
  id: number;
  perfilId: number;
  nome: string;
  fotoUrl?: string | null;
  xpGrupo: number;
  moedasGrupo: number;
  organizador: boolean;
  entrouEm: string;
}

export interface GrupoHabito {
  id: number;
  habito: string;
  xp: number;
  frequencia: string;
  ultimaExecucao?: string | null;
  disponivel: boolean;
}

export interface GrupoMissao {
  id: number;
  titulo: string;
  recompensaXp: number;
  recompensaMoedas: number;
  dataLimite?: string | null;
  concluida: boolean;
  concluidaEm?: string | null;
  totalConclusoes: number;
}

export interface GrupoRecompensa {
  id: number;
  nome: string;
  custo: number;
  meusResgates: number;
}

export interface GrupoFeedEvento {
  tipo: string;
  mensagem: string;
  criadoEm: string;
}

export interface GrupoConviteEnviado {
  id: number;
  email: string;
  criadoEm: string;
}

export interface GrupoConvitePendente {
  id: number;
  grupoId: number;
  grupoNome: string;
  plano: PlanoGrupo;
  totalMembros: number;
  maxMembros: number;
  criadoEm: string;
}

export interface GrupoDetalhe {
  id: number;
  nome: string;
  plano: PlanoGrupo;
  maxMembros: number;
  ativo: boolean;
  organizador: boolean;
  criadoEm: string;
  membro?: GrupoMeuSaldo | null;
  membros: GrupoMembroInfo[];
  habitos: GrupoHabito[];
  missoes: GrupoMissao[];
  recompensas: GrupoRecompensa[];
  feed: GrupoFeedEvento[];
  convites?: GrupoConviteEnviado[] | null;
}
