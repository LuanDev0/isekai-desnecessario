import { Routes } from '@angular/router';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { MissoesComponent } from './pages/missoes/missoes.component';
import { LojaComponent } from './pages/loja/loja.component';
import { ConfiguracoesComponent } from './pages/configuracoes/configuracoes.component';
import { CadastroComponent } from './pages/cadastro/cadastro.component';
import { GraficoComponent } from './pages/grafico/grafico.component';
import { InventarioComponent } from './pages/inventario/inventario.component';
import { StatusComponent } from './pages/status/status.component';
import { LaboratorioComponent } from './pages/laboratorio/laboratorio.component';
import { MundoComponent } from './pages/mundo/mundo.component';

export const routes: Routes = [
  { path: 'cadastro',      component: CadastroComponent },
  { path: '',              component: DashboardComponent },
  { path: 'missoes',       component: MissoesComponent },
  { path: 'status',        component: StatusComponent },
  { path: 'loja',          component: LojaComponent },
  { path: 'grafico',       component: GraficoComponent },
  { path: 'inventario',    component: InventarioComponent },
  { path: 'configuracoes', component: ConfiguracoesComponent },
  { path: 'laboratorio',   component: LaboratorioComponent },
  { path: 'mundo',         component: MundoComponent },
];
