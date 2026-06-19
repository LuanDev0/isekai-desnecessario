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
import { authGuard, guestGuard } from './services/auth.guard';

export const routes: Routes = [
  { path: 'cadastro',      component: CadastroComponent,      canActivate: [guestGuard] },
  { path: '',              component: DashboardComponent,     canActivate: [authGuard] },
  { path: 'missoes',       component: MissoesComponent,       canActivate: [authGuard] },
  { path: 'status',        component: StatusComponent,        canActivate: [authGuard] },
  { path: 'loja',          component: LojaComponent,          canActivate: [authGuard] },
  { path: 'grafico',       component: GraficoComponent,       canActivate: [authGuard] },
  { path: 'inventario',    component: InventarioComponent,    canActivate: [authGuard] },
  { path: 'configuracoes', component: ConfiguracoesComponent, canActivate: [authGuard] },
  { path: 'laboratorio',   component: LaboratorioComponent,   canActivate: [authGuard] },
  { path: 'mundo',         component: MundoComponent,         canActivate: [authGuard] },
];
