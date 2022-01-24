using Tab;
using Tab.Enums;
using Xadrez;

namespace xadrez_console
{
    class Tela
    {
        public static void ImprimirPartida(PartidaDeXadrez partida)
        {
            ImprimirTabuleiro(partida.Tabuleiro);
            Console.WriteLine();
            ImprimirPecasCapturadas(partida);
        }

        private static void ImprimirPecasCapturadas(PartidaDeXadrez partida)
        {
            Console.WriteLine("Peças capturadas:");
            Console.Write("Brancas: ");
            ImprimirConjunto(partida.PecasCapturadas(Cor.Branco));
            Console.WriteLine();
            Console.Write("Pretas: ");
            ConsoleColor corOriginal = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Blue;
            ImprimirConjunto(partida.PecasCapturadas(Cor.Preto));
            Console.ForegroundColor = corOriginal;
            Console.WriteLine();
        }

        private static void ImprimirConjunto(HashSet<Peca> conjunto)
        {
            Console.Write("[");
            foreach (Peca peca in conjunto)
            {
                Console.Write(peca + " ");
            }
            Console.Write("]");
        }

        public static void ImprimirTabuleiro(Tabuleiro tabuleiro)
        {
            for (int i = 0; i < tabuleiro.Linhas; i++)
            {
                Console.Write(8 - i + " ");
                for (int j = 0; j < tabuleiro.Colunas; j++)
                {
                    ImprimirPeca(tabuleiro.Peca(i, j));
                }
                Console.WriteLine();
            }
            Console.WriteLine("  a b c d e f g h");
        }

        public static void ImprimirTabuleiro(Tabuleiro tabuleiro, bool[,] posicoesPossiveis)
        {
            ConsoleColor fundoOriginal = Console.BackgroundColor;
            ConsoleColor fundoAlterado = ConsoleColor.DarkGray;

            for (int i = 0; i < tabuleiro.Linhas; i++)
            {
                Console.Write(8 - i + " ");
                for (int j = 0; j < tabuleiro.Colunas; j++)
                {
                    if (posicoesPossiveis[i, j])
                    {
                        Console.BackgroundColor = fundoAlterado;
                    }
                    else
                    {
                        Console.BackgroundColor = fundoOriginal;
                    }
                    ImprimirPeca(tabuleiro.Peca(i, j));
                    Console.BackgroundColor = fundoOriginal;
                }
                Console.WriteLine();
            }
            Console.WriteLine("  a b c d e f g h");
            Console.BackgroundColor = fundoOriginal;
        }

        public static void ImprimirPeca(Peca peca)
        {
            if (peca == null)
            {
                Console.Write("- ");
            }
            else
            {
                if (peca.Cor == Cor.Branco)
                {
                    Console.Write(peca);
                }
                else
                {
                    ConsoleColor fundoOriginal = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write(peca);
                    Console.ForegroundColor = fundoOriginal;
                }
                Console.Write(" ");
            }
        }

        public static PosicaoXadrez LerPosicaoXadrez()
        {
            string s = Console.ReadLine();
            char coluna = s[0];
            int linha = int.Parse(s[1] + "");
            return new PosicaoXadrez(coluna, linha);
        }
    }
}