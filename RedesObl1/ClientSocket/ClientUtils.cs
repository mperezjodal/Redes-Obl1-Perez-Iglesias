using System;
using System.Collections.Generic;
using System.Net.Sockets;
using DisplayUtils;
using Domain;
using ProtocolLibrary;
using SocketUtils;

namespace ClientSocket
{
    public class ClientUtils
    {
        private Socket clientSocket;
        private User myUser;
        public ClientUtils(Socket clientSocket) 
        {
            this.clientSocket = clientSocket;
        }

        public void Login()
        {
            string userName = DialogUtils.Login();
            var header = new Header(HeaderConstants.Request, CommandConstants.Login, userName.Length);
            Utils.SendData(clientSocket, header, userName);

            Console.WriteLine(Utils.ReciveMessageData(clientSocket));

            var userJson = Utils.ReciveMessageData(clientSocket);
            myUser = User.Decode(userJson);
        }

        public void AdquireGame()
        {
            Game game = DialogUtils.SelectGame(GetGames());
            if(game == null){
                Console.WriteLine("Retorno al menú.");
                return ;
            }

            var message = new UserGamePair(myUser, game).Encode();
            var header = new Header(HeaderConstants.Request, CommandConstants.AdquireGame, message.Length);
            Utils.SendData(clientSocket, header, message);

            Console.WriteLine(Utils.ReciveMessageData(clientSocket));
        }

        public List<Game> GetGames(){
            var headerRequestGameList = new Header(HeaderConstants.Request, CommandConstants.GetGames, 0);
            Utils.SendData(clientSocket, headerRequestGameList, "");

            var gamesJson = Utils.ReciveMessageData(clientSocket);
            
            return GameSystem.DecodeGames(gamesJson);
        }

        public List<Game> GetAdquiredGames(){
            var message = myUser.Encode();
            var headerRequestGameList = new Header(HeaderConstants.Request, CommandConstants.GetAdquiredGames, message.Length);
            Utils.SendData(clientSocket, headerRequestGameList, message);

            var gamesJson = Utils.ReciveMessageData(clientSocket);

            return GameSystem.DecodeGames(gamesJson);
        }

        public void PublishGame(){
            Game gameToPublish = DialogUtils.InputGame();

            var message = gameToPublish.Encode();
            var header = new Header(HeaderConstants.Request, CommandConstants.PublishGame, message.Length);
            Utils.SendData(clientSocket, header, message);
            
            Console.WriteLine(Utils.ReciveMessageData(clientSocket));
        }

        public void PublishReview(){
            Game game = DialogUtils.SelectGame(GetGames());
            if(game == null){
                Console.WriteLine("Retorno al menú.");
                return ;
            }

            Review review = DialogUtils.InputReview();
            game.AddReview(review);

            var message = game.Encode();
            var header = new Header(HeaderConstants.Request, CommandConstants.PublishReview, message.Length);
            Utils.SendData(clientSocket, header, message);
            
            Console.WriteLine(Utils.ReciveMessageData(clientSocket));
        }

        public void ModifyGame() {
            List<Game> games = GetGames();
            
            Game gameToModify = DialogUtils.SelectGame(games);
            if(gameToModify == null){
                Console.WriteLine("Retorno al menú.");
                return;
            }
            
            Console.WriteLine("Ingrese los nuevos datos del juego. Si no quiere modificar el campo, presione ENTER.");
            Game modifiedGame = DialogUtils.InputGame();

            var modifyGameMessage = GameSystem.EncodeGames(new List<Game>() {gameToModify, modifiedGame});
            var modifyGameHeader = new Header(HeaderConstants.Request, CommandConstants.ModifyGame, modifyGameMessage.Length);
            Utils.SendData(clientSocket, modifyGameHeader, modifyGameMessage);

            Console.WriteLine(Utils.ReciveMessageData(clientSocket));
        }

        public void DeleteGame() {
            List<Game> games = GetGames();

            Game gameToDelete = DialogUtils.SelectGame(games);
            if(gameToDelete == null){
                Console.WriteLine("Retorno al menú.");
                return ;
            }

            var deleteGameMessage = gameToDelete.Encode();
            var deleteGameHeader = new Header(HeaderConstants.Request, CommandConstants.DeleteGame, deleteGameMessage.Length);
            Utils.SendData(clientSocket, deleteGameHeader, deleteGameMessage);

            Console.WriteLine(Utils.ReciveMessageData(clientSocket));
        }
    }
}