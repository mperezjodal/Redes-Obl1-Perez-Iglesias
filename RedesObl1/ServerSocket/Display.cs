using System;
using System.Collections.Generic;
using Domain;

namespace ServerSocket
{
    public class Display
    {
        public static void ServerMenu() {
            Console.WriteLine();
            Console.WriteLine(@"###############################################");
            Console.WriteLine(@"#                                             #");
            Console.WriteLine(@"#       |¯\ /¯| | ____| |¯\ |¯| | | | |       #");
            Console.WriteLine(@"#       |  ¯  | | __|   |  \| | | |_| |       #");
            Console.WriteLine(@"#       |     | |_____| | \   | |_____|       #");
            Console.WriteLine(@"#                                             #");
            Console.WriteLine(@"#   Seleccione una opción:                    #");
            Console.WriteLine(@"#                                             #");
            Console.WriteLine(@"#   1-   Ver catálogo de juegos               #");
            Console.WriteLine(@"#   2-   Adquirir juego                       #");
            Console.WriteLine(@"#   3-   Publicar juego                       #");
            Console.WriteLine(@"#   4-   Publicar calificación de un juego    #");
            Console.WriteLine(@"#   5-   Buscar juegos                        #");
            Console.WriteLine(@"#        exit                                 #");
            Console.WriteLine(@"#                                             #");
            Console.WriteLine(@"###############################################");
            Console.WriteLine();
        }

        public static void GameList(List<Game> games){
            Console.WriteLine("Lista de juegos:");
            foreach(Game g in games){
                Console.WriteLine(g.Title);
            }
        }
    }
}