using System;
using System.Collections.Generic;
using Domain;

namespace ServerSocket
{
    public class Display
    {
        public static void ServerMenu() {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine(@"        |¯\ /¯| | ____| |¯\ |¯| | | | |        ");
            Console.WriteLine(@"        |  ¯  | | __|   |  \| | | |_| |        ");
            Console.WriteLine(@"        |     | |_____| | \   | |_____|        ");
            Console.WriteLine();
            Console.WriteLine("    Seleccione una opción:                     ");
            Console.WriteLine();
            Console.WriteLine("    1-   Ver catálogo de juegos                ");
            Console.WriteLine("    2-   Adquirir juego                        ");
            Console.WriteLine("    3-   Publicar juego                        ");
            Console.WriteLine("    4-   Publicar calificación de un juego     ");
            Console.WriteLine("    5-   Buscar juegos                         ");
            Console.WriteLine("         exit                                  ");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Console.ResetColor();
            Console.WriteLine();
        }

        public static void GameList(List<Game> games){
            Console.WriteLine("Lista de juegos:");
            foreach(Game g in games){
                Console.WriteLine(g.Title);
            }
        }

        public static Game InputGame() {
            Game newGame = new Game();

            Console.WriteLine("Título:");
            newGame.Title = Console.ReadLine();
            Console.WriteLine("Género:");
            newGame.Genre = Console.ReadLine();
            Console.WriteLine("Calificación (número del 1 al 10):");
            int rating;
            Int32.TryParse(Console.ReadLine(), out rating);
            newGame.Rating = rating;
            Console.WriteLine("Sinópsis:");
            newGame.Synopsis = Console.ReadLine();

            return newGame;
        }
    }
}