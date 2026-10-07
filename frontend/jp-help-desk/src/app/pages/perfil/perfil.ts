import { DatePipe } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormGroupDirective, NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBar } from '@angular/material/snack-bar';
import { finalize } from 'rxjs';
import { PerfilBadge } from '../../components/badges/perfil-badge';
import { ForcaSenha } from '../../components/forca-senha/forca-senha';
import { iniciais } from '../../models/usuario';
import { AuthService } from '../../services/auth.service';
import { mensagemErro } from '../../services/erro-api';
import { confirmaCampo, LIMITES_SENHA, senhaForte } from '../../validacao/senha';

/** Dados do usuário logado e troca da própria senha (todos os perfis). */
@Component({
  selector: 'app-perfil',
  imports: [
    DatePipe, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule,
    MatProgressBarModule, ForcaSenha, PerfilBadge,
  ],
  templateUrl: './perfil.html',
  styleUrl: './perfil.scss',
})
export class Perfil {
  private readonly auth = inject(AuthService);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly usuario = this.auth.usuario;
  protected readonly iniciais = computed(() => iniciais(this.usuario()?.nome ?? ''));
  protected readonly limiteSenha = LIMITES_SENHA.max;

  protected readonly salvando = signal(false);
  protected readonly erro = signal<string | null>(null);
  protected readonly mostrarSenhas = signal(false);

  protected readonly form = inject(NonNullableFormBuilder).group({
    senhaAtual: ['', Validators.required],
    novaSenha: ['', [Validators.required, Validators.maxLength(LIMITES_SENHA.max), senhaForte]],
    confirmacao: ['', [Validators.required, confirmaCampo('novaSenha')]],
  });

  constructor() {
    // A sessão guarda uma cópia dos dados do login; busca a versão atual (ex.: nome alterado pelo ADMIN).
    this.auth.recarregarUsuario().pipe(takeUntilDestroyed()).subscribe({ error: () => undefined });

    // A confirmação depende da nova senha: revalida quando ela muda.
    this.form.controls.novaSenha.valueChanges
      .pipe(takeUntilDestroyed())
      .subscribe(() => this.form.controls.confirmacao.updateValueAndValidity({ emitEvent: false }));
  }

  protected alterarSenha(formDiretiva: FormGroupDirective): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { senhaAtual, novaSenha } = this.form.getRawValue();
    this.erro.set(null);
    this.salvando.set(true);

    this.auth
      .alterarSenha({ senhaAtual, novaSenha })
      .pipe(finalize(() => this.salvando.set(false)))
      .subscribe({
        next: () => {
          formDiretiva.resetForm();
          this.mostrarSenhas.set(false);
          this.snackBar.open('Senha alterada com sucesso.', 'OK', { duration: 4000 });
        },
        error: (erro: unknown) => this.erro.set(mensagemErro(erro)),
      });
  }
}
