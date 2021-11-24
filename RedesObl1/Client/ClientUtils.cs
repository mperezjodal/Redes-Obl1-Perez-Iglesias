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

        public async Task LoginAsync()
        {
            var notLogin = true;

            while (notLogin)
            {
                try
                {
                    string userName = DialogUtils.Login();
                    if (userName != "")
                    {
                        await SendDataAsync(CommandConstants.Login, userName);
                        List<string> commandAndMessage = await Utils.ReceiveCommandAndMessageAsync(networkStream);

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
            var userJson = await Utils.ClientReceiveMessageDataAsync(networkStream);

            myUser = new User(userJson);
        }

        public async Task LogoutAsync()
        {
            await SendDataAsync(CommandConstants.Logout, myUser.Encode());
        }

        public async Task AcquireGameAsync()
        {
            Game game = DialogUtils.SelectGame(await GetGamesAsync());
            if (game == null)
            {
                Console.WriteLine("Retorno al menú.");
                return;
            }

            await SendDataAsync(CommandConstants.AcquireGame, game.Encode());

            Console.WriteLine(await Utils.ClientReceiveMessageDataAsync(networkStream));
        }

        public async Task<List<Game>> GetGamesAsync()
        {
            await SendDataAsync(CommandConstants.GetGames, "");

            var gamesJson = await Utils.ClientReceiveMessageDataAsync(networkStream);
            List<Game> gameList = GameSystem.DecodeGames(gamesJson);
            return gameList;
        }

        public async Task ShowGamesAndDetailAsync(List<Game> games)
        {
            Game gameToShow = DialogUtils.SelectGame(games);

            if (gameToShow != null && !String.IsNullOrEmpty(gameToShow.Cover))
            {
                await ReceiveGameCoverAsync(gameToShow);
            }

            DialogUtils.ShowGameDetail(gameToShow);
        }

        public async Task ReceiveGameCoverAsync(Game g)
        {
            await SendDataAsync(CommandConstants.GetGameCover, g.Encode());
            
            await ReceiveFileAsync();
        }

        public async Task<List<Game>> GetAcquiredGamesAsync()
        {
            await SendDataAsync(CommandConstants.GetAcquiredGames, "");

            var gamesJson = await Utils.ClientReceiveMessageDataAsync(networkStream);

            return GameSystem.DecodeGames(gamesJson);
        }

        public async Task SendFileAsync(string path)
        {
            var fileCommunication = new FileCommunicationHandler(this.networkStream);
            await fileCommunication.SendFileAsync(path);
        }

        public async Task ReceiveFileAsync()
        {
            var fileCommunicationGameList = new FileCommunicationHandler(this.networkStream);
            await fileCommunicationGameList.ReceiveFileAsync();
        }

        public async Task PublishGameAsync()
        {
            Game gameToPublish = DialogUtils.InputGame();

            await SendDataAsync(CommandConstants.PublishGame, gameToPublish.Encode());

            if (File.Exists(gameToPublish.Cover))
            {
                await SendFileAsync(gameToPublish.Cover);
            }

            Console.WriteLine(await Utils.ClientReceiveMessageDataAsync(networkStream));
        }

        public async Task PublishReviewAsync()
        {
            Game game = DialogUtils.SelectGame(await GetGamesAsync());
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

            await SendDataAsync(CommandConstants.PublishReview, game.Encode());

            Console.WriteLine(await Utils.ClientReceiveMessageDataAsync(networkStream));
        }

        public async Task ModifyGameAsync()
        {
            List<Game> games = await GetGamesAsync();
            Game gameToModify = DialogUtils.SelectGame(games);

            if (gameToModify == null)
            {
                Console.WriteLine("Retorno al menú.");
                return;
            }

            await SendDataAsync(CommandConstants.ModifyingGame, gameToModify.Encode());

            List<string> headerAndMessage = await Utils.ReceiveCommandAndMessageAsync(networkStream);

            Console.WriteLine(headerAndMessage[1]);

            if (headerAndMessage[0] == CommandConstants.ModifyingGameError.ToString())
            {
                Console.WriteLine("Retorno al menú.");
                return;
            }

            Console.WriteLine("Ingrese los nuevos datos del juego. Si no quiere modificar el campo, presione ENTER.");
            Game modifiedGame = DialogUtils.InputGame();

            await SendDataAsync(CommandConstants.ModifyGame, GameSystem.EncodeGames(new List<Game>() { gameToModify, modifiedGame }));

            if (File.Exists(modifiedGame.Cover))
            {
                await SendFileAsync(modifiedGame.Cover);
            }

            Console.WriteLine(await Utils.ClientReceiveMessageDataAsync(networkStream));
        }

        public async Task DeleteGameAsync()
        {
            List<Game> games = await GetGamesAsync();

            Game gameToDelete = DialogUtils.SelectGame(games);
            if (gameToDelete == null)
            {
                Console.WriteLine("Retorno al menú.");
                return;
            }

            await SendDataAsync(CommandConstants.DeleteGame, gameToDelete.Encode());

            Console.WriteLine(await Utils.ClientReceiveMessageDataAsync(networkStream));
        }

        public async Task SendDataAsync(int command, string message)
        {
            var header = new Header(HeaderConstants.Request, command, message.Length);
            await Utils.ClientSendDataAsync(networkStream, header, message);
        }
    }
}