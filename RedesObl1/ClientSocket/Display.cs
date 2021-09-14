using System;
using System.Collections.Generic;
using Domain;

namespace ClientSocket
{
    public class Display
    {
        public static void ClientMenu() {
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
        }

        public static Game InputGame() {
            //try catch
            Game newGame = new Game();

            Console.WriteLine("Ingrese el título del juego:");
            newGame.Title = Console.ReadLine();
            Console.WriteLine(@"Ingrese el género del juego:");
            newGame.Genre = Console.ReadLine();
            Console.WriteLine(@"Ingrese el rating del juego (número del 1 al 10):");
            int rating;
            Int32.TryParse(Console.ReadLine(), out rating);
            newGame.Rating = rating;
            Console.WriteLine(@"Ingrese la sinopsis del juego:");
            newGame.Synopsis = Console.ReadLine();

            return newGame;
        }

        public static Game SelectGame(List<Game> gameList){
            Console.WriteLine("Esta es la lista de juegos:");
            foreach(Game g in gameList){
                Console.WriteLine(g.Title);
            }
            Game selectedGame = null;
            Console.WriteLine("Ingrese el título del juego que desea modificar:");
            string gameTitle = Console.ReadLine();
            selectedGame = gameList.Find(g => g.Title.Equals(gameTitle));
            if(selectedGame == null){
                Console.WriteLine("Juego inválido, ingrese el título del juego que desea modificar:");
                gameTitle = Console.ReadLine();
                selectedGame = gameList.Find(g => g.Title.Equals(gameTitle));
            }
            if(selectedGame == null){
                Console.WriteLine("Juego inválido, retorno al menú");
            }
            return selectedGame;
        }
    }
}