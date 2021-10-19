using System.Linq;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using DisplayUtils;
using Domain;
using ProtocolLibrary;
using ConnectionUtils;
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
                try
                {
                    string userName = DialogUtils.Login();

                    SendData(CommandConstants.Login, userName);

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

            var userJson = Utils.ClientReceiveMessageData(networkStream);
            myUser = new User(userJson);
        }

        public void Logout()
        {
            SendData(CommandConstants.Logout, myUser.Encode());
        }

        public void AcquireGame()
        {
            Game game = DialogUtils.SelectGame(GetGames());
            if (game == null)
            {
                Console.WriteLine("Retorno al menú.");
                return;
            }

            SendData(CommandConstants.AcquireGame, new UserGamePair(myUser, game).Encode());

            Console.WriteLine(Utils.ClientReceiveMessageData(networkStream));
        }

        public List<Game> GetGames()
        {
            SendData(CommandConstants.GetGames, "");

            var gamesJson = Utils.ClientReceiveMessageData(networkStream);
            List<Game> gameList = GameSystem.DecodeGames(gamesJson);
            return gameList;
        }

        public async void ShowGamesAndDetail(List<Game> games)
        {
            Game gameToShow = DialogUtils.SelectGame(games);

            if (gameToShow != null && !String.IsNullOrEmpty(gameToShow.Cover))
            {
                await ReciveGameCover(gameToShow);
            }

            DialogUtils.ShowGameDetail(gameToShow);
        }

        public async Task ReciveGameCover(Game g)
        {
            SendData(CommandConstants.GetGameCover, g.Encode());

            await ReciveFile();
        }

        public List<User> GetUsers()
        {
            SendData(CommandConstants.GetUsers, "");

            var usersJson = Utils.ClientReceiveMessageData(networkStream);
            List<User> users = GameSystem.DecodeUsers(usersJson);

            return users;
        }

        public List<Game> GetAcquiredGames()
        {
            SendData(CommandConstants.GetAcquiredGames, myUser.Encode());

            var gamesJson = Utils.ClientReceiveMessageData(networkStream);

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

        public async void PublishGame()
        {
            Game gameToPublish = DialogUtils.InputGame();

            SendData(CommandConstants.PublishGame, gameToPublish.Encode());

            if (File.Exists(gameToPublish.Cover))
            {
                await SendFile(gameToPublish.Cover);
            }

            Console.WriteLine(Utils.ClientReceiveMessageData(networkStream));
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

            if (review == null)
            {
                Console.WriteLine("Retorno al menú.");
                return;
            }

            game.AddReview(review);

            SendData(CommandConstants.PublishReview, game.Encode());

            Console.WriteLine(Utils.ClientReceiveMessageData(networkStream));
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

            SendData(CommandConstants.ModifyingGame, gameToModify.Encode());

            List<string> headerAndMessage = Utils.ReceiveCommandAndMessage(networkStream);

            Console.WriteLine(headerAndMessage[1]);

            if (headerAndMessage[0] == CommandConstants.ModifyingGameError.ToString())
            {
                Console.WriteLine("Retorno al menú.");
                return;
            }

            Console.WriteLine("Ingrese los nuevos datos del juego. Si no quiere modificar el campo, presione ENTER.");
            Game modifiedGame = DialogUtils.InputGame();

            SendData(CommandConstants.ModifyGame, GameSystem.EncodeGames(new List<Game>() { gameToModify, modifiedGame }));

            if (File.Exists(modifiedGame.Cover))
            {
                SendFile(modifiedGame.Cover);
            }

            Console.WriteLine(Utils.ClientReceiveMessageData(networkStream));
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

            SendData(CommandConstants.DeleteGame, gameToDelete.Encode());

            Console.WriteLine(Utils.ClientReceiveMessageData(networkStream));
        }

        public void SendData(int command, string message)
        {
            var header = new Header(HeaderConstants.Request, command, message.Length);
            Utils.ClientSendData(networkStream, header, message);
        }
    }
}