import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { ProfileService } from '../../services/profile.service';
import { ItemInventario } from '../../models/models';

export type AbaInventario = 'disponiveis' | 'usados';

export interface Pilha {
  nome:        string;
  emoji:       string;
  descricao:   string;
  preco:       number;
  quantidade:  number;
  itens:       ItemInventario[];
  ultimaCompra: string;
  dataUso:     string | null;
}

@Component({
  selector: 'app-inventario',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './inventario.component.html',
  styleUrl: './inventario.component.scss',
})
export class InventarioComponent implements OnInit {
  private api     = inject(ApiService);
  private profile = inject(ProfileService);
  private router  = inject(Router);

  itens:     ItemInventario[] = [];
  aba:       AbaInventario    = 'disponiveis';
  usando:    number | null    = null;
  feedbacks: Record<number, 'ok' | 'erro'> = {};

  ngOnInit() {
    const savedId = this.profile.getSavedId();
    if (!savedId) { this.router.navigate(['/cadastro']); return; }
    this.carregar();
  }

  carregar() {
    this.api.getInventario(this.profile.id).subscribe({ next: itens => this.itens = itens });
  }

  get disponiveis(): ItemInventario[] { return this.itens.filter(i => !i.usado); }
  get usados():      ItemInventario[] { return this.itens.filter(i =>  i.usado); }

  get pilhas(): Pilha[] {
    const lista = this.aba === 'disponiveis' ? this.disponiveis : this.usados;
    const mapa  = new Map<string, Pilha>();

    for (const item of lista) {
      if (mapa.has(item.nome)) {
        const p = mapa.get(item.nome)!;
        p.quantidade++;
        p.itens.push(item);
        if (new Date(item.dataCompra) > new Date(p.ultimaCompra)) p.ultimaCompra = item.dataCompra;
      } else {
        mapa.set(item.nome, {
          nome:        item.nome,
          emoji:       item.emoji,
          descricao:   item.descricao,
          preco:       item.preco,
          quantidade:  1,
          itens:       [item],
          ultimaCompra: item.dataCompra,
          dataUso:     item.dataUso ?? null,
        });
      }
    }

    return [...mapa.values()].sort((a, b) =>
      new Date(b.ultimaCompra).getTime() - new Date(a.ultimaCompra).getTime()
    );
  }

  usar(pilha: Pilha) {
    const item = pilha.itens[0];
    if (this.usando !== null) return;
    this.usando = item.id;

    this.api.usarItem(item.id, this.profile.id).subscribe({
      next: () => {
        this.feedbacks[item.id] = 'ok';
        setTimeout(() => { delete this.feedbacks[item.id]; this.usando = null; this.carregar(); }, 1000);
      },
      error: () => {
        this.feedbacks[item.id] = 'erro';
        this.usando = null;
        setTimeout(() => delete this.feedbacks[item.id], 2000);
      }
    });
  }

  get totalComprado(): number { return this.itens.length; }
  get totalUsado():    number { return this.itens.filter(i => i.usado).length; }

  get favorito(): { nome: string; emoji: string } | null {
    if (this.itens.length === 0) return null;
    const freq = new Map<string, { count: number; emoji: string }>();
    for (const i of this.itens)
      freq.set(i.nome, { count: (freq.get(i.nome)?.count ?? 0) + 1, emoji: i.emoji });
    const [, val] = [...freq.entries()].sort((a, b) => b[1].count - a[1].count)[0];
    return { nome: val.emoji + ' ' + [...freq.entries()].sort((a,b)=>b[1].count-a[1].count)[0][0], emoji: val.emoji };
  }

  fmtData(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('pt-BR', {
      day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit'
    });
  }
}
