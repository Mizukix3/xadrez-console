using Tab;
using Tab.Enums;
using Xadrez;

namespace xadrez_console {
    class Program
    {
        static void Main(string[] args)
        {
            Tabuleiro tabuleiro = new Tabuleiro(8, 8);

            tabuleiro.ColocarPeca(new Torre(Cor.Preto, tabuleiro), new Posicao(0,0));
            tabuleiro.ColocarPeca(new Torre(Cor.Preto, tabuleiro), new Posicao(1, 3));
            tabuleiro.ColocarPeca(new Rei(Cor.Preto, tabuleiro), new Posicao(2, 4));

            Tela.ImprimirTabuleiro(tabuleiro);
        }
    }
}