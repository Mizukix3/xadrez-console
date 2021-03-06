using Tab;
using Tab.Enums;

namespace Xadrez
{
    class PartidaDeXadrez
    {
        public Tabuleiro Tabuleiro { get; private set; }
        public int Turno { get; private set; }
        public Cor JogadorAtual { get; private set; }
        public bool Terminada { get; private set; }
        public bool Xeque { get; private set; }
        public Peca VulneravelEnPassant { get; private set; }
        private HashSet<Peca> Pecas;
        private HashSet<Peca> Capturadas;

        public PartidaDeXadrez()
        {
            Tabuleiro = new Tabuleiro(8, 8);
            Turno = 1;
            JogadorAtual = Cor.Branco;
            Terminada = false;
            Pecas = new HashSet<Peca>();
            Capturadas = new HashSet<Peca>();
            ColocarPecas();
        }

        private Peca ExecutaMovimento(Posicao origem, Posicao destino)
        {
            Peca peca = Tabuleiro.RetirarPeca(origem);
            peca.IncrementarQtdMovimentos();
            Peca pecaCapturada = Tabuleiro.RetirarPeca(destino);
            Tabuleiro.ColocarPeca(peca, destino);
            if (pecaCapturada != null)
            {
                Capturadas.Add(pecaCapturada);
            }

            // #jogadaespecial roque pequeno
            if (peca is Rei && destino.Coluna == origem.Coluna + 2)
            {
                Posicao origemT = new Posicao(origem.Linha, origem.Coluna + 3);
                Posicao destinoT = new Posicao(origem.Linha, origem.Coluna + 1);
                Peca torre = Tabuleiro.RetirarPeca(origemT);
                torre.IncrementarQtdMovimentos();
                Tabuleiro.ColocarPeca(torre, destinoT);
            }

            // #jogadaespecial roque grande
            if (peca is Rei && destino.Coluna == origem.Coluna - 2)
            {
                Posicao origemT = new Posicao(origem.Linha, origem.Coluna - 4);
                Posicao destinoT = new Posicao(origem.Linha, origem.Coluna - 1);
                Peca torre = Tabuleiro.RetirarPeca(origemT);
                torre.IncrementarQtdMovimentos();
                Tabuleiro.ColocarPeca(torre, destinoT);
            }

            // #jogadaespecial en passant
            if (peca is Peao)
            {
                if (origem.Coluna != destino.Coluna && pecaCapturada == null)
                {
                    Posicao posP;
                    if (peca.Cor == Cor.Branco)
                    {
                        posP = new Posicao(destino.Linha + 1, destino.Coluna);
                    } else
                    {
                        posP = new Posicao(destino.Linha - 1, destino.Coluna);
                    }
                    pecaCapturada = Tabuleiro.RetirarPeca(posP);
                    Capturadas.Add(pecaCapturada);
                }
            }

            return pecaCapturada;
        }

        private void DesfazMovimento(Posicao origem, Posicao destino, Peca pecaCapturada)
        {
            Peca peca = Tabuleiro.RetirarPeca(destino);
            peca.DecrementarQtdMovimentos();
            if (pecaCapturada != null)
            {
                Tabuleiro.ColocarPeca(pecaCapturada, destino);
                Capturadas.Remove(pecaCapturada);
            }
            Tabuleiro.ColocarPeca(peca, origem);

            // #jogadaespecial roque pequeno
            if (peca is Rei && destino.Coluna == origem.Coluna + 2)
            {
                Posicao origemT = new Posicao(origem.Linha, origem.Coluna + 3);
                Posicao destinoT = new Posicao(origem.Linha, origem.Coluna + 1);
                Peca torre = Tabuleiro.RetirarPeca(destinoT);
                torre.DecrementarQtdMovimentos();
                Tabuleiro.ColocarPeca(torre, origemT);
            }

            // #jogadaespecial roque grande
            if (peca is Rei && destino.Coluna == origem.Coluna - 2)
            {
                Posicao origemT = new Posicao(origem.Linha, origem.Coluna - 4);
                Posicao destinoT = new Posicao(origem.Linha, origem.Coluna - 1);
                Peca torre = Tabuleiro.RetirarPeca(destinoT);
                torre.DecrementarQtdMovimentos();
                Tabuleiro.ColocarPeca(torre, origemT);
            }

            // #jogadaespecial en passant
            if (peca is Peao)
            {
                if (origem.Coluna != destino.Coluna && pecaCapturada == VulneravelEnPassant)
                {
                    Peca peao = Tabuleiro.RetirarPeca(destino);
                    Posicao posP;
                    if (peca.Cor == Cor.Branco)
                    {
                        posP = new Posicao(3, destino.Coluna);
                    } else
                    {
                        posP = new Posicao(4, destino.Coluna);
                    }
                    Tabuleiro.ColocarPeca(peao, posP);
                }
            }
        }

        public void RealizaJogada(Posicao origem, Posicao destino)
        {
            Peca pecaCapturada = ExecutaMovimento(origem, destino);
            Peca p = Tabuleiro.Peca(destino);

            if (EstaEmXeque(JogadorAtual))
            {
                DesfazMovimento(origem, destino, pecaCapturada);
                throw new TabuleiroException("Você não pode se colocar em xeque!");
            }

            // #jogadaespecial promocao
            if (p is Peao)
            {
                if ((p.Cor == Cor.Branco && destino.Linha == 0) || (p.Cor == Cor.Preto && destino.Linha == 7))
                {
                    p = Tabuleiro.RetirarPeca(destino);
                    Pecas.Remove(p);
                    Peca dama = new Dama(p.Cor, Tabuleiro);
                    Tabuleiro.ColocarPeca(dama, destino);
                    Pecas.Add(dama);
                }
            }

            if (EstaEmXeque(Adversario(JogadorAtual)))
            {
                Xeque = true;
            }
            else
            {
                Xeque = false;
            }

            if (TesteXequeMate(Adversario(JogadorAtual)))
            {
                Terminada = true;
            }
            else
            {
                Turno++;
                MudaJogador();
            }

            // #jogadaespecial en passant
            if (p is Peao && (destino.Linha == origem.Linha - 2 || destino.Linha == origem.Linha + 2)) {
                VulneravelEnPassant = p;
            } else
            {
                VulneravelEnPassant = null;
            }
        }

        public void ValidarPosicaoDeOrigem(Posicao pos)
        {
            if (Tabuleiro.Peca(pos) == null)
            {
                throw new TabuleiroException("Não existe peça na posição de origem escolhida!");
            }
            if (JogadorAtual != Tabuleiro.Peca(pos).Cor)
            {
                throw new TabuleiroException("A peça de origem escolhida não é sua!");
            }
            if (!Tabuleiro.Peca(pos).ExisteMovimentosPossiveis())
            {
                throw new TabuleiroException("Não há movimentos possíveis para a peça de origem escolhida!");
            }
        }

        public void ValidarPosicaoDeDestino(Posicao origem, Posicao destino)
        {
            if (!Tabuleiro.Peca(origem).MovimentoPossivel(destino))
            {
                throw new TabuleiroException("Posição de destino inválida!");
            }
        }

        private void MudaJogador()
        {
            if (JogadorAtual == Cor.Branco)
            {
                JogadorAtual = Cor.Preto;
            }
            else
            {
                JogadorAtual = Cor.Branco;
            }
        }

        public HashSet<Peca> PecasCapturadas(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca peca in Capturadas)
            {
                if (peca.Cor == cor)
                {
                    aux.Add(peca);
                }
            }
            return aux;
        }

        public HashSet<Peca> PecasEmJogo(Cor cor)
        {
            HashSet<Peca> aux = new HashSet<Peca>();
            foreach (Peca peca in Pecas)
            {
                if (peca.Cor == cor)
                {
                    aux.Add(peca);
                }
            }
            aux.ExceptWith(PecasCapturadas(cor));
            return aux;
        }

        private Cor Adversario(Cor cor)
        {
            if (cor == Cor.Preto)
            {
                return Cor.Branco;
            }
            else
            {
                return Cor.Preto;
            }
        }

        private Peca Rei(Cor cor)
        {
            foreach (Peca peca in PecasEmJogo(cor))
            {
                if (peca is Rei)
                {
                    return peca;
                }
            }
            return null;
        }

        private bool EstaEmXeque(Cor cor)
        {
            Peca rei = Rei(cor);
            if (rei == null)
            {
                throw new TabuleiroException("Não tem rei da cor " + cor + " no tabuleiro!");
            }

            foreach (Peca peca in PecasEmJogo(Adversario(cor)))
            {
                bool[,] mat = peca.MovimentosPossiveis();
                if (mat[rei.Posicao.Linha, rei.Posicao.Coluna])
                {
                    return true;
                }
            }

            return false;
        }

        private bool TesteXequeMate(Cor cor)
        {
            if (!EstaEmXeque(cor))
            {
                return false;
            }

            foreach (Peca peca in PecasEmJogo(cor))
            {
                bool[,] mat = peca.MovimentosPossiveis();
                for (int i = 0; i < Tabuleiro.Linhas; i++)
                {
                    for (int j = 0; j < Tabuleiro.Colunas; j++)
                    {
                        if (mat[i, j])
                        {
                            Posicao origem = peca.Posicao;
                            Posicao destino = new Posicao(i, j);
                            Peca pecaCapturada = ExecutaMovimento(peca.Posicao, destino);
                            bool testeXeque = EstaEmXeque(cor);
                            DesfazMovimento(origem, destino, pecaCapturada);
                            if (!testeXeque)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        private void ColocarNovaPeca(char coluna, int linha, Peca peca)
        {
            Tabuleiro.ColocarPeca(peca, new PosicaoXadrez(coluna, linha).ToPosicao());
            Pecas.Add(peca);
        }

        private void ColocarPecas()
        {
            ColocarNovaPeca('a', 1, new Torre(Cor.Branco, Tabuleiro));
            ColocarNovaPeca('b', 1, new Cavalo(Cor.Branco, Tabuleiro));
            ColocarNovaPeca('c', 1, new Bispo(Cor.Branco, Tabuleiro));
            ColocarNovaPeca('d', 1, new Dama(Cor.Branco, Tabuleiro));
            ColocarNovaPeca('e', 1, new Rei(Cor.Branco, Tabuleiro, this));
            ColocarNovaPeca('f', 1, new Bispo(Cor.Branco, Tabuleiro));
            ColocarNovaPeca('g', 1, new Cavalo(Cor.Branco, Tabuleiro));
            ColocarNovaPeca('h', 1, new Torre(Cor.Branco, Tabuleiro));
            ColocarNovaPeca('a', 2, new Peao(Cor.Branco, Tabuleiro, this));
            ColocarNovaPeca('b', 2, new Peao(Cor.Branco, Tabuleiro, this));
            ColocarNovaPeca('c', 2, new Peao(Cor.Branco, Tabuleiro, this));
            ColocarNovaPeca('d', 2, new Peao(Cor.Branco, Tabuleiro, this));
            ColocarNovaPeca('e', 2, new Peao(Cor.Branco, Tabuleiro, this));
            ColocarNovaPeca('f', 2, new Peao(Cor.Branco, Tabuleiro, this));
            ColocarNovaPeca('g', 2, new Peao(Cor.Branco, Tabuleiro, this));
            ColocarNovaPeca('h', 2, new Peao(Cor.Branco, Tabuleiro, this));

            ColocarNovaPeca('a', 8, new Torre(Cor.Preto, Tabuleiro));
            ColocarNovaPeca('b', 8, new Cavalo(Cor.Preto, Tabuleiro));
            ColocarNovaPeca('c', 8, new Bispo(Cor.Preto, Tabuleiro));
            ColocarNovaPeca('d', 8, new Dama(Cor.Preto, Tabuleiro));
            ColocarNovaPeca('e', 8, new Rei(Cor.Preto, Tabuleiro, this));
            ColocarNovaPeca('f', 8, new Bispo(Cor.Preto, Tabuleiro));
            ColocarNovaPeca('g', 8, new Cavalo(Cor.Preto, Tabuleiro));
            ColocarNovaPeca('h', 8, new Torre(Cor.Preto, Tabuleiro));
            ColocarNovaPeca('a', 7, new Peao(Cor.Preto, Tabuleiro, this));
            ColocarNovaPeca('b', 7, new Peao(Cor.Preto, Tabuleiro, this));
            ColocarNovaPeca('c', 7, new Peao(Cor.Preto, Tabuleiro, this));
            ColocarNovaPeca('d', 7, new Peao(Cor.Preto, Tabuleiro, this));
            ColocarNovaPeca('e', 7, new Peao(Cor.Preto, Tabuleiro, this));
            ColocarNovaPeca('f', 7, new Peao(Cor.Preto, Tabuleiro, this));
            ColocarNovaPeca('g', 7, new Peao(Cor.Preto, Tabuleiro, this));
            ColocarNovaPeca('h', 7, new Peao(Cor.Preto, Tabuleiro, this));
        }
    }
}
