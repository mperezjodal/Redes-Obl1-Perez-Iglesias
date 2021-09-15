using System;
using System.Collections.Generic;

namespace Domain
{
    public class DialogUtils
    {
        public static void Menu(Dictionary<string, string> items) {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Console.WriteLine();
            Console.ResetColor();
            DialogUtils.MenuTitle();
            Console.WriteLine();
            Console.WriteLine("    Seleccione una opción:                     ");
            Console.WriteLine();
            foreach(var menuOption in items){
                Console.WriteLine("    " + menuOption.Key + " -   "  + menuOption.Value);       
            }
            Console.WriteLine("         exit                                  ");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Console.ResetColor();
            Console.WriteLine();
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

        public static Game SelectGame(List<Game> games){
            DialogUtils.GameList(games);

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

        public static void GameFilterOptions(List<Game> games){
            Console.WriteLine();
            Console.WriteLine("Seleccione una opción para filtrar juegos:");
            Console.WriteLine();
            Console.WriteLine("    1-   Categoría");
            Console.WriteLine("    2-   Título");
            Console.WriteLine("    3-   Rating");
            Console.WriteLine();
            var filter =  Console.ReadLine();
            List<Game> filtedGames = new List<Game>();
            switch (filter)
                {
                    case "1":
                        Console.WriteLine("Ingrese categoría.");
                        var cat = Console.ReadLine();
                        filtedGames = games.FindAll(g => g.Genre.Equals(cat));
                        DialogUtils.GameList(filtedGames);
                    break;
                    case "2": 
                        Console.WriteLine("Ingrese tituilo.");
                        var title = Console.ReadLine();
                        filtedGames = games.FindAll(g => g.Title.Equals(title));
                        DialogUtils.GameList(filtedGames);
                    break;
                    case "3": 
                    Console.WriteLine("Ingrese categoría.");
                        int rating;
                        Int32.TryParse(Console.ReadLine(), out rating);
                        filtedGames = games.FindAll(g => g.Rating.Equals(rating));
                        DialogUtils.GameList(filtedGames);
                    break;
                    default:
                    Console.WriteLine("Opción inválida.");
                    break;
                }
        }
        
        public static void ShowGameDetail(List<Game> games) {
            Game gameToShow = SelectGame(games);
            Console.Write("Juego: ");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(gameToShow.Title);
            Console.ResetColor();
            Console.Write("Género: ");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(gameToShow.Genre);
            Console.ResetColor();
            Console.Write("Sinópsis: ");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(gameToShow.Synopsis);
            Console.ResetColor();
            Console.WriteLine("Promedio de Calificaciones:");
        }
    }
}