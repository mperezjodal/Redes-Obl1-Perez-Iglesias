using System;
using System.IO;
using System.Linq;
using DisplayUtils;
using Domain;

namespace ServerSocket
{
    public class ServerMenuUtils
    {
        private GameSystem gameSystem;
        public ServerMenuUtils(GameSystem gs)
        {
            gameSystem = gs;
        }

        public void InsertGame()
        {
            Game gameToPublish = DialogUtils.InputGame();
            try
            {
                if (gameToPublish.Cover != null)
                {
                    var fileName = gameToPublish.Cover.Split("/").Last();
                    System.IO.File.Copy(gameToPublish.Cover, Directory.GetCurrentDirectory().ToString() + "/" + fileName);
                    gameToPublish.Cover = fileName;
                }

                gameSystem.AddGame(gameToPublish);
                Console.WriteLine("Se ha publicado el juego: " + gameToPublish.Title + ".");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        public void InsertReview()
        {
            Game selectedGame = DialogUtils.SelectGame(gameSystem.Games);
            if (selectedGame == null)
            {
                return;
            }
            if (gameSystem.IsGameBeingModified(selectedGame))
            {
                Console.WriteLine("No se puede publicar una califiación de este juego.");
                return;
            }

            Review selectedGameReview = DialogUtils.InputReview();

            if (selectedGameReview == null)
            {
                return;
            }
            if (gameSystem.IsGameBeingModified(selectedGame) || !gameSystem.GameExists(selectedGame))
            {
                Console.WriteLine("No se puede publicar una califiación de este juego.");
                return;
            }
            try
            {
                selectedGame.AddReview(selectedGameReview);
                Console.WriteLine("Se ha publicado la calificación del juego " + selectedGame.Title + ".");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void InsertUser()
        {
            try
            {
                User userToInsert = DialogUtils.InputUser(gameSystem.Users);
                userToInsert.Login = false;
                if (userToInsert == null)
                {
                    Console.WriteLine("No se puede insertar este usuario.");
                    return;
                }
                else
                {
                    gameSystem.AddUser(userToInsert.Name);
                    Console.WriteLine("Se ha insertado el usuario: " + userToInsert.Name + ".");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void ModifyUser()
        {
            try
            {
                User userToModify = DialogUtils.SelectUser(gameSystem.Users);

                if (userToModify == null)
                {
                    Console.WriteLine("Retorno al menú.");
                    return;
                }

                Console.WriteLine("Ingrese el nuevo nombre de usuario:");
                User modifiedUser = DialogUtils.InputUser(gameSystem.Users);

                if (modifiedUser == null)
                {
                    Console.WriteLine("Retorno al menú.");
                    return;
                }
                gameSystem.UpdateUser(userToModify, modifiedUser);
                Console.WriteLine("Se ha modificado el usuario: " + modifiedUser.Name + ".");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void DeleteUser()
        {
            try
            {
                User userToDelete = DialogUtils.SelectUser(gameSystem.Users);
                if (userToDelete == null)
                {
                    Console.WriteLine("Retorno al menú.");
                    return;
                }

                gameSystem.Users.RemoveAll(u => u.Name.Equals(userToDelete.Name));
                Console.WriteLine("Se ha eliminado el usuario: " + userToDelete.Name + ".");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}