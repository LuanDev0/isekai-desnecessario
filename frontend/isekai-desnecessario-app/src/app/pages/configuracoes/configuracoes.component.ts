import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { ProfileService } from '../../services/profile.service';
import { LanguageService } from '../../services/language.service';
import { TranslatePipe } from '../../pipes/translate.pipe';
import { Atributo, BomHabito, Classe, MauHabito, Missao, Perfil, Recompensa } from '../../models/models';
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

  get perfilId() { return this.profile.id; }

  // Listas
  bonsHabitos:  BomHabito[]  = [];
  mausHabitos:  MauHabito[]  = [];
  missoes:      Missao[]     = [];
  recompensas:  Recompensa[] = [];
  tiposMissao:  { id: number; nome: string }[] = [];
  atributos:    Atributo[]   = [];
  classes:      Classe[]     = [];

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

  // Formulários de criação
  novoBom  = { habito: '', xp: 50, frequencia: 'Livre', atributoId: null as number | null };
  novoMau  = { habito: '', xp: 50, frequencia: 'Livre', atributoId: null as number | null };
  novaMiss = { titulo: '', tipoId: 1, recompensaXp: 10, recompensaMoedas: 30, atributoId: null as number | null, dataLimite: null as string | null, missaoPrincipalId: null as number | null };
  novaRecomp = { nome: '', descricao: '', emoji: '🎁', preco: 50, atributoId: null as number | null, pontosNecessarios: 0 };

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
  secaoBons    = false;
  secaoMaus    = false;
  secaoMiss    = false;
  secaoRecomp  = false;
  secaoDanger  = false;

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
  }

  // ── Bons hábitos ──────────────────────────────────
  salvarBomHabito() {
    if (!this.novoBom.habito.trim()) return;
    if (!this.novoBom.atributoId) { this.mostrarFeedback('bom', 'Selecione um atributo!', 'warn'); return; }
    const payload = { habito: this.novoBom.habito, xp: +this.novoBom.xp, frequencia: this.novoBom.frequencia, perfilId: this.perfilId, atributoId: this.novoBom.atributoId };
    this.api.criarBomHabito(payload).subscribe({
      next: () => {
        this.novoBom = { habito: '', xp: 50, frequencia: 'Livre', atributoId: null };
        this.mostrarFeedback('bom', 'Hábito criado!');
        this.carregar();
      },
      error: e => { console.error('Erro ao criar bom hábito:', e); this.mostrarFeedback('bom', 'Erro ao cadastrar!'); }
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
    const payload = { habito: this.novoMau.habito, xp: +this.novoMau.xp, frequencia: this.novoMau.frequencia, perfilId: this.perfilId, atributoId: this.novoMau.atributoId };
    this.api.criarMauHabito(payload).subscribe({
      next: () => {
        this.novoMau = { habito: '', xp: 50, frequencia: 'Livre', atributoId: null };
        this.mostrarFeedback('mau', 'Hábito criado!');
        this.carregar();
      },
      error: e => { console.error('Erro ao criar mau hábito:', e); this.mostrarFeedback('mau', 'Erro ao cadastrar!'); }
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
    const payload = { titulo: this.novaMiss.titulo, tipoId: +this.novaMiss.tipoId, recompensaXp: +this.novaMiss.recompensaXp, recompensaMoedas: +this.novaMiss.recompensaMoedas, perfilId: this.perfilId, atributoId: this.novaMiss.atributoId, dataLimite: this.novaMiss.dataLimite || null, missaoPrincipalId: this.novaMiss.missaoPrincipalId || null };
    this.api.criarMissao(payload).subscribe({
      next: () => {
        this.novaMiss = { titulo: '', tipoId: this.tiposMissao[0]?.id ?? 1, recompensaXp: 10, recompensaMoedas: 30, atributoId: null, dataLimite: null, missaoPrincipalId: null };
        this.mostrarFeedback('miss', 'Missão criada!');
        this.carregar();
      },
      error: e => { console.error('Erro ao criar missão:', e); this.mostrarFeedback('miss', 'Erro ao cadastrar!'); }
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
    const payload = { ...this.novaRecomp, preco: +this.novaRecomp.preco, perfilId: this.perfilId };
    this.api.criarRecompensa(payload).subscribe({
      next: () => {
        this.novaRecomp = { nome: '', descricao: '', emoji: '🎁', preco: 50, atributoId: null, pontosNecessarios: 0 };
        this.mostrarFeedback('recomp', 'Recompensa criada!');
        this.carregar();
      },
      error: () => this.mostrarFeedback('recomp', 'Erro ao cadastrar!', 'warn')
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
