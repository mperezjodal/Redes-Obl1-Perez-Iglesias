using System.Linq;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using DisplayUtils;
using Domain;
using ProtocolLibrary;
using SocketUtils;
using System.IO;
using FileStreamLibrary;
using System.Text;
using System.Threading.Tasks;

namespace networkStream
{
    public class ClientUtils
    {
        private NetworkStream networkStream;
        private User myUser;
        public ClientUtils(NetworkStream networkStream)
        {
            this.networkStream = networkStream;
        }

        public void Login()
        {
            var notLogin = true;

            while (notLogin)
            {
                try{
                    string userName = DialogUtils.Login();
                    var headerLoginRequest = new Header(HeaderConstants.Request, CommandConstants.Login, userName.Length);
                    Utils.SendData(networkStream, headerLoginRequest, userName);

                    List<string> commandAndMessage = Utils.ReceiveCommandAndMessage(networkStream);

                    Console.WriteLine(commandAndMessage[1]);

                    if (commandAndMessage[0] == CommandConstants.LoginOk.ToString())
                    {
                        notLogin = false;
                    }
                }
                catch (SocketException)
                {
                    notLogin = false;
                    
                }
                catch (Exception)
                {
                    notLogin = false;
                }
            }

            var userJson = Utils.ReceiveMessageData(networkStream);
            myUser = User.Decode(userJson);
        }

        public void Logout()
        {
            var message = myUser.Encode();
            var header = new Header(HeaderConstants.Request, CommandConstants.Logout, message.Length);
            Utils.SendData(networkStream, header, message);
        }

        public void AcquireGame()
        {
            Game game = DialogUtils.SelectGame(GetGames());
            if (game == null)
            {
                Console.WriteLine("Retorno al menú.");
                return;
            }

            var message = new UserGamePair(myUser, game).Encode();
            var header = new Header(HeaderConstants.Request, CommandConstants.AcquireGame, message.Length);
            Utils.SendData(networkStream, header, message);

            Console.WriteLine(Utils.ReceiveMessageData(networkStream));
        }

        public List<Game> GetGames()
        {
            var headerRequestGameList = new Header(HeaderConstants.Request, CommandConstants.GetGames, 0);
            Utils.SendData(networkStream, headerRequestGameList, "");
            var gamesJson = Utils.ReceiveMessageData(networkStream);
            List<Game> gameList = GameSystem.DecodeGames(gamesJson);
            return gameList;
        }

        public void ReciveGameCover(Game g)
        {
            var headerRequestGameCover = new Header(HeaderConstants.Request, CommandConstants.GetGameCover, 0);
            Utils.SendData(networkStream, headerRequestGameCover, "");
            ReciveFile();
        }

        public List<User> GetUsers()
        {
            var headerRequestUsersList = new Header(HeaderConstants.Request, CommandConstants.GetUsers, 0);
            Utils.SendData(networkStream, headerRequestUsersList, "");

            var usersJson = Utils.ReceiveMessageData(networkStream);
            List<User> users = GameSystem.DecodeUsers(usersJson);

            return users;
        }

        public List<Game> GetAcquiredGames()
        {
            var message = myUser.Encode();
            var headerRequestGameList = new Header(HeaderConstants.Request, CommandConstants.GetAcquiredGames, message.Length);
            Utils.SendData(networkStream, headerRequestGameList, message);

            var gamesJson = Utils.ReceiveMessageData(networkStream);

            return GameSystem.DecodeGames(gamesJson);
        }

        public async Task SendFile(string path)
        {
            var fileCommunication = new FileCommunicationHandler(this.networkStream);
            await fileCommunication.SendFileAsync(path);
        }

        public async Task ReciveFile()
        {
            var fileCommunicationGameList = new FileCommunicationHandler(this.networkStream);
            await fileCommunicationGameList.ReceiveFileAsync();
        }

        public void PublishGame()
        {
            Game gameToPublish = DialogUtils.InputGame();

            var message = gameToPublish.Encode();
            var header = new Header(HeaderConstants.Request, CommandConstants.PublishGame, message.Length);

            Utils.SendData(networkStream, header, message);

            if (File.Exists(gameToPublish.Cover))
            {
                SendFile(gameToPublish.Cover);
            }

            Console.WriteLine(Utils.ReceiveMessageData(networkStream));
        }

        public void PublishReview()
        {
            Game game = DialogUtils.SelectGame(GetGames());
            if (game == null)
            {
                Console.WriteLine("Retorno al menú.");
                return;
            }

            Review review = DialogUtils.InputReview();

            if(review == null)
            {
                Console.WriteLine("Retorno al menú.");
                return;
            }

            game.AddReview(review);

            var message = game.Encode();
            var header = new Header(HeaderConstants.Request, CommandConstants.PublishReview, message.Length);
            Utils.SendData(networkStream, header, message);

            Console.WriteLine(Utils.ReceiveMessageData(networkStream));
        }

        public void ModifyGame()
        {
            List<Game> games = GetGames();
            Game gameToModify = DialogUtils.SelectGame(games);

            if (gameToModify == null)
            {
                Console.WriteLine("Retorno al menú.");
                return;
            }

            var modifyingGameMessage = gameToModify.Encode();
            var modifyingGameHeader = new Header(HeaderConstants.Request, CommandConstants.ModifyingGame, modifyingGameMessage.Length);
            Utils.SendData(networkStream, modifyingGameHeader, modifyingGameMessage);

            List<string> headerAndMessage = Utils.ReceiveCommandAndMessage(networkStream);

            Console.WriteLine(headerAndMessage[1]);

            if (headerAndMessage[0] == CommandConstants.ModifyingGameError.ToString())
            {
                Console.WriteLine("Retorno al menú.");
                return;
            }
            
            Console.WriteLine("Ingrese los nuevos datos del juego. Si no quiere modificar el campo, presione ENTER.");
            Game modifiedGame = DialogUtils.InputGame();

            var modifyGameMessage = GameSystem.EncodeGames(new List<Game>() { gameToModify, modifiedGame });
            var modifyGameHeader = new Header(HeaderConstants.Request, CommandConstants.ModifyGame, modifyGameMessage.Length);
            Utils.SendData(networkStream, modifyGameHeader, modifyGameMessage);

            if (File.Exists(modifiedGame.Cover))
            {
                SendFile(modifiedGame.Cover);
            }

            Console.WriteLine(Utils.ReceiveMessageData(networkStream));
        }

        public void DeleteGame()
        {
            List<Game> games = GetGames();

            Game gameToDelete = DialogUtils.SelectGame(games);
            if (gameToDelete == null)
            {
                Console.WriteLine("Retorno al menú.");
                return;
            }

            var deleteGameMessage = gameToDelete.Encode();
            var deleteGameHeader = new Header(HeaderConstants.Request, CommandConstants.DeleteGame, deleteGameMessage.Length);
            Utils.SendData(networkStream, deleteGameHeader, deleteGameMessage);

            Console.WriteLine(Utils.ReceiveMessageData(networkStream));
        }
    }
}