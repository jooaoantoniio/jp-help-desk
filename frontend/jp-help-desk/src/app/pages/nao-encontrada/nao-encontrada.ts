import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-nao-encontrada',
  imports: [RouterLink, MatButtonModule, MatIconModule],
  templateUrl: './nao-encontrada.html',
  styleUrl: './nao-encontrada.scss',
})
export class NaoEncontrada {}
