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
            MenuTitle();
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
            Console.WriteLine();
            foreach(Game g in games){
                Console.WriteLine(g.Title);
            }
        }

        
        public static void ShowGameDetail(List<Game> games) {
            Console.WriteLine("Para ver el detalle de un juego ingrese 1. Ingrese 2 para volver al menú.");
            var detailOption = Console.ReadLine();
            switch (detailOption){
                case "1":
                    Console.WriteLine();
                    Console.WriteLine();
                    Game gameToShow = Display.SelectGame(games);
                    Console.WriteLine();
                    Console.WriteLine("Detalle del juego: " + gameToShow.Title);
                    Console.WriteLine();
                    Console.WriteLine("Género:");
                    Console.WriteLine();
                    Console.WriteLine(gameToShow.Genre);
                    Console.WriteLine();
                    Console.WriteLine("Sinópsis:");
                    Console.WriteLine();
                    Console.WriteLine(gameToShow.Synopsis);
                    Console.WriteLine();
                    Console.WriteLine("Promedio de Calificaciónes:");
                    Console.WriteLine();
                    break;
                case "2":
                    break;
                default:
                Console.WriteLine("option invalida");
                break;
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

        public static Game SelectGame(List<Game> games){
            //GameList(games);

            Game selectedGame = null;
            Console.WriteLine("Ingrese el título del juego:");
            string gameTitle = Console.ReadLine();
            selectedGame = games.Find(g => g.Title.Equals(gameTitle));

            if(selectedGame == null){
                Console.WriteLine("Juego inválido, ingrese el título nuevamente:");
                gameTitle = Console.ReadLine();
                selectedGame = games.Find(g => g.Title.Equals(gameTitle));
            }
            if(selectedGame == null){
                Console.WriteLine("Juego inválido.");
            }

            return selectedGame;
        }

        public static void MenuTitle(){
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(@"        |¯\ /¯|");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(@" | ____|");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(@" |¯\ |¯|");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(@" | | | |");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(@"        |  ¯  |");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(@" | __|  ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(@" |  \| |");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(@" | |_| |");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(@"        |     |");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(@" |_____|");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(@" | \   |");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(@" |_____|");
            Console.ResetColor();
        }

        public static void GameFilterOptions(){
            Console.WriteLine();
            Console.WriteLine("Seleccione una opción para filtrar juegos");
            Console.WriteLine();
            Console.WriteLine("    1-   Categoría                     ");
            Console.WriteLine("    2-   Titulo                        ");
            Console.WriteLine("    3-   Rating                        ");
            Console.WriteLine();
        }
    }
}