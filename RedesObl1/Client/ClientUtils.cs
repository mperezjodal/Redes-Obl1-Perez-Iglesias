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

        public async Task Login()
        {
            var notLogin = true;

            while (notLogin)
            {
                try
                {
                    string userName = DialogUtils.Login();
                    if (userName != "")
                    {
                        await SendData(CommandConstants.Login, userName);
                        List<string> commandAndMessage = await Utils.ReceiveCommandAndMessage(networkStream);

                        Console.WriteLine(commandAndMessage[1]);

                        if (commandAndMessage[0] == CommandConstants.LoginOk.ToString())
                        {
                            notLogin = false;
                        }
                    }
                    else
                    {
                        Console.WriteLine("El nombre del usuario no puede ser vacío.");
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
            var userJson = await Utils.ClientReceiveMessageData(networkStream);

            myUser = new User(userJson);
        }

        public async Task Logout()
        {
            await SendData(CommandConstants.Logout, myUser.Encode());
        }

        public async Task AcquireGame()
        {
            Game game = DialogUtils.SelectGame(await GetGames());
            if (game == null)
            {
                Console.WriteLine("Retorno al menú.");
                return;
            }

            await SendData(CommandConstants.AcquireGame, game.Encode());

            Console.WriteLine(await Utils.ClientReceiveMessageData(networkStream));
        }

        public async Task<List<Game>> GetGames()
        {
            await SendData(CommandConstants.GetGames, "");

            var gamesJson = await Utils.ClientReceiveMessageData(networkStream);
            List<Game> gameList = GameSystem.DecodeGames(gamesJson);
            return gameList;
        }

        public async Task ShowGamesAndDetail(List<Game> games)
        {
            Game gameToShow = DialogUtils.SelectGame(games);

            if (gameToShow != null && !String.IsNullOrEmpty(gameToShow.Cover))
            {
                await ReceiveGameCover(gameToShow);
            }

            DialogUtils.ShowGameDetail(gameToShow);
        }

        public async Task ReceiveGameCover(Game g)
        {
            await SendData(CommandConstants.GetGameCover, g.Encode());
            
            await ReceiveFile();
        }

        public async Task<List<Game>> GetAcquiredGames()
        {
            await SendData(CommandConstants.GetAcquiredGames, "");

            var gamesJson = await Utils.ClientReceiveMessageData(networkStream);

            return GameSystem.DecodeGames(gamesJson);
        }

        public async Task SendFile(string path)
        {
            var fileCommunication = new FileCommunicationHandler(this.networkStream);
            await fileCommunication.SendFileAsync(path);
        }

        public async Task ReceiveFile()
        {
            var fileCommunicationGameList = new FileCommunicationHandler(this.networkStream);
            await fileCommunicationGameList.ReceiveFileAsync();
        }

        public async Task PublishGame()
        {
            Game gameToPublish = DialogUtils.InputGame();

            await SendData(CommandConstants.PublishGame, gameToPublish.Encode());

            if (File.Exists(gameToPublish.Cover))
            {
                await SendFile(gameToPublish.Cover);
            }

            Console.WriteLine(await Utils.ClientReceiveMessageData(networkStream));
        }

        public async Task PublishReview()
        {
            Game game = DialogUtils.SelectGame(await GetGames());
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

            await SendData(CommandConstants.PublishReview, game.Encode());

            Console.WriteLine(await Utils.ClientReceiveMessageData(networkStream));
        }

        public async Task ModifyGame()
        {
            List<Game> games = await GetGames();
            Game gameToModify = DialogUtils.SelectGame(games);

            if (gameToModify == null)
            {
                Console.WriteLine("Retorno al menú.");
                return;
            }

            await SendData(CommandConstants.ModifyingGame, gameToModify.Encode());

            List<string> headerAndMessage = await Utils.ReceiveCommandAndMessage(networkStream);

            Console.WriteLine(headerAndMessage[1]);

            if (headerAndMessage[0] == CommandConstants.ModifyingGameError.ToString())
            {
                Console.WriteLine("Retorno al menú.");
                return;
            }

            Console.WriteLine("Ingrese los nuevos datos del juego. Si no quiere modificar el campo, presione ENTER.");
            Game modifiedGame = DialogUtils.InputGame();

            await SendData(CommandConstants.ModifyGame, GameSystem.EncodeGames(new List<Game>() { gameToModify, modifiedGame }));

            if (File.Exists(modifiedGame.Cover))
            {
                await SendFile(modifiedGame.Cover);
            }

            Console.WriteLine(await Utils.ClientReceiveMessageData(networkStream));
        }

        public async Task DeleteGame()
        {
            List<Game> games = await GetGames();

            Game gameToDelete = DialogUtils.SelectGame(games);
            if (gameToDelete == null)
            {
                Console.WriteLine("Retorno al menú.");
                return;
            }

            await SendData(CommandConstants.DeleteGame, gameToDelete.Encode());

            Console.WriteLine(await Utils.ClientReceiveMessageData(networkStream));
        }

        public async Task SendData(int command, string message)
        {
            var header = new Header(HeaderConstants.Request, command, message.Length);
            await Utils.ClientSendData(networkStream, header, message);
        }
    }
}