using System;
using System.Collections.Generic;
using Domain;
using System.IO;

namespace DisplayUtils
{
    public class DialogUtils
    {
        public static string Menu(Dictionary<string, string> items)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Console.WriteLine();
            Console.ResetColor();
            DialogUtils.MenuTitle();
            Console.WriteLine();
            Console.WriteLine("    Seleccione una opción:");
            Console.WriteLine();
            foreach (var menuOption in items)
            {
                Console.WriteLine("    " + menuOption.Key + " -   " + menuOption.Value);
            }
            Console.WriteLine("          exit");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            Console.ResetColor();
            Console.WriteLine();
            var option = Console.ReadLine();
            if (items.ContainsKey(option))
            {
                Console.WriteLine("Has seleccionado: " + items[option]);
            }
            return option;
        }

        public static void MenuTitle()
        {
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

        public static string Login()
        {
            Console.WriteLine("Ingrese un nombre de usuario:");
            return Console.ReadLine();
        }
        public static void ReturnToMenu()
        {
            Console.WriteLine();
            Console.WriteLine("ENTER para volver al menú.");
            Console.ReadLine();
        }

        public static void GameList(List<Game> games)
        {
            if (games.Count == 0)
            {
                Console.WriteLine("No se han encontrado juegos.");
            }
            else
            {
                Console.WriteLine("Lista de juegos:");
                foreach (Game g in games)
                {
                    Console.WriteLine(g.Title);
                }
            }
        }

        public static void UserList(List<User> users)
        {
            if (users.Count == 0)
            {
                Console.WriteLine("No se han encontrado usuarios.");
            }
            else
            {
                Console.WriteLine("Lista de usuarios:");
                foreach (User user in users)
                {
                    Console.WriteLine(user.Name);
                }
            }
        }

        public static Game InputGame()
        {
            Game newGame = new Game();

            Console.WriteLine("Título:");
            newGame.Title = Console.ReadLine();
            Console.WriteLine("Género:");
            newGame.Genre = Console.ReadLine();
            Console.WriteLine("Sinópsis:");
            newGame.Synopsis = Console.ReadLine();

            Console.WriteLine("Path de la carátula:");
            String path = @"" + Console.ReadLine();
            if (File.Exists(path))
            {
                newGame.Cover = path;
            }
            else
            {
                Console.WriteLine("Archivo no disponible.");
            }

            newGame.Reviews = new List<Review>();
            return newGame;
        }

        public static Review InputReview()
        {
            Review Review = new Review();

            Console.WriteLine("Rating (entre 1 y 10):");
            var rating = Console.ReadLine();
            int rat;
            Int32.TryParse(rating, out rat);

            if (rat > 10 || rat < 1)
            {
                Console.WriteLine("Rating inválido.");
                return null;
            }

            Review.Rating = rat;
            Console.WriteLine("Comentario:");
            Review.Comment = Console.ReadLine();

            return Review;
        }

        public static User InputUser(List<User> users)
        {
            User user = new User();

            Console.WriteLine("Nombre:");
            var name = Console.ReadLine();

            User existingUser = users.Find(u => u.Name.Equals(name));
            if (existingUser != null)
            {
                Console.WriteLine("Ya existe un usuario con este nombre, ingrese otro porfavor");
                Console.WriteLine("Nombre:");
                name = Console.ReadLine();

                existingUser = users.Find(u => u.Name.Equals(name));
                if (existingUser != null)
                {
                    Console.WriteLine("Nombre invalido.");
                    return null;
                }
            }
            user.Name = name;
            return user;
        }

        public static Game SelectGame(List<Game> games)
        {
            DialogUtils.GameList(games);

            Game selectedGame = null;
            if (games.Count > 0)
            {
                Console.WriteLine("Ingrese el título del juego:");
                string gameTitle = Console.ReadLine();
                selectedGame = games.Find(g => g.Title.Equals(gameTitle));
                if (selectedGame == null)
                {
                    Console.WriteLine("Juego inválido, ingrese el título nuevamente:");
                    gameTitle = Console.ReadLine();
                    selectedGame = games.Find(g => g.Title.Equals(gameTitle));
                }
                if (selectedGame == null)
                {
                    Console.WriteLine("Juego inválido.");
                }
            }
            return selectedGame;
        }

        public static User SelectUser(List<User> users)
        {
            DialogUtils.UserList(users);

            User selectedUser = null;
            if (users.Count > 0)
            {
                Console.WriteLine("Ingrese el nombre del usuario:");
                string selectedName = Console.ReadLine();
                selectedUser = users.Find(u => u.Name.Equals(selectedName));
                if (selectedUser == null)
                {
                    Console.WriteLine("Nombre de usuario inválido, intente nuevamente:");
                    selectedName = Console.ReadLine();
                    selectedUser = users.Find(u => u.Name.Equals(selectedName));
                }
                if (selectedUser == null)
                {
                    Console.WriteLine("Juego inválido.");
                }
            }
            return selectedUser;
        }

        public static void SearchFilteredGames(List<Game> games)
        {
            Console.WriteLine();
            Console.WriteLine("Seleccione una opción para filtrar juegos:");
            Console.WriteLine();
            Console.WriteLine("    1-   Categoría");
            Console.WriteLine("    2-   Título");
            Console.WriteLine("    3-   Rating");
            Console.WriteLine();
            var filter = Console.ReadLine();
            List<Game> filteredGames = new List<Game>();
            switch (filter)
            {
                case "1":
                    Console.WriteLine("Ingrese categoría:");
                    var cat = Console.ReadLine();
                    filteredGames = games.FindAll(g => g.Genre.Equals(cat));
                    DialogUtils.GameList(filteredGames);
                    break;
                case "2":
                    Console.WriteLine("Ingrese título:");
                    var title = Console.ReadLine();
                    filteredGames = games.FindAll(g => g.Title.Equals(title));
                    DialogUtils.GameList(filteredGames);
                    break;
                case "3":
                    Console.WriteLine("Ingrese rating:");
                    string rating = Console.ReadLine();
                    filteredGames = games.FindAll(g => g.Rating.ToString().Equals(rating));
                    DialogUtils.GameList(filteredGames);
                    break;
                default:
                    Console.WriteLine("Opción inválida.");
                    break;
            }
        }

        public static void ShowGameDetail(List<Game> games)
        {
            Game gameToShow = SelectGame(games);
            if (gameToShow == null)
            {
                Console.WriteLine("Retorno al menú.");
                return;
            }

            Console.WriteLine("Detalle del juego: ");
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

            if (gameToShow.Reviews.Count > 0)
            {
                int totalRating = 0;
                int cont = 0;

                foreach (Review r in gameToShow.Reviews)
                {
                    cont++;
                    totalRating += r.Rating;
                    Console.WriteLine("Review " + cont);

                    Console.Write("    Rating: ");

                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine(r.Rating);
                    Console.ResetColor();

                    Console.Write("    Comentario: ");

                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine(r.Comment);
                    Console.ResetColor();
                }

                Console.Write("Promedio de Calificaciones: ");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine(totalRating / cont);
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("No hay reviews para este juego en el sistema.");
            }

            if (File.Exists(gameToShow.Cover))
            {
                Console.Write("Carátula: ");
                Console.WriteLine("Se encuentra en: " + gameToShow.Cover);
            }
            else
            {
                Console.Write("No hay carátula para este juego.");
            }

        }

        public static void ShowGameDetail(Game gameToShow)
        {
            if (gameToShow == null)
            {
                Console.WriteLine("Retorno al menú.");
                return;
            }

            Console.WriteLine("Detalle del juego: ");
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

            if (gameToShow.Reviews.Count > 0)
            {
                int totalRating = 0;
                int cont = 0;

                foreach (Review r in gameToShow.Reviews)
                {
                    cont++;
                    totalRating += r.Rating;
                    Console.WriteLine("Review " + cont);

                    Console.Write("    Rating: ");

                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine(r.Rating);
                    Console.ResetColor();

                    Console.Write("    Comentario: ");

                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine(r.Comment);
                    Console.ResetColor();
                }

                Console.Write("Promedio de Calificaciones: ");
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine(totalRating / cont);
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("No hay reviews para este juego en el sistema.");
            }

            if (File.Exists(gameToShow.Cover))
            {
                Console.Write("Carátula: ");
                Console.WriteLine("Se encuentra en: " + gameToShow.Cover);
            }
            else
            {
                Console.Write("No hay carátula para este juego.");
            }

        }
    }
}
