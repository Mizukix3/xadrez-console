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
        private HashSet<Peca> Pecas;
        private HashSet<Peca> Capturadas;
        public bool Xeque { get; private set; }

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
            Peca p = Tabuleiro.RetirarPeca(origem);
            p.IncrementarQtdMovimentos();
            Peca pecaCapturada = Tabuleiro.RetirarPeca(destino);
            Tabuleiro.ColocarPeca(p, destino);
            if (pecaCapturada != null)
            {
                Capturadas.Add(pecaCapturada);
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
        }

        public void RealizaJogada(Posicao origem, Posicao destino)
        {
            Peca pecaCapturada = ExecutaMovimento(origem, destino);

            if (EstaEmXeque(JogadorAtual))
            {
                DesfazMovimento(origem, destino, pecaCapturada);
                throw new TabuleiroException("Você não pode se colocar em xeque!");
            }

            if (EstaEmXeque(Adversario(JogadorAtual)))
            {
                Xeque = true;
            }
            else
            {
                Xeque = false;
            }

            if (TesteXequeMate(Adversario(JogadorAtual))) {
                Terminada = true;
            } else
            {
                Turno++;
                MudaJogador();
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
                        if (mat[i,j])
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
            ColocarNovaPeca('e', 1, new Rei(Cor.Branco, Tabuleiro));
            ColocarNovaPeca('f', 1, new Bispo(Cor.Branco, Tabuleiro));
            ColocarNovaPeca('g', 1, new Cavalo(Cor.Branco, Tabuleiro));
            ColocarNovaPeca('h', 1, new Torre(Cor.Branco, Tabuleiro));
            ColocarNovaPeca('a', 2, new Peao(Cor.Branco, Tabuleiro));
            ColocarNovaPeca('b', 2, new Peao(Cor.Branco, Tabuleiro));
            ColocarNovaPeca('c', 2, new Peao(Cor.Branco, Tabuleiro));
            ColocarNovaPeca('d', 2, new Peao(Cor.Branco, Tabuleiro));
            ColocarNovaPeca('e', 2, new Peao(Cor.Branco, Tabuleiro));
            ColocarNovaPeca('f', 2, new Peao(Cor.Branco, Tabuleiro));
            ColocarNovaPeca('g', 2, new Peao(Cor.Branco, Tabuleiro));
            ColocarNovaPeca('h', 2, new Peao(Cor.Branco, Tabuleiro));

            ColocarNovaPeca('a', 8, new Torre(Cor.Preto, Tabuleiro));
            ColocarNovaPeca('b', 8, new Cavalo(Cor.Preto, Tabuleiro));
            ColocarNovaPeca('c', 8, new Bispo(Cor.Preto, Tabuleiro));
            ColocarNovaPeca('d', 8, new Dama(Cor.Preto, Tabuleiro));
            ColocarNovaPeca('e', 8, new Rei(Cor.Preto, Tabuleiro));
            ColocarNovaPeca('f', 8, new Bispo(Cor.Preto, Tabuleiro));
            ColocarNovaPeca('g', 8, new Cavalo(Cor.Preto, Tabuleiro));
            ColocarNovaPeca('h', 8, new Torre(Cor.Preto, Tabuleiro));
            ColocarNovaPeca('a', 7, new Peao(Cor.Preto, Tabuleiro));
            ColocarNovaPeca('b', 7, new Peao(Cor.Preto, Tabuleiro));
            ColocarNovaPeca('c', 7, new Peao(Cor.Preto, Tabuleiro));
            ColocarNovaPeca('d', 7, new Peao(Cor.Preto, Tabuleiro));
            ColocarNovaPeca('e', 7, new Peao(Cor.Preto, Tabuleiro));
            ColocarNovaPeca('f', 7, new Peao(Cor.Preto, Tabuleiro));
            ColocarNovaPeca('g', 7, new Peao(Cor.Preto, Tabuleiro));
            ColocarNovaPeca('h', 7, new Peao(Cor.Preto, Tabuleiro));
        }
    }
}
