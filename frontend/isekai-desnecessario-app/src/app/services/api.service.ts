import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Atributo, BomHabito, Classe, DiarioAcao, Experimento, GrupoDetalhe, GrupoHabito, GrupoMeuSaldo, GrupoMissao, GrupoRecompensa, GrupoResumo, GrupoConvitePendente, HabitoCatalogo, ItemInventario, JornadaSemana, MauHabito, Missao, MissaoCatalogo, Notificacao, Pendente, Perfil, PerfilRanking, PerfilRankingAtributo, PlanoGrupo, Recompensa, RecompensaCatalogo } from '../models/models';

// Campos extras de catálogo no payload de criação (Partes 3-5).
type ExtrasCatalogo = { classeIds?: number[]; proprio?: boolean; travaDias?: number };
import { environment } from '../../environments/environment';

const BASE = environment.apiUrl;

@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);

  getRanking(top = 50) {
    return this.http.get<PerfilRanking[]>(`${BASE}/perfil/ranking?top=${top}`);
  }

  getRankingAtributos(atributoId?: number, top = 50) {
    const q = atributoId != null ? `&atributoId=${atributoId}` : '';
    return this.http.get<PerfilRankingAtributo[]>(`${BASE}/perfil/ranking/atributos?top=${top}${q}`);
  }

  definirPerfilPrincipal(id: number) {
    return this.http.post<Perfil>(`${BASE}/perfil/${id}/definir-principal`, {});
  }

  getMeusPerfis() {
    return this.http.get<Perfil[]>(`${BASE}/perfil/meus`);
  }

  vincularPerfil(id: number) {
    return this.http.post<Perfil>(`${BASE}/perfil/${id}/vincular`, {});
  }

  desvincularPerfil(id: number) {
    return this.http.post(`${BASE}/perfil/${id}/desvincular`, {});
  }

  getOrfaos() { return this.http.get<Perfil[]>(`${BASE}/perfil/orfaos`); }

  getPerfil(id: number) {
    return this.http.get<Perfil>(`${BASE}/perfil/${id}`);
  }

  criarPerfil(nome: string, classeId?: number | null, genero?: string | null) {
    return this.http.post<Perfil>(`${BASE}/perfil`, { nome, xp: 0, moedas: 0, rank: 'H', titulo: 'Iniciante', nivel: 1, proximoNivelXp: 100, classeId, genero });
  }

  getClasses() {
    return this.http.get<Classe[]>(`${BASE}/classes`);
  }

  atualizarPerfilInfo(id: number, nome: string, classeId: number | null, genero: string | null) {
    return this.http.patch<Perfil>(`${BASE}/perfil/${id}/info`, { nome, classeId, genero });
  }

  excluirPerfil(id: number) {
    return this.http.delete(`${BASE}/perfil/${id}`);
  }

  getBonsHabitos(perfilId: number) {
    return this.http.get<BomHabito[]>(`${BASE}/habitos/bons?perfilId=${perfilId}`);
  }

  getMausHabitos(perfilId: number) {
    return this.http.get<MauHabito[]>(`${BASE}/habitos/maus?perfilId=${perfilId}`);
  }

  completarBomHabito(id: number, perfilId: number) {
    return this.http.post<BomHabito>(`${BASE}/habitos/bons/${id}/completar?perfilId=${perfilId}`, {});
  }

  registrarMauHabito(id: number, perfilId: number) {
    return this.http.post(`${BASE}/habitos/maus/${id}/registrar?perfilId=${perfilId}`, {});
  }

  uploadFoto(perfilId: number, arquivo: File) {
    const form = new FormData();
    form.append('arquivo', arquivo);
    return this.http.post<Perfil>(`${BASE}/perfil/${perfilId}/foto`, form);
  }

  getMissoes(perfilId: number) {
    return this.http.get<Missao[]>(`${BASE}/missoes?perfilId=${perfilId}`);
  }

  completarMissao(id: number, perfilId: number) {
    return this.http.post(`${BASE}/missoes/${id}/completar?perfilId=${perfilId}`, {});
  }

  criarBomHabito(habito: Partial<BomHabito> & ExtrasCatalogo) {
    return this.http.post<BomHabito>(`${BASE}/habitos/bons`, habito);
  }

  editarBomHabito(id: number, habito: Partial<BomHabito>) {
    return this.http.put<BomHabito>(`${BASE}/habitos/bons/${id}`, habito);
  }

  excluirBomHabito(id: number) {
    return this.http.delete(`${BASE}/habitos/bons/${id}`);
  }

  criarMauHabito(habito: Partial<MauHabito> & ExtrasCatalogo) {
    return this.http.post<MauHabito>(`${BASE}/habitos/maus`, habito);
  }

  editarMauHabito(id: number, habito: Partial<MauHabito>) {
    return this.http.put<MauHabito>(`${BASE}/habitos/maus/${id}`, habito);
  }

  excluirMauHabito(id: number) {
    return this.http.delete(`${BASE}/habitos/maus/${id}`);
  }

  criarMissao(missao: { titulo: string; tipoId: number; recompensaXp: number; recompensaMoedas: number; perfilId?: number } & ExtrasCatalogo) {
    return this.http.post<Missao>(`${BASE}/missoes`, missao);
  }

  editarMissao(id: number, missao: { titulo: string; tipoId: number; recompensaXp: number; recompensaMoedas: number; concluida?: boolean }) {
    return this.http.put<Missao>(`${BASE}/missoes/${id}`, missao);
  }

  excluirMissao(id: number) {
    return this.http.delete(`${BASE}/missoes/${id}`);
  }

  getTiposMissao() {
    return this.http.get<{ id: number; nome: string }[]>(`${BASE}/missoes/tipos`);
  }

  resetarPerfil(id: number) {
    return this.http.post<Perfil>(`${BASE}/perfil/${id}/reset`, {});
  }

  // ── Recompensas ──────────────────────────────────────
  getRecompensas(perfilId: number) {
    return this.http.get<Recompensa[]>(`${BASE}/recompensas?perfilId=${perfilId}`);
  }

  criarRecompensa(r: Partial<Recompensa> & ExtrasCatalogo) {
    return this.http.post<Recompensa>(`${BASE}/recompensas`, r);
  }

  editarRecompensa(id: number, r: Partial<Recompensa>) {
    return this.http.put<Recompensa>(`${BASE}/recompensas/${id}`, r);
  }

  excluirRecompensa(id: number) {
    return this.http.delete(`${BASE}/recompensas/${id}`);
  }

  resgatarRecompensa(id: number, perfilId: number) {
    return this.http.post<Perfil>(`${BASE}/recompensas/${id}/resgatar?perfilId=${perfilId}`, {});
  }

  // ── Inventário ───────────────────────────────────────
  getInventario(perfilId: number) {
    return this.http.get<ItemInventario[]>(`${BASE}/inventario?perfilId=${perfilId}`);
  }

  usarItem(id: number, perfilId: number) {
    return this.http.post<ItemInventario>(`${BASE}/inventario/${id}/usar?perfilId=${perfilId}`, {});
  }

  // ── Histórico XP ─────────────────────────────────────
  getHistorico(perfilId: number) {
    return this.http.get<any[]>(`${BASE}/perfil/${perfilId}/historico`);
  }

  upsertHistorico(perfilId: number, xpHoje: number, nivel: number, moedas: number, hora: number) {
    return this.http.post<any>(`${BASE}/perfil/${perfilId}/historico/upsert`, { xpHoje, nivel, moedas, hora });
  }

  // ── Atributos ────────────────────────────────────────
  getAtributos() {
    return this.http.get<Atributo[]>(`${BASE}/atributos`);
  }

  // ── Lootbox ──────────────────────────────────────────
  getLootboxStatus(perfilId: number) {
    return this.http.get<{ disponivel: boolean; xpHoje: number; xpNecessario: number; jaAbriuHoje: boolean }>
      (`${BASE}/perfil/${perfilId}/lootbox/status`);
  }

  // ── Desafio do dia ───────────────────────────────────
  recusarDesafio(perfilId: number) {
    return this.http.post<Perfil>(`${BASE}/perfil/${perfilId}/desafio/recusar`, {});
  }

  concluirDesafio(perfilId: number) {
    return this.http.post<Perfil>(`${BASE}/perfil/${perfilId}/desafio/concluir`, {});
  }

  // ── Diário de Conquistas ─────────────────────────────
  getDiario(perfilId: number) {
    return this.http.get<DiarioAcao[]>(`${BASE}/diarioacoes?perfilId=${perfilId}`);
  }

  // ── Jornada de Missões ───────────────────────────────
  getJornada(perfilId: number) {
    return this.http.get<JornadaSemana[]>(`${BASE}/missoes/jornada?perfilId=${perfilId}`);
  }

  // ── Pontos de Atributo (persistidos) ─────────────────
  getPontosAtributos(perfilId: number) {
    return this.http.get<{ atributoId: number; total: number }[]>(`${BASE}/pontosatributos?perfilId=${perfilId}`);
  }

  // ── Snapshots de Atributos ───────────────────────────
  getSnapshotAnterior(perfilId: number) {
    return this.http.get<{ atributoId: number; pontos: number; data: string }[]>(`${BASE}/snapshots/anterior?perfilId=${perfilId}`);
  }

  salvarSnapshot(perfilId: number, dados: { atributoId: number; pontos: number }[]) {
    return this.http.post<any>(`${BASE}/snapshots/salvar?perfilId=${perfilId}`, dados);
  }

  // ── Experimentos ─────────────────────────────────────
  getExperimentos(perfilId: number) {
    return this.http.get<Experimento[]>(`${BASE}/experimentos?perfilId=${perfilId}`);
  }

  criarExperimento(dto: { perfilId: number; titulo: string; descricao: string; duracaoDias: number }) {
    return this.http.post<Experimento>(`${BASE}/experimentos`, dto);
  }

  marcarDiaExperimento(id: number) {
    return this.http.post<Experimento>(`${BASE}/experimentos/${id}/dia`, {});
  }

  converterExperimento(id: number, perfilId: number) {
    return this.http.post<any>(`${BASE}/experimentos/${id}/converter?perfilId=${perfilId}`, {});
  }

  excluirExperimento(id: number) {
    return this.http.delete(`${BASE}/experimentos/${id}`);
  }

  abrirLootbox(perfilId: number) {
    return this.http.post<{ recompensa: Recompensa; chance: number }>
      (`${BASE}/perfil/${perfilId}/lootbox/abrir`, {});
  }

  // ── Catálogo (itens disponíveis para ativar) ─────────
  getCatalogoBonsHabitos(perfilId: number) {
    return this.http.get<HabitoCatalogo[]>(`${BASE}/habitos/bons/catalogo?perfilId=${perfilId}`);
  }
  getCatalogoMausHabitos(perfilId: number) {
    return this.http.get<HabitoCatalogo[]>(`${BASE}/habitos/maus/catalogo?perfilId=${perfilId}`);
  }
  getCatalogoMissoes(perfilId: number) {
    return this.http.get<MissaoCatalogo[]>(`${BASE}/missoes/catalogo?perfilId=${perfilId}`);
  }
  getCatalogoRecompensas(perfilId: number) {
    return this.http.get<RecompensaCatalogo[]>(`${BASE}/recompensas/catalogo?perfilId=${perfilId}`);
  }

  // ── Ativar / desativar no perfil ─────────────────────
  ativarBomHabito(id: number, perfilId: number, ativar: boolean) {
    return this.http.post(`${BASE}/habitos/bons/${id}/${ativar ? 'ativar' : 'desativar'}?perfilId=${perfilId}`, {});
  }
  ativarMauHabito(id: number, perfilId: number, ativar: boolean) {
    return this.http.post(`${BASE}/habitos/maus/${id}/${ativar ? 'ativar' : 'desativar'}?perfilId=${perfilId}`, {});
  }
  ativarMissao(id: number, perfilId: number, ativar: boolean) {
    return this.http.post(`${BASE}/missoes/${id}/${ativar ? 'ativar' : 'desativar'}?perfilId=${perfilId}`, {});
  }
  ativarRecompensa(id: number, perfilId: number, ativar: boolean) {
    return this.http.post(`${BASE}/recompensas/${id}/${ativar ? 'ativar' : 'desativar'}?perfilId=${perfilId}`, {});
  }

  // ── Aprovações (Admin) ───────────────────────────────
  getPendentes() {
    return this.http.get<Pendente[]>(`${BASE}/aprovacoes/pendentes`);
  }
  aprovarPendente(tipo: string, id: number) {
    return this.http.post(`${BASE}/aprovacoes/${tipo}/${id}/aprovar`, {});
  }
  rejeitarPendente(tipo: string, id: number) {
    return this.http.post(`${BASE}/aprovacoes/${tipo}/${id}/rejeitar`, {});
  }

  // ── Grupos ───────────────────────────────────────────
  getGrupos(perfilId: number) {
    return this.http.get<GrupoResumo[]>(`${BASE}/grupos?perfilId=${perfilId}`);
  }
  criarGrupo(nome: string, plano: PlanoGrupo, perfilId: number) {
    return this.http.post<GrupoResumo>(`${BASE}/grupos`, { nome, plano, perfilId });
  }
  getGrupoDetalhe(id: number, perfilId: number) {
    return this.http.get<GrupoDetalhe>(`${BASE}/grupos/${id}?perfilId=${perfilId}`);
  }
  renomearGrupo(id: number, nome: string) {
    return this.http.put(`${BASE}/grupos/${id}`, { nome });
  }
  upgradeGrupo(id: number, plano: PlanoGrupo) {
    return this.http.post(`${BASE}/grupos/${id}/upgrade`, { plano });
  }
  cancelarGrupo(id: number) {
    return this.http.post(`${BASE}/grupos/${id}/cancelar`, {});
  }
  reativarGrupo(id: number) {
    return this.http.post(`${BASE}/grupos/${id}/reativar`, {});
  }
  excluirGrupo(id: number) {
    return this.http.delete(`${BASE}/grupos/${id}`);
  }
  sairDoGrupo(id: number, perfilId: number) {
    return this.http.post(`${BASE}/grupos/${id}/sair?perfilId=${perfilId}`, {});
  }
  removerMembroGrupo(grupoId: number, membroId: number) {
    return this.http.delete(`${BASE}/grupos/${grupoId}/membros/${membroId}`);
  }

  // Convites de grupo
  convidarParaGrupo(grupoId: number, email: string) {
    return this.http.post(`${BASE}/grupos/${grupoId}/convites`, { email });
  }
  getConvitesGrupo() {
    return this.http.get<GrupoConvitePendente[]>(`${BASE}/grupos/convites`);
  }
  aceitarConviteGrupo(conviteId: number, perfilId: number) {
    return this.http.post(`${BASE}/grupos/convites/${conviteId}/aceitar?perfilId=${perfilId}`, {});
  }
  recusarConviteGrupo(conviteId: number) {
    return this.http.post(`${BASE}/grupos/convites/${conviteId}/recusar`, {});
  }

  // Conteúdo do grupo (só o Organizador cria/edita/exclui)
  criarHabitoGrupo(grupoId: number, h: { habito: string; xp: number; frequencia: string }) {
    return this.http.post<GrupoHabito>(`${BASE}/grupos/${grupoId}/habitos`, h);
  }
  editarHabitoGrupo(id: number, h: { habito: string; xp: number; frequencia: string }) {
    return this.http.put<GrupoHabito>(`${BASE}/grupos/habitos/${id}`, h);
  }
  excluirHabitoGrupo(id: number) {
    return this.http.delete(`${BASE}/grupos/habitos/${id}`);
  }
  completarHabitoGrupo(id: number, perfilId: number) {
    return this.http.post<GrupoMeuSaldo>(`${BASE}/grupos/habitos/${id}/completar?perfilId=${perfilId}`, {});
  }
  criarMissaoGrupo(grupoId: number, m: { titulo: string; recompensaXp: number; recompensaMoedas: number; dataLimite?: string | null }) {
    return this.http.post<GrupoMissao>(`${BASE}/grupos/${grupoId}/missoes`, m);
  }
  editarMissaoGrupo(id: number, m: { titulo: string; recompensaXp: number; recompensaMoedas: number; dataLimite?: string | null }) {
    return this.http.put<GrupoMissao>(`${BASE}/grupos/missoes/${id}`, m);
  }
  excluirMissaoGrupo(id: number) {
    return this.http.delete(`${BASE}/grupos/missoes/${id}`);
  }
  completarMissaoGrupo(id: number, perfilId: number) {
    return this.http.post<GrupoMeuSaldo>(`${BASE}/grupos/missoes/${id}/completar?perfilId=${perfilId}`, {});
  }
  criarRecompensaGrupo(grupoId: number, r: { nome: string; custo: number }) {
    return this.http.post<GrupoRecompensa>(`${BASE}/grupos/${grupoId}/recompensas`, r);
  }
  editarRecompensaGrupo(id: number, r: { nome: string; custo: number }) {
    return this.http.put<GrupoRecompensa>(`${BASE}/grupos/recompensas/${id}`, r);
  }
  excluirRecompensaGrupo(id: number) {
    return this.http.delete(`${BASE}/grupos/recompensas/${id}`);
  }
  resgatarRecompensaGrupo(id: number, perfilId: number) {
    return this.http.post<GrupoMeuSaldo>(`${BASE}/grupos/recompensas/${id}/resgatar?perfilId=${perfilId}`, {});
  }

  // ── Notificações (sininho) ───────────────────────────
  getNotificacoes() {
    return this.http.get<Notificacao[]>(`${BASE}/notificacoes`);
  }

  marcarNotificacaoLida(id: number) {
    return this.http.post(`${BASE}/notificacoes/${id}/lida`, {});
  }

  marcarTodasNotificacoesLidas() {
    return this.http.post(`${BASE}/notificacoes/lidas`, {});
  }
}
