import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Atributo, BomHabito, Classe, DiarioAcao, Experimento, ItemInventario, JornadaSemana, MauHabito, Missao, Perfil, Recompensa } from '../models/models';
import { environment } from '../../environments/environment';

const BASE = environment.apiUrl;

@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);

  getPerfis() {
    return this.http.get<Perfil[]>(`${BASE}/perfil`);
  }

  getMeusPerfis() {
    return this.http.get<Perfil[]>(`${BASE}/perfil/meus`);
  }

  vincularPerfil(id: number) {
    return this.http.post<Perfil>(`${BASE}/perfil/${id}/vincular`, {});
  }

  getOrfaos() {
    return this.http.get<Perfil[]>(`${BASE}/perfil/orfaos`);
  }

  getPerfil(id: number) {
    return this.http.get<Perfil>(`${BASE}/perfil/${id}`);
  }

  getPerfilMe() {
    return this.http.get<Perfil>(`${BASE}/perfil/me`);
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

  criarBomHabito(habito: Partial<BomHabito>) {
    return this.http.post<BomHabito>(`${BASE}/habitos/bons`, habito);
  }

  editarBomHabito(id: number, habito: Partial<BomHabito>) {
    return this.http.put<BomHabito>(`${BASE}/habitos/bons/${id}`, habito);
  }

  excluirBomHabito(id: number) {
    return this.http.delete(`${BASE}/habitos/bons/${id}`);
  }

  criarMauHabito(habito: Partial<MauHabito>) {
    return this.http.post<MauHabito>(`${BASE}/habitos/maus`, habito);
  }

  editarMauHabito(id: number, habito: Partial<MauHabito>) {
    return this.http.put<MauHabito>(`${BASE}/habitos/maus/${id}`, habito);
  }

  excluirMauHabito(id: number) {
    return this.http.delete(`${BASE}/habitos/maus/${id}`);
  }

  criarMissao(missao: { titulo: string; tipoId: number; recompensaXp: number; recompensaMoedas: number; perfilId?: number }) {
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

  criarRecompensa(r: Partial<Recompensa>) {
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
}
