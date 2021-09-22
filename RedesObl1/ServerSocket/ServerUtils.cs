using System.Security.AccessControl;
using System.Globalization;
using System;
using System.ComponentModel.Design;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SocketUtils;
using Domain;
using ProtocolLibrary;
using DisplayUtils;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace ServerSocket
{
    public class ServerUtils
    {   
        public Socket serverSocket;
        public GameSystem GameSystem;
        public ServerUtils(Socket serverSocket, GameSystem gameSystem) 
        {
            this.serverSocket = serverSocket;
            this.GameSystem = gameSystem;
        }

        public void GetAdquiredGamesManager(Socket clientSocket, string jsonUser)
        {
            try
            {
                User user = User.Decode(jsonUser);
                var systemUser = GameSystem.Users.Find(u => u.Name.Equals(user.Name));

                var gamesMessage = systemUser.EncodeGames();
                var adquireGameHeader = new Header(HeaderConstants.Response, CommandConstants.GetAdquiredGamesOk, gamesMessage.Length);
                Utils.SendData(clientSocket, adquireGameHeader, gamesMessage);
            }
            catch (Exception){
                var adquireGameMessage = "[]";
                var adquireGameHeader = new Header(HeaderConstants.Response, CommandConstants.GetAdquiredGamesError, adquireGameMessage.Length);
                Utils.SendData(clientSocket, adquireGameHeader, adquireGameMessage);
            }
        }

        public void AdquireGameManager(Socket clientSocket, string jsonAdquireGameData)
        {
            try
            {
                UserGamePair userGame = UserGamePair.Decode(jsonAdquireGameData);
                var user = GameSystem.Users.Find(u => u.Name.Equals(userGame.User.Name));
                var game = GameSystem.Games.Find(g => g.Title.Equals(userGame.Game.Title));
                user.AquireGame(game);
                
                var adquireGameMessage = "Se ha adquirido el juego: " + game.Title + ".";
                var adquireGameHeader = new Header(HeaderConstants.Response, CommandConstants.AdquireGameOk, adquireGameMessage.Length);
                Utils.SendData(clientSocket, adquireGameHeader, adquireGameMessage);
            }
            catch (Exception){
                var adquireGameMessage = "No se ha podido adquirir el juego.";
                var adquireGameHeader = new Header(HeaderConstants.Response, CommandConstants.AdquireGameError, adquireGameMessage.Length);
                Utils.SendData(clientSocket, adquireGameHeader, adquireGameMessage);
            }
        }

        public void LoginManager(Socket clientSocket, string userName){
            User newUser = GameSystem.AddUser(userName);

            var loginMessage = "Se ha creado el usuario: " + userName + ".";
            var loginHeader = new Header(HeaderConstants.Response, CommandConstants.LoginOk, loginMessage.Length);
            Utils.SendData(clientSocket, loginHeader, loginMessage);

            var userMessage = newUser.Encode();
            var userHeader = new Header(HeaderConstants.Response, CommandConstants.NewUser, userMessage.Length);
            Utils.SendData(clientSocket, userHeader, userMessage);
        }

        public void ModifyGameManager(Socket clientSocket, string jsonModifyGameData){
            try
            {
                List<Game> updatingGames = GameSystem.DecodeGames(jsonModifyGameData);
                var gameToModify = GameSystem.Games.Find(g => g.Title.Equals(updatingGames[0].Title));
                gameToModify.Update(updatingGames[1]);
                
                var modifyGameMessage = "Se ha modificado el juego: " + gameToModify.Title + ".";
                var modifyGameHeader = new Header(HeaderConstants.Response, CommandConstants.ModifyGameOk, modifyGameMessage.Length);
                Utils.SendData(clientSocket, modifyGameHeader, modifyGameMessage);
            }
            catch (Exception){
                var modifyGameMessage = "No se ha podido modificar el juego.";
                var modifyGameHeader = new Header(HeaderConstants.Response, CommandConstants.ModifyGameError, modifyGameMessage.Length);
                Utils.SendData(clientSocket, modifyGameHeader, modifyGameMessage);
            }
        }

        public void DeleteGameManager(Socket clientSocket, string jsonDeleteGameData){
            try
            {
                Game gameToDelete = Game.Decode(jsonDeleteGameData);
                this.GameSystem.DeleteGame(gameToDelete);
                
                var deleteGameMessage = "Se ha eliminado el juego: " + gameToDelete.Title + ".";
                var deleteGameHeader = new Header(HeaderConstants.Response, CommandConstants.DeleteGameOk, deleteGameMessage.Length);
                Utils.SendData(clientSocket, deleteGameHeader, deleteGameMessage);
            }
            catch (Exception){
                var deleteGameMessage = "No se ha podido eliminar el juego.";
                var deleteGameHeader = new Header(HeaderConstants.Response, CommandConstants.DeleteGameError, deleteGameMessage.Length);
                Utils.SendData(clientSocket, deleteGameHeader, deleteGameMessage);
            }
        }
        public void PublishReviewManager(Socket clientSocket, string jsonPublishReviewData){
            try
            {
                Game publishReviewGame = Game.Decode(jsonPublishReviewData);
                var gameToModify = GameSystem.Games.Find(g => g.Title.Equals(publishReviewGame.Title));
                gameToModify.UpdateReviews(publishReviewGame.Reviews);
                
                var publishReviewMessage = "Se ha publicado la calificacion para el juego: " + gameToModify.Title + ".";
                var publishReviewHeader = new Header(HeaderConstants.Response, CommandConstants.PublishReviewOk, publishReviewMessage.Length);
                Utils.SendData(clientSocket, publishReviewHeader, publishReviewMessage);
            }
            catch (Exception){
                var publishReviewMessage = "No se ha podido publicar la calificacion del juego.";
                var publishReviewHeader = new Header(HeaderConstants.Response, CommandConstants.PublishReviewError, publishReviewMessage.Length);
                Utils.SendData(clientSocket, publishReviewHeader, publishReviewMessage);
            }
        }

        public void PublishGameManager(Socket clientSocket, string jsonPublishGame){
            Game newGame = Game.Decode(jsonPublishGame);
            GameSystem.AddGame(newGame);

            var publishedGameMessage = "Se ha publicado el juego: " + newGame.Title + ".";
            var publishedGameHeader = new Header(HeaderConstants.Response, CommandConstants.PublishGameOk, publishedGameMessage.Length);
            Utils.SendData(clientSocket, publishedGameHeader, publishedGameMessage);
        }
    }
}