export interface Classe {
  id: number;
  nome: string;
  nomeFeminino?: string | null;
  emoji: string;
  atributoId: number;
  atributo?: Atributo;
}

export interface Perfil {
  id: number;
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
  desafioConcluídoEm?: string | null;
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
}
