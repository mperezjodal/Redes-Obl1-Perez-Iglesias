using System;
using System.Collections.Generic;
using Domain;

namespace ClientSocket
{
    public class Display
    {
        public static void ClientMenu() {
            Console.WriteLine();
            Console.WriteLine(@"###############################################");
            Console.WriteLine(@"#                                             #");
            Console.WriteLine(@"#       |¯\ /¯| | ____| |¯\ |¯| | | | |       #");
            Console.WriteLine(@"#       |  ¯  | | __|   |  \| | | |_| |       #");
            Console.WriteLine(@"#       |     | |_____| | \   | |_____|       #");
            Console.WriteLine(@"#                                             #");
            Console.WriteLine(@"#   Seleccione una opción:                    #");
            Console.WriteLine(@"#                                             #");
            Console.WriteLine(@"#   1-   Publicar juego                       #");
            Console.WriteLine(@"#   2-   Modificar juego                      #");
            Console.WriteLine(@"#   3-   Eliminar juego                       #");
            Console.WriteLine(@"#   4-   Buscar juego                         #");
            Console.WriteLine(@"#   5-   Calificar juegos                     #");
            Console.WriteLine(@"#        exit                                 #");
            Console.WriteLine(@"#                                             #");
            Console.WriteLine(@"###############################################");
            Console.WriteLine();
        }

        public static Game InputGame() {
            //try catch
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
            GameList(games);

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

        public static Game ModifyGame(List<Game> games){
            GameList(games);

            Game selectedGame = null;
            Console.WriteLine("Ingrese el título del juego que desea modificar:");
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

        public static void GameList(List<Game> games){
            Console.WriteLine("Lista de juegos:");
            foreach(Game g in games){
                Console.WriteLine(g.Title);
            }
        }
    }
}