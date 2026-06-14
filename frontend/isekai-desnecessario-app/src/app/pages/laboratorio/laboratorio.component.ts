import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { ProfileService } from '../../services/profile.service';
import { Experimento } from '../../models/models';

@Component({
  selector: 'app-laboratorio',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './laboratorio.component.html',
  styleUrl: './laboratorio.component.scss',
})
export class LaboratorioComponent implements OnInit {
  private api     = inject(ApiService);
  private profile = inject(ProfileService);
  private router  = inject(Router);

  experimentos: Experimento[] = [];

  mostraForm    = false;
  novoTitulo    = '';
  novaDescricao = '';
  novaDuracao   = 21;
  msg           = '';

  ngOnInit() {
    const savedId = this.profile.getSavedId();
    if (!savedId) { this.router.navigate(['/cadastro']); return; }
    if (!this.profile.perfilAtivo()) {
      this.api.getPerfil(savedId).subscribe({ next: p => { this.profile.setPerfilAtivo(p); this.carregar(); } });
    } else {
      this.carregar();
    }
  }

  carregar() {
    this.api.getExperimentos(this.profile.id).subscribe(e => this.experimentos = e);
  }

  get ativos()      { return this.experimentos.filter(e => e.ativo); }
  get concluidos()  { return this.experimentos.filter(e => !e.ativo && !e.convertido); }
  get convertidos() { return this.experimentos.filter(e => e.convertido); }

  progresso(e: Experimento): number {
    return Math.min(100, Math.round((e.dias.length / e.duracaoDias) * 100));
  }

  diasRestantes(e: Experimento): number {
    return Math.max(0, e.duracaoDias - e.dias.length);
  }

  marcouHoje(e: Experimento): boolean {
    const hoje = new Date().toISOString().slice(0, 10);
    return e.dias.some(d => d.data.slice(0, 10) === hoje);
  }

  marcarHoje(e: Experimento) {
    if (this.marcouHoje(e)) return;
    this.api.marcarDiaExperimento(e.id).subscribe({
      next: () => { this.msg = '✅ Dia registrado!'; this.carregar(); setTimeout(() => this.msg = '', 2500); },
      error: () => { this.msg = 'Já registrado hoje.'; setTimeout(() => this.msg = '', 2500); }
    });
  }

  converter(e: Experimento) {
    this.api.converterExperimento(e.id, this.profile.id).subscribe({
      next: () => { this.msg = '🏆 Experimento virou hábito!'; this.carregar(); setTimeout(() => this.msg = '', 3000); }
    });
  }

  excluir(e: Experimento) {
    if (!confirm(`Excluir "${e.titulo}"?`)) return;
    this.api.excluirExperimento(e.id).subscribe(() => this.carregar());
  }

  salvarNovo() {
    if (!this.novoTitulo.trim()) return;
    this.api.criarExperimento({
      perfilId:    this.profile.id,
      titulo:      this.novoTitulo.trim(),
      descricao:   this.novaDescricao.trim(),
      duracaoDias: this.novaDuracao,
    }).subscribe({
      next: () => {
        this.mostraForm = false;
        this.novoTitulo = ''; this.novaDescricao = ''; this.novaDuracao = 21;
        this.carregar();
      }
    });
  }

  cancelarForm() {
    this.mostraForm = false;
    this.novoTitulo = ''; this.novaDescricao = ''; this.novaDuracao = 21;
  }
}
