import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { ProfileService } from '../../services/profile.service';
import { LanguageService } from '../../services/language.service';
import { AuthService } from '../../services/auth.service';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { Atributo, BomHabito, Classe, HabitoCatalogo, MauHabito, Missao, MissaoCatalogo, Pendente, Perfil, Recompensa, RecompensaCatalogo } from '../../models/models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-configuracoes',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './configuracoes.component.html',
  styleUrl: './configuracoes.component.scss',
})
export class ConfiguracoesComponent implements OnInit {
  private api     = inject(ApiService);
  private profile = inject(ProfileService);
  private router  = inject(Router);
  readonly lang   = inject(LanguageService);
  readonly auth   = inject(AuthService);

  get perfilId() { return this.profile.id; }

  // Listas — ativos no perfil
  bonsHabitos:  BomHabito[]  = [];
  mausHabitos:  MauHabito[]  = [];
  missoes:      Missao[]     = [];
  recompensas:  Recompensa[] = [];
  tiposMissao:  { id: number; nome: string }[] = [];
  atributos:    Atributo[]   = [];
  classes:      Classe[]     = [];

  // Catálogo (itens disponíveis para ativar)
  catBons:   HabitoCatalogo[]     = [];
  catMaus:   HabitoCatalogo[]     = [];
  catMiss:   MissaoCatalogo[]     = [];
  catRecomp: RecompensaCatalogo[] = [];

  // Colapsáveis do catálogo (sub-aba "Adicionar")
  catAbertoBons   = false;
  catAbertoMaus   = false;
  catAbertoMiss   = false;
  catAbertoRecomp = false;

  // Aprovações (admin)
  secaoAprov = false;
  pendentes: Pendente[] = [];

  // ── Aba Perfil ────────────────────────────────────
  secaoPerfil    = false;
  editNome       = '';
  editClasseId:  number | null = null;
  editGenero:    string | null = null;
  fotoFileConfig:    File | null   = null;
  fotoPreviewConfig: string | null = null;
  salvandoPerfil = false;
  feedbackPerfil     = '';
  feedbackPerfilTipo: 'good' | 'warn' = 'good';
  readonly generos = ['Masculino', 'Feminino', 'Não-binário'];

  nomeClasse(c: Classe): string {
    if (this.editGenero === 'Feminino') return c.nomeFeminino ?? c.nome;
    return c.nome;
  }

  editGeneroChanged(val: string | null) {
    this.editGenero = val;
    this.classes = [...this.classes];
  }

  // Formulários de criação (proprio/classeIds/travaDias = campos de catálogo)
  novoBom  = { habito: '', xp: 50, frequencia: 'Livre', atributoId: null as number | null, proprio: false, classeIds: [] as number[], travaDias: 0 };
  novoMau  = { habito: '', xp: 50, frequencia: 'Livre', atributoId: null as number | null, proprio: false, classeIds: [] as number[], travaDias: 0 };
  novaMiss = { titulo: '', tipoId: 1, recompensaXp: 10, recompensaMoedas: 30, atributoId: null as number | null, dataLimite: null as string | null, missaoPrincipalId: null as number | null, proprio: false, classeIds: [] as number[], travaDias: 0 };
  novaRecomp = { nome: '', descricao: '', emoji: '🎁', preco: 50, atributoId: null as number | null, pontosNecessarios: 0, proprio: false, classeIds: [] as number[], travaDias: 0 };

  // Edição inline — armazena o item sendo editado por id
  editandoBomId:    number | null = null;
  editandoMauId:    number | null = null;
  editandoMissId:   number | null = null;
  editandoRecompId: number | null = null;
  editBom:  Partial<BomHabito> = {};
  editMau:  Partial<MauHabito> = {};
  editMiss: { titulo: string; tipoId: number; recompensaXp: number; recompensaMoedas: number; concluida: boolean; atributoId: number | null; dataLimite: string | null; missaoPrincipalId: number | null } =
    { titulo: '', tipoId: 1, recompensaXp: 10, recompensaMoedas: 30, concluida: false, atributoId: null, dataLimite: null, missaoPrincipalId: null };
  editRecomp: { nome: string; descricao: string; emoji: string; preco: number; ativa: boolean; atributoId: number | null; pontosNecessarios: number } =
    { nome: '', descricao: '', emoji: '🎁', preco: 50, ativa: true, atributoId: null, pontosNecessarios: 0 };

  // Feedback
  feedbackBom    = '';
  feedbackBomTipo: 'good' | 'warn' = 'good';
  feedbackMau    = '';
  feedbackMauTipo: 'good' | 'warn' = 'good';
  feedbackMiss     = '';
  feedbackMissTipo: 'good' | 'warn' = 'good';
  feedbackRecomp     = '';
  feedbackRecompTipo: 'good' | 'warn' = 'good';

  // Colapsáveis — listas internas
  abertoBons   = false;
  abertoMaus   = false;
  abertoMiss   = false;
  abertoRecomp = false;

  // Colapsáveis — seções inteiras (todas fechadas por padrão)
  secaoBons       = false;
  secaoMaus       = false;
  secaoMiss       = false;
  secaoRecomp     = false;
  secaoPrincipal  = false;
  secaoAssinatura = false;
  secaoDanger     = false;

  // ── Perfil principal ──────────────────────────────
  meusPerfis:       Perfil[] = [];
  definindoPrincipal = false;

  get podeTrocarPrincipal(): boolean {
    return new Date().getDate() === 1;
  }

  carregarMeusPerfis() {
    this.api.getMeusPerfis().subscribe({ next: p => this.meusPerfis = p });
  }

  definirPrincipal(id: number) {
    if (!this.podeTrocarPrincipal || this.definindoPrincipal) return;
    this.definindoPrincipal = true;
    this.api.definirPerfilPrincipal(id).subscribe({
      next: () => { this.carregarMeusPerfis(); this.definindoPrincipal = false; },
      error: e  => { this.mostrarFeedback('perfil', this.erroMsg(e), 'warn'); this.definindoPrincipal = false; },
    });
  }

  // Confirmação reset
  confirmarReset = false;

  // Confirmação apagar perfil
  confirmarApagar = false;
  digitouNome     = '';

  ngOnInit() {
    const savedId = this.profile.getSavedId();
    if (!savedId) { this.router.navigate(['/cadastro']); return; }
    const init = () => {
      this.api.getTiposMissao().subscribe({ next: t => { this.tiposMissao = t; this.novaMiss.tipoId = t[0]?.id ?? 1; } });
      this.api.getAtributos().subscribe({ next: a => this.atributos = a });
      this.api.getClasses().subscribe({ next: c => this.classes = c });
      this.carregarPerfil();
      this.carregar();
      this.carregarMeusPerfis();
    };
    if (!this.profile.perfilAtivo()) {
      this.api.getPerfil(savedId).subscribe({ next: p => { this.profile.setPerfilAtivo(p); init(); } });
    } else {
      init();
    }
  }

  carregar() {
    this.api.getBonsHabitos(this.perfilId).subscribe({ next: h => this.bonsHabitos = h });
    this.api.getMausHabitos(this.perfilId).subscribe({ next: h => this.mausHabitos = h });
    this.api.getMissoes(this.perfilId).subscribe({ next: m => this.missoes = m });
    this.api.getRecompensas(this.perfilId).subscribe({ next: r => this.recompensas = r });
    this.carregarCatalogos();
    if (this.auth.isAdmin()) this.carregarPendentes();
  }

  carregarCatalogos() {
    this.api.getCatalogoBonsHabitos(this.perfilId).subscribe({ next: c => this.catBons = c });
    this.api.getCatalogoMausHabitos(this.perfilId).subscribe({ next: c => this.catMaus = c });
    this.api.getCatalogoMissoes(this.perfilId).subscribe({ next: c => this.catMiss = c });
    this.api.getCatalogoRecompensas(this.perfilId).subscribe({ next: c => this.catRecomp = c });
  }

  // ── Ativar / desativar do catálogo ───────────────────
  toggleAtivoBom(item: HabitoCatalogo) {
    if (item.bloqueado) return;
    this.api.ativarBomHabito(item.id, this.perfilId, !item.ativo).subscribe({
      next: () => this.carregar(),
      error: e => this.mostrarFeedback('bom', this.erroMsg(e), 'warn'),
    });
  }
  toggleAtivoMau(item: HabitoCatalogo) {
    if (item.bloqueado) return;
    this.api.ativarMauHabito(item.id, this.perfilId, !item.ativo).subscribe({
      next: () => this.carregar(),
      error: e => this.mostrarFeedback('mau', this.erroMsg(e), 'warn'),
    });
  }
  toggleAtivoMiss(item: MissaoCatalogo) {
    if (item.bloqueado) return;
    this.api.ativarMissao(item.id, this.perfilId, !item.ativo).subscribe({
      next: () => this.carregar(),
      error: e => this.mostrarFeedback('miss', this.erroMsg(e), 'warn'),
    });
  }
  toggleAtivoRecomp(item: RecompensaCatalogo) {
    if (item.bloqueado) return;
    this.api.ativarRecompensa(item.id, this.perfilId, !item.ativo).subscribe({
      next: () => this.carregar(),
      error: e => this.mostrarFeedback('recomp', this.erroMsg(e), 'warn'),
    });
  }

  // Desativar a partir da lista de Ativos (respeita trava — backend devolve 400).
  desativarAtivo(tipo: 'bom' | 'mau' | 'miss' | 'recomp', id: number) {
    const obs =
      tipo === 'bom'  ? this.api.ativarBomHabito(id, this.perfilId, false) :
      tipo === 'mau'  ? this.api.ativarMauHabito(id, this.perfilId, false) :
      tipo === 'miss' ? this.api.ativarMissao(id, this.perfilId, false) :
                        this.api.ativarRecompensa(id, this.perfilId, false);
    obs.subscribe({ next: () => this.carregar(), error: e => this.mostrarFeedback(tipo, this.erroMsg(e), 'warn') });
  }

  // ── Seleção de classes (chips) no formulário de criação ──
  toggleClasse(arr: number[], id: number) {
    const i = arr.indexOf(id);
    if (i >= 0) arr.splice(i, 1); else arr.push(id);
  }

  // ── Aprovações (admin) ───────────────────────────────
  carregarPendentes() {
    this.api.getPendentes().subscribe({ next: p => this.pendentes = p, error: () => {} });
  }
  aprovar(p: Pendente) {
    this.api.aprovarPendente(p.tipo, p.id).subscribe({ next: () => { this.carregarPendentes(); this.carregarCatalogos(); } });
  }
  rejeitar(p: Pendente) {
    this.api.rejeitarPendente(p.tipo, p.id).subscribe({ next: () => this.carregarPendentes() });
  }

  private erroMsg(e: any): string {
    return typeof e?.error === 'string' ? e.error : 'Não foi possível concluir a ação.';
  }

  // Admin/Moderador escolhem global vs próprio (toggle); VIP só cria próprio.
  escopoProprio(toggle: boolean): boolean {
    return this.auth.podeCatalogoGlobal() ? toggle : true;
  }

  // Rótulo das classes de um item (para "exclusivo de ..." no catálogo).
  classesLabel(ids: number[]): string {
    return ids
      .map(id => { const c = this.classes.find(x => x.id === id); return c ? `${c.emoji} ${c.nome}` : ''; })
      .filter(Boolean)
      .join(', ');
  }

  // ── Bons hábitos ──────────────────────────────────
  salvarBomHabito() {
    if (!this.novoBom.habito.trim()) return;
    if (!this.novoBom.atributoId) { this.mostrarFeedback('bom', 'Selecione um atributo!', 'warn'); return; }
    const payload = { habito: this.novoBom.habito, xp: +this.novoBom.xp, frequencia: this.novoBom.frequencia, perfilId: this.perfilId, atributoId: this.novoBom.atributoId, proprio: this.escopoProprio(this.novoBom.proprio), classeIds: this.novoBom.classeIds, travaDias: +this.novoBom.travaDias };
    this.api.criarBomHabito(payload).subscribe({
      next: () => {
        this.novoBom = { habito: '', xp: 50, frequencia: 'Livre', atributoId: null, proprio: false, classeIds: [], travaDias: 0 };
        this.mostrarFeedback('bom', 'Hábito criado!');
        this.carregar();
      },
      error: e => { console.error('Erro ao criar bom hábito:', e); this.mostrarFeedback('bom', this.erroMsg(e), 'warn'); }
    });
  }

  iniciarEdicaoBom(h: BomHabito) {
    this.editandoBomId = h.id;
    this.editBom = { habito: h.habito, xp: h.xp, frequencia: h.frequencia, atributoId: h.atributoId };
  }

  salvarEdicaoBom() {
    if (!this.editandoBomId) return;
    if (!this.editBom.atributoId) { this.mostrarFeedback('bom', 'Selecione um atributo!', 'warn'); return; }
    this.api.editarBomHabito(this.editandoBomId, this.editBom).subscribe({ next: () => {
      this.editandoBomId = null;
      this.mostrarFeedback('bom', 'Hábito atualizado!');
      this.carregar();
    }});
  }

  excluirBom(id: number) {
    if (!confirm('Excluir este hábito?')) return;
    this.api.excluirBomHabito(id).subscribe({ next: () => this.carregar() });
  }

  // ── Maus hábitos ──────────────────────────────────
  salvarMauHabito() {
    if (!this.novoMau.habito.trim()) return;
    if (!this.novoMau.atributoId) { this.mostrarFeedback('mau', 'Selecione um atributo!', 'warn'); return; }
    const payload = { habito: this.novoMau.habito, xp: +this.novoMau.xp, frequencia: this.novoMau.frequencia, perfilId: this.perfilId, atributoId: this.novoMau.atributoId, proprio: this.escopoProprio(this.novoMau.proprio), classeIds: this.novoMau.classeIds, travaDias: +this.novoMau.travaDias };
    this.api.criarMauHabito(payload).subscribe({
      next: () => {
        this.novoMau = { habito: '', xp: 50, frequencia: 'Livre', atributoId: null, proprio: false, classeIds: [], travaDias: 0 };
        this.mostrarFeedback('mau', 'Hábito criado!');
        this.carregar();
      },
      error: e => { console.error('Erro ao criar mau hábito:', e); this.mostrarFeedback('mau', this.erroMsg(e), 'warn'); }
    });
  }

  iniciarEdicaoMau(h: MauHabito) {
    this.editandoMauId = h.id;
    this.editMau = { habito: h.habito, xp: h.xp, frequencia: h.frequencia, atributoId: h.atributoId };
  }

  salvarEdicaoMau() {
    if (!this.editandoMauId) return;
    if (!this.editMau.atributoId) { this.mostrarFeedback('mau', 'Selecione um atributo!', 'warn'); return; }
    this.api.editarMauHabito(this.editandoMauId, this.editMau).subscribe({ next: () => {
      this.editandoMauId = null;
      this.mostrarFeedback('mau', 'Hábito atualizado!');
      this.carregar();
    }});
  }

  excluirMau(id: number) {
    if (!confirm('Excluir este hábito?')) return;
    this.api.excluirMauHabito(id).subscribe({ next: () => this.carregar() });
  }

  // ── Missões ───────────────────────────────────────
  salvarMissao() {
    if (!this.novaMiss.titulo.trim()) return;
    if (!this.novaMiss.atributoId) { this.mostrarFeedback('miss', 'Selecione um atributo!', 'warn'); return; }
    const payload = { titulo: this.novaMiss.titulo, tipoId: +this.novaMiss.tipoId, recompensaXp: +this.novaMiss.recompensaXp, recompensaMoedas: +this.novaMiss.recompensaMoedas, perfilId: this.perfilId, atributoId: this.novaMiss.atributoId, dataLimite: this.novaMiss.dataLimite || null, missaoPrincipalId: this.novaMiss.missaoPrincipalId || null, proprio: this.escopoProprio(this.novaMiss.proprio), classeIds: this.novaMiss.classeIds, travaDias: +this.novaMiss.travaDias };
    this.api.criarMissao(payload).subscribe({
      next: () => {
        this.novaMiss = { titulo: '', tipoId: this.tiposMissao[0]?.id ?? 1, recompensaXp: 10, recompensaMoedas: 30, atributoId: null, dataLimite: null, missaoPrincipalId: null, proprio: false, classeIds: [], travaDias: 0 };
        this.mostrarFeedback('miss', 'Missão criada!');
        this.carregar();
      },
      error: e => { console.error('Erro ao criar missão:', e); this.mostrarFeedback('miss', this.erroMsg(e), 'warn'); }
    });
  }

  iniciarEdicaoMiss(m: Missao) {
    this.editandoMissId = m.id;
    this.editMiss = { titulo: m.titulo, tipoId: m.tipoId, recompensaXp: m.recompensaXp, recompensaMoedas: m.recompensaMoedas, concluida: m.concluida, atributoId: m.atributoId ?? null, dataLimite: m.dataLimite ? m.dataLimite.slice(0, 10) : null, missaoPrincipalId: m.missaoPrincipalId ?? null };
  }

  salvarEdicaoMiss() {
    if (!this.editandoMissId) return;
    if (!this.editMiss.atributoId) { this.mostrarFeedback('miss', 'Selecione um atributo!', 'warn'); return; }
    const payload = { titulo: this.editMiss.titulo, tipoId: +this.editMiss.tipoId, recompensaXp: +this.editMiss.recompensaXp, recompensaMoedas: +this.editMiss.recompensaMoedas, concluida: this.editMiss.concluida, atributoId: this.editMiss.atributoId, dataLimite: this.editMiss.dataLimite || null, missaoPrincipalId: this.editMiss.missaoPrincipalId || null };
    this.api.editarMissao(this.editandoMissId, payload).subscribe({ next: () => {
      this.editandoMissId = null;
      this.mostrarFeedback('miss', 'Missão atualizada!');
      this.carregar();
    }});
  }

  excluirMissao(id: number) {
    if (!confirm('Excluir esta missão?')) return;
    this.api.excluirMissao(id).subscribe({ next: () => this.carregar() });
  }

  // ── Recompensas ───────────────────────────────────
  salvarRecompensa() {
    if (!this.novaRecomp.nome.trim()) return;
    const payload = { ...this.novaRecomp, preco: +this.novaRecomp.preco, perfilId: this.perfilId, proprio: this.escopoProprio(this.novaRecomp.proprio), travaDias: +this.novaRecomp.travaDias };
    this.api.criarRecompensa(payload).subscribe({
      next: () => {
        this.novaRecomp = { nome: '', descricao: '', emoji: '🎁', preco: 50, atributoId: null, pontosNecessarios: 0, proprio: false, classeIds: [], travaDias: 0 };
        this.mostrarFeedback('recomp', 'Recompensa criada!');
        this.carregar();
      },
      error: e => this.mostrarFeedback('recomp', this.erroMsg(e), 'warn')
    });
  }

  iniciarEdicaoRecomp(r: Recompensa) {
    this.editandoRecompId = r.id;
    this.editRecomp = { nome: r.nome, descricao: r.descricao, emoji: r.emoji, preco: r.preco, ativa: r.ativa, atributoId: r.atributoId ?? null, pontosNecessarios: r.pontosNecessarios ?? 0 };
  }

  salvarEdicaoRecomp() {
    if (!this.editandoRecompId) return;
    const payload = { ...this.editRecomp, preco: +this.editRecomp.preco };
    this.api.editarRecompensa(this.editandoRecompId, payload).subscribe({ next: () => {
      this.editandoRecompId = null;
      this.mostrarFeedback('recomp', 'Recompensa atualizada!');
      this.carregar();
    }});
  }

  excluirRecomp(id: number) {
    if (!confirm('Excluir esta recompensa?')) return;
    this.api.excluirRecompensa(id).subscribe({ next: () => this.carregar() });
  }

  onEmojiInput(event: Event, destino: 'nova' | 'edit') {
    const input = event.target as HTMLInputElement;
    // Pega apenas o último grapheme (emoji) digitado
    const segments = [...new Intl.Segmenter().segment(input.value)];
    const ultimo = segments.at(-1)?.segment ?? '';
    input.value = ultimo;
    if (destino === 'nova') this.novaRecomp.emoji = ultimo;
    else                    this.editRecomp.emoji  = ultimo;
  }

  // ── Reset ─────────────────────────────────────────
  resetar() {
    this.api.resetarPerfil(this.perfilId).subscribe({ next: p => {
      this.profile.setPerfilAtivo(p);
      this.confirmarReset = false;
      this.carregar();
      alert('Perfil resetado com sucesso!');
    }});
  }

  // ── Apagar perfil ─────────────────────────────────
  get nomePerfilAtual(): string {
    return this.profile.perfilAtivo()?.nome ?? '';
  }

  apagar() {
    if (this.digitouNome !== this.nomePerfilAtual) return;
    this.api.excluirPerfil(this.perfilId).subscribe({
      next: () => {
        this.profile.clearPerfil();
        this.router.navigate(['/cadastro']);
      }
    });
  }

  // ── Perfil ────────────────────────────────────────
  get perfilAtual(): Perfil | null { return this.profile.perfilAtivo(); }

  get fotoUrlAtual(): string | null {
    const f = this.profile.perfilAtivo()?.fotoUrl;
    if (!f) return null;
    return f.startsWith('/') ? `${environment.apiUrl.replace('/api', '')}${f}` : f;
  }

  toggleSecaoPerfil() {
    this.secaoPerfil = !this.secaoPerfil;
    if (this.secaoPerfil) this.carregarPerfil();
  }

  carregarPerfil() {
    const p = this.profile.perfilAtivo();
    if (!p) return;
    this.editNome    = p.nome;
    this.editClasseId = (p as any).classeId ?? null;
    this.editGenero   = (p as any).genero   ?? null;
    if (this.fotoPreviewConfig) URL.revokeObjectURL(this.fotoPreviewConfig);
    this.fotoFileConfig    = null;
    this.fotoPreviewConfig = null;
  }

  onFotoSelecionadaConfig(event: Event) {
    const input = event.target as HTMLInputElement;
    const file  = input.files?.[0];
    if (!file) return;
    const ok = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];
    if (!ok.includes(file.type)) { this.mostrarFeedback('perfil', 'Use JPG, PNG, GIF ou WebP.', 'warn'); return; }
    if (file.size > 5 * 1024 * 1024) { this.mostrarFeedback('perfil', 'Imagem muito grande (máx 5MB).', 'warn'); return; }
    if (this.fotoPreviewConfig) URL.revokeObjectURL(this.fotoPreviewConfig);
    this.fotoFileConfig    = file;
    this.fotoPreviewConfig = URL.createObjectURL(file);
  }

  removerFotoConfig() {
    if (this.fotoPreviewConfig) URL.revokeObjectURL(this.fotoPreviewConfig);
    this.fotoFileConfig    = null;
    this.fotoPreviewConfig = null;
  }

  salvarPerfil() {
    if (!this.editNome.trim()) { this.mostrarFeedback('perfil', 'Digite um nome!', 'warn'); return; }
    if (this.salvandoPerfil) return;
    this.salvandoPerfil = true;
    this.api.atualizarPerfilInfo(this.perfilId, this.editNome.trim(), this.editClasseId, this.editGenero).subscribe({
      next: p => {
        this.profile.setPerfilAtivo(p);
        if (this.fotoFileConfig) {
          this.api.uploadFoto(p.id, this.fotoFileConfig!).subscribe({
            next: atualizado => {
              this.profile.setPerfilAtivo(atualizado);
              this.removerFotoConfig();
              this.salvandoPerfil = false;
              this.mostrarFeedback('perfil', 'Perfil atualizado!');
            },
            error: () => { this.salvandoPerfil = false; this.mostrarFeedback('perfil', 'Info salva, mas erro na foto.', 'warn'); }
          });
        } else {
          this.salvandoPerfil = false;
          this.mostrarFeedback('perfil', 'Perfil atualizado!');
        }
      },
      error: () => { this.salvandoPerfil = false; this.mostrarFeedback('perfil', 'Erro ao salvar.', 'warn'); }
    });
  }

  // ── Helpers ───────────────────────────────────────
  nomeTipo(tipoId: number) {
    return this.tiposMissao.find(t => t.id === tipoId)?.nome ?? '';
  }

  get missoesPrincipais() {
    const idSecundaria = this.tiposMissao.find(t => t.nome === 'Secundária')?.id;
    return this.missoes.filter(m => m.tipoId !== idSecundaria);
  }

  get tipoSecundariaId() {
    return this.tiposMissao.find(t => t.nome === 'Secundária')?.id ?? -1;
  }

  nomeAtributo(atributoId: number | null | undefined): string {
    if (!atributoId) return '';
    const a = this.atributos.find(a => a.id === atributoId);
    return a ? `${a.emoji} ${a.nome}` : '';
  }

  private mostrarFeedback(tipo: 'bom' | 'mau' | 'miss' | 'recomp' | 'perfil', msg: string, classe: 'good' | 'warn' = 'good') {
    if (tipo === 'bom')    { this.feedbackBomTipo = classe; this.feedbackBom    = msg; setTimeout(() => this.feedbackBom    = '', 3000); }
    if (tipo === 'mau')    { this.feedbackMauTipo = classe; this.feedbackMau    = msg; setTimeout(() => this.feedbackMau    = '', 3000); }
    if (tipo === 'miss')   { this.feedbackMissTipo = classe; this.feedbackMiss   = msg; setTimeout(() => this.feedbackMiss   = '', 3000); }
    if (tipo === 'recomp') { this.feedbackRecompTipo = classe; this.feedbackRecomp = msg; setTimeout(() => this.feedbackRecomp = '', 3000); }
    if (tipo === 'perfil') { this.feedbackPerfilTipo = classe; this.feedbackPerfil = msg; setTimeout(() => this.feedbackPerfil = '', 3000); }
  }
}
